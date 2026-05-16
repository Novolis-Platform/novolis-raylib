# AGENTS.md

Guidance for AI agents and contributors working in **novolis-raylib**.

## What this repo is

Multi-package **.NET 10** bindings for [raylib](https://www.raylib.com/) 6 and raygui, published as NuGet packages under the `Novolis.Raylib.*` id prefix. Bindings are **manifest-driven**: JSON under `pipeline/raylib6/` drives Roslyn codegen. **Interop** (`*.g.cs` in `Novolis.Raylib.Bindings`) and **public API** (façades, `Hud`, `Gui` in `Novolis.Raylib.Runtime`) are generated. Hand-crafted shell/debug live in `Runtime/` beside generated code.

**Consumer entry point:** `Novolis.Raylib` (meta package). Do not tell app authors to reference `Novolis.Raylib.Native` or `Novolis.Raylib.Abstractions` directly.

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
| `src/Novolis.Raylib.Testing/` | Offscreen harness, deterministic clock, hosting test host |
| `codegen/Novolis.Raylib.CodeGen/` | Roslyn CLI: `generate`, `verify`, `suggest-raylib`, `hooks list` (not published) |
| `codegen/Novolis.Raylib.CodeGen.Hooks/` | `IRaylibCodegenHook` implementations (not published) |
| `pipeline/raylib6/` | Manifests, fetch/native orchestration (`run.cs`) |
| `vendor/raylib-6/`, `vendor/raygui-6/` | Vendored headers + prebuilt `raylib.dll` (fetched) |
| `native/raylib6-with-raygui/` | CMake shim (`novolis_raygui.dll`) + trace forwarder |
| `build/` | MSBuild targets (codegen on compile, native copy) |
| `tests/` | TUnit unit/integration tests |
| `samples/` | HelloGame, HelloRuntime, HelloHosting, HelloTesting |
| `docs/` | `codegen.md`, `testing.md` |
| `scripts/` | `raylib-codegen-check.ps1`, `raylib-e2e.ps1`, `pack-all.ps1` |

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

Novolis.Raylib.Testing  → references Runtime + Game patterns (test projects only)
```

## Public APIs (where to implement features)

| Use case | API | Namespace examples |
|----------|-----|-------------------|
| Quick prototype / jam game | `RayGame.Run(title, w, h, loop)` | `Novolis.Raylib.Game`, `Novolis.Raylib.Colors` |
| Full app with DI / phases | `RaylibHost.CreateApplicationBuilder()` + `AddRaylib` / `AddRaylibSystem<T>()` | `Novolis.Raylib.Hosting`, implement `IRenderSystem`, `IUpdateSystem`, … |
| Low-level drawing without Game | `RaylibRuntimeShell.RunShellFrame` + `IRaylibFrameRenderer` | `Novolis.Raylib.Shell` |
| Drawing / scene | Static façades | `Graphics`, `World`, `Window`, `Input`, `Textures` in `Novolis.Raylib.Runtime` |
| HUD overlay (2D) | `Hud` | `Novolis.Raylib` (generated) |
| GUI widgets (raygui) | `Gui` | `Novolis.Raylib` (generated) |

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

# Windows: build novolis_raygui shim (needed for raygui + trace bridge)
dotnet run pipeline/raylib6/run.cs native

dotnet build Novolis.Raylib.slnx -c Release
dotnet test Novolis.Raylib.slnx -c Release --filter "Category!=Native" -- --maximum-parallel-tests 1
```

Native/offscreen tests are **opt-in** via environment variables (default CI does not set them):

| Variable | Purpose |
|----------|---------|
| `NOVOLIS_RAYLIB_OFFSCREEN_TESTS=1` | Offscreen harness |
| `NOVOLIS_RAYLIB_NATIVE_TESTS=1` | Load `raylib.dll` / window smoke |
| `NOVOLIS_RAYLIB_HEADLESS=1` | Skip blocking window loop (samples/CI) |
| `NOVOLIS_RAYLIB_DEBUG_CAPTURE=1` | Debug frame capture (see manifest) |

Full native E2E: `./scripts/raylib-e2e.ps1` (sets offscreen + native env, runs pipeline + CodeGen unit tests).

Pack all NuGet packages: `./scripts/pack-all.ps1` → `artifacts/`.

## CI (`.github/workflows/ci.yml`)

| Job | OS | What it checks |
|-----|-----|----------------|
| `codegen-drift` | ubuntu | Regenerate bindings; `git diff --exit-code src/Novolis.Raylib.Bindings/` |
| `build-test` | windows | fetch → native → build → test (non-Native filter) |
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

## What not to do

- Hand-edit `*.g.cs` under `src/Novolis.Raylib.Bindings/`.
- Add direct `PackageReference` to `Novolis.Raylib.Native` or `Novolis.Raylib.Abstractions` in application projects.
- Commit secrets or local IDE-only config (`.idea/` is untracked noise).
- Assume Linux CI runs native window tests — use env gates or Windows locally.
