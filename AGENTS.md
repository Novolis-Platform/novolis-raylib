# AGENTS.md

Guidance for AI agents and contributors working in **novolis-raylib**.

## What this repo is

Multi-package **.NET 10** bindings for [raylib](https://www.raylib.com/) 6 and raygui, published as NuGet packages under the `Novolis.Raylib.*` id prefix. Bindings are **manifest-driven**: JSON under `pipeline/raylib6/` drives Roslyn codegen. **Interop** (`*.g.cs` in `Novolis.Raylib.Bindings`) and **public API** (façades, `Hud`, `Gui` in `Novolis.Raylib.Runtime`) are generated. Hand-crafted shell/debug live in `Runtime/` beside generated code.

**Consumer entry point:** `Novolis.Raylib` (meta package). Do not tell app authors to reference `Novolis.Raylib.Native` or `Novolis.Raylib.Abstractions` directly.

## Agentic tools (read first)

| Resource | Purpose |
|----------|---------|
| [agentic-tools/README.md](agentic-tools/README.md) | Tool catalog and rules for agents |
| [agentic-tools/registry.json](agentic-tools/registry.json) | Machine-readable commands and workflows |
| [agentic-tools/workflows/codegen.md](agentic-tools/workflows/codegen.md) | Step-by-step codegen |
| `.cursor/rules/` | Cursor rules (codegen, generated files) |
| `.cursor/hooks.json` | Blocks direct edits to generated `*.g.cs` |

**Before finishing binding/manifest work:**

```bash
dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
pwsh ./scripts/agent-verify.ps1
```

## Repository layout

| Path | Role |
|------|------|
| `src/Novolis.Raylib/` | Meta package — install this in games/apps |
| `src/Novolis.Raylib.Bindings/` | Generated interop (`Raylib6Native`), shared types (`Camera`, `Texture`, …) |
| `src/Novolis.Raylib.Runtime/` | Generated façades (`Graphics`, `World`, `Hud`, `Gui`, …) + hand-crafted shell, logging, debug |
| `src/Novolis.Raylib.Native/` | Native DLL assets per RID (transitive; not C# bindings) |
| `src/Novolis.Raylib.Game/` | `RayGame.Run` jam API |
| `src/Novolis.Raylib.Hosting/` | `IHost` + phased game-loop systems |
| `src/Novolis.Raylib.Abstractions/` | `IRaylibFrameRenderer`, `IRenderSystem`, … (transitive) |
| `src/Novolis.Raylib.Capture/` | Optional packable per-frame framebuffer streaming (`FrameCaptureSession`) |
| `src/Novolis.Raylib.Testing/` | Offscreen harness, golden QA API, deterministic clock (test projects only) |
| `codegen/Novolis.Raylib.CodeGen/` | Roslyn CLI: `generate`, `verify`, `suggest-raylib`, `hooks list` (not published) |
| `codegen/Novolis.Raylib.CodeGen.Hooks/` | `IRaylibCodegenHook` implementations (not published) |
| `pipeline/raylib6/` | Manifests, fetch/native orchestration (`run.cs`) |
| `vendor/raylib-6/`, `vendor/raygui-6/`, `vendor/raylib-cimgui/` | Vendored raylib prebuilts, raygui header, ray-cimgui + bundled cimgui (fetched) |
| `native/raylib6-platform/` | Trace forwarder (`novolis_raylib_trace`) |
| `native/raylib6-with-imgui/` | ImGui shim (`novolis_imgui.dll`) |
| `native/raylib6-with-raygui/` | RayGui add-on shim (`novolis_raygui.dll`) |
| `src/Novolis.Raylib.Raygui/` | Optional raygui widgets add-on package |
| `build/` | MSBuild targets (codegen on compile, native copy) |
| `tests/` | TUnit unit/integration tests |
| `samples/` | HelloGame, HelloRuntime, HelloHosting, HelloCapture, HelloTesting |
| `docs/` | `codegen.md`, `testing.md` |
| `scripts/` | `agent-verify.ps1`, `raylib-codegen-check.ps1`, `raylib-e2e.ps1`, `pack-all.ps1` |
| `agentic-tools/` | Agent registry, workflows (codegen discipline) |

Solution file: `Novolis.Raylib.slnx`. Central package versions: `Directory.Packages.props`.

**`src/`** — publishable NuGet packages only. **`codegen/`** — build-time tooling (never published).

## Package dependency graph

```
Novolis.Raylib
├── Novolis.Raylib.Game
├── Novolis.Raylib.Hosting
├── Novolis.Raylib.Runtime  ← generated API + shell
│   ├── Novolis.Raylib.Bindings  ← generated interop (codegen triggered from Bindings build)
│   │   └── Novolis.Raylib.Native (DLL assets only)
│   └── Novolis.Raylib.Abstractions
└── (transitive via Runtime)

Novolis.Raylib.Capture  → Runtime (optional; not in meta package)

Novolis.Raylib.Testing  → Novolis.Raylib + Capture (test projects only)
```

## Public APIs (where to implement features)

| Use case | API | Namespace examples |
|----------|-----|-------------------|
| Quick prototype / jam game | `RayGame.Run(title, w, h, loop)` | `Novolis.Raylib.Game`, `Novolis.Raylib.Colors` |
| Full app with DI / phases | `RaylibHost.CreateApplicationBuilder()` + `AddRaylib` / `AddRaylibSystem<T>()` | `Novolis.Raylib.Hosting`, implement `IRenderSystem`, `IUpdateSystem`, … |
| Low-level drawing without Game | `RaylibRuntimeShell.RunShellFrame` + `IRaylibFrameRenderer` | `Novolis.Raylib.Shell` |
| Drawing / scene | Static façades | `Graphics`, `World`, `Window`, `Input`, `Textures` in `Novolis.Raylib.Runtime` |
| HUD overlay (2D) | `Hud` | `Novolis.Raylib` (generated) |
| GUI widgets (Dear ImGui) | `Gui` | `Novolis.Raylib` (generated) |
| GUI widgets (raygui, optional) | `RayGui` | `Novolis.Raylib.Raygui` add-on |

Hand-written shell code lives in `Runtime/` (`Shell/`, `Debug/`, …). Shared types used by façades (`Color`, `Vector3`, …) live in `Bindings/`. **Do not hand-edit `*.g.cs`.**

## Codegen rules (read before touching bindings)

1. **Source of truth:** `pipeline/raylib6/*.manifest.json` (exports, raygui, debug hooks, façades).
2. **Generated output:** `src/Novolis.Raylib.Bindings/Interop/*.g.cs` and `src/Novolis.Raylib.Runtime/**/*.g.cs` — each file has `ManifestSha256` in the header.
3. **Regenerate:**
   ```bash
   dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate
   ```
   Or full maintainer pipeline:
   ```bash
   dotnet run pipeline/raylib6/run.cs all   # fetch + native + generate
   ```
4. **Extend behavior:** add or change hooks in `codegen/Novolis.Raylib.CodeGen.Hooks/` (see `pipeline/raylib6/hooks/README.md`).
5. **Drift check (CI/local):**
   ```powershell
   ./scripts/raylib-codegen-check.ps1
   ```
   CI job `codegen-drift` runs the same check on Ubuntu.
6. **MSBuild:** `build/Novolis.Raylib.CodeGen.targets` runs codegen before `CoreCompile` on `Novolis.Raylib.Bindings` when manifests/hooks change. Packing sets `RunRaylibCodegen=false`.

To add a raylib export: update `raylib-exports.manifest.json` (and façade manifest if needed), run `generate`, commit both manifest and `*.g.cs` changes together.

## Build and test

**Prerequisites:** .NET 10 SDK. For native shim: CMake + platform toolchain (Windows CI uses MSVC).

```bash
# Fetch vendor headers/DLL (required before first build)
dotnet run pipeline/raylib6/fetch-sources.cs

# Windows: build native shims (trace, ImGui, optional raygui)
dotnet run pipeline/raylib6/run.cs native

dotnet build Novolis.Raylib.slnx -c Release
dotnet test Novolis.Raylib.slnx -c Release --filter "Category!=Native" -- --maximum-parallel-tests 1
```

**Golden image tests** (preferred for visuals): `tests/Novolis.Raylib.Golden`, `[Category("Golden")]`, no env vars — use `RaylibTestRuntime.EnableForAssembly()`. CI job `golden-tests` on Windows. See [docs/testing.md](docs/testing.md). Scripts: `./scripts/run-golden-tests.ps1`, `./scripts/seed-golden-baselines.ps1`.

Legacy native/offscreen tests may still use environment variables (default unit CI does not set them):

| Variable | Purpose |
|----------|---------|
| `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` | Legacy offscreen harness |
| `NOVOLIS_RAYLIB_NATIVE_TESTS=1` | Legacy native smoke |
| `NOVOLIS_RAYLIB_HEADLESS=1` | Skip blocking window loop (samples/CI) |
| `NOVOLIS_RAYLIB_DEBUG_CAPTURE=1` | Debug frame capture in Bindings (not golden CI) |

Full native E2E: `./scripts/raylib-e2e.ps1` (sets offscreen + native env, runs pipeline + CodeGen unit tests).

Pack all NuGet packages: `./scripts/pack-all.ps1` → `artifacts/`.

## CI (`.github/workflows/ci.yml`)

| Job | OS | What it checks |
|-----|-----|----------------|
| `codegen-drift` | ubuntu | Regenerate bindings; `git diff --exit-code` on Bindings + Runtime |
| `build-test` | windows | fetch → native → build → test (non-Native filter) |
| `golden-tests` | windows | fetch → native → build → `[Category("Golden")]`; uploads `temp/test-renders/` on failure |
| `pack-smoke` | ubuntu | `dotnet pack` after build-test |

## Coding conventions

- **TFM:** `net10.0`, `LangVersion=latest`, nullable enabled, **warnings as errors** (`Directory.Build.props`).
- **Tests:** TUnit (`TestingPlatformDotnetTestSupport`), under `tests/`.
- **Namespaces:** `Novolis.Raylib.*` (interop types in `Novolis.Raylib.Interop`, internal `Raylib6Native`).
- **Bindings project:** `AllowUnsafeBlocks`, `DisableRuntimeMarshalling` — string interop uses UTF-8 marshalling; respect `interopPolicy` in manifests for `SuppressGCTransition`.
- **Scope discipline:** Only change files required by the task. Do not drive-by refactor unrelated packages or stale pipeline scripts.

## Stale references in the tree

Some comments and help text still mention **Star Conflicts Revolt**, `tools/raylib6-pipeline/`, or `StarConflictsRevolt.Raylib6.Bindings`. The live paths are `pipeline/raylib6/` and `src/Novolis.Raylib.*`. Prefer updating call sites to current paths when you touch those files; do not copy stale paths into new code.

## Common agent tasks

| Task | Approach |
|------|----------|
| Add raylib function | Manifest row → `generate` → commit `.g.cs` + manifest |
| Add façade method | `facades.manifest.json` → `generate` |
| Customize emitted C# | New `IRaylibCodegenHook` in CodeGen.Hooks |
| New sample | `samples/Hello*` pattern; reference `Novolis.Raylib` |
| Game loop with systems | `HelloHosting` sample + `IStartupSystem` / `IRenderSystem` / … |
| Test rendering | `Novolis.Raylib.Testing` + env gates; see `docs/testing.md` |

## Docs to read first

- [README.md](README.md) — install and quick start
- [docs/codegen.md](docs/codegen.md) — codegen overview
- [docs/testing.md](docs/testing.md) — test helpers and env gates
- Per-package `README.md` under `src/Novolis.Raylib.*/`

## Git remote (Novolis-Platform)

This repo lives under `d:\novolis\` and **`origin` must be the org repo**:

- `https://github.com/Novolis-Platform/novolis-raylib.git`

Before the first `git push` or `gh repo create` in any `novolis-*` repo:

1. Run `git remote -v` and confirm `origin` points at `Novolis-Platform/`.
2. Never create a personal-repo remote for reserved `novolis-*` names.
3. Use `gh repo create --org Novolis-Platform` when bootstrapping a new org repo.

## What not to do

- Hand-edit `*.g.cs` under `src/Novolis.Raylib.Bindings/Interop/` or `src/Novolis.Raylib.Runtime/`.
- Add direct `PackageReference` to `Novolis.Raylib.Native` or `Novolis.Raylib.Abstractions` in application projects.
- Commit secrets or local IDE-only config (`.idea/` is untracked noise).
- Assume Linux CI runs native window tests — use env gates or Windows locally.
