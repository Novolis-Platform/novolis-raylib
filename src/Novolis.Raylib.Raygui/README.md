<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-raylib">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Raylib.Raygui

Optional add-on: **raygui** rectangle widgets via `RayGuiControls` and the legacy `RayGui` façade.

Install alongside `Novolis.Raylib` when you need raygui-style controls. Core `Gui` in Runtime uses Dear ImGui.

## Install

```bash
dotnet add package Novolis.Raylib
dotnet add package Novolis.Raylib.Raygui
```

## Quick start

```csharp
using Novolis.Raylib.Raygui;

RayguiShimHost.EnsureInitialized();
if (RayGuiControls.Button(new RectangleF(8, 8, 120, 28), "Play"))
    /* clicked */;
```

Call `RayguiShimHost.EnsureInitialized()` once before any widget. Native `novolis_raygui` is transitive via `Novolis.Raylib.Raygui.Native`.

## API

| Type | Role |
|------|------|
| `RayguiShimHost` | `EnsureInitialized()` |
| `RayGuiControls` | `Button`, `Label`, `Panel`, `Toggle`, `CheckBox`, `ComboBox`, `Slider`, `ProgressBar`, style helpers |
| `RayGui` | Legacy static partial façade (same widgets) |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Raylib.Runtime` | Primary `Gui` (ImGui) and shell loop |
| `Novolis.Raylib.Raygui.Native` | Native raygui binaries (transitive) |

