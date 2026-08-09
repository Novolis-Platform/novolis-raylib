# Release

This repository publishes with the org CalVer scheme (`2026.1.*`) via `merge.yml` to GitHub Packages when packages are packable.

See [release-policy](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/release-policy.md).

Published docs: [https://novolis-platform.github.io/.github/novolis-raylib/](https://novolis-platform.github.io/.github/novolis-raylib/)

## Packages

- `Novolis.Raylib`
- `Novolis.Raylib.Abstractions`
- `Novolis.Raylib.Bindings`
- `Novolis.Raylib.Capture`
- `Novolis.Raylib.Game`
- `Novolis.Raylib.Hosting`
- `Novolis.Raylib.Input`
- `Novolis.Raylib.Loaders`
- `Novolis.Raylib.Manifests`
- `Novolis.Raylib.Native`
- `Novolis.Raylib.Raygui`
- `Novolis.Raylib.Raygui.Native`
- `Novolis.Raylib.Runtime`
- `Novolis.Raylib.Testing`

## Consumers

Restore from nuget.org + `https://nuget.pkg.github.com/Novolis-Platform/index.json` only.

Local multi-repo iteration: open `d:\novolis\Novolis.Platform.slnx` (ProjectReference mode) — do not add a local feed.
