#!/usr/bin/env bash
# Build linux-x64 novolis_raylib_trace + novolis_imgui and stage with libraylib.so into Native/runtimes.
# Safe to run from WSL or native Linux. Downloads a user-local CMake if missing (no root required).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
RAYLIB_ROOT="$REPO_ROOT/codegen/pipeline/raylib6/steps/step_01_source/artifacts/raylib-6"
PREBUILT="$RAYLIB_ROOT/prebuilt/linux-x64"
HEADER_DIR="$RAYLIB_ROOT/include"
STAGE="$REPO_ROOT/src/Novolis.Raylib.Native/runtimes/linux-x64/native"
PLATFORM_DIR="$REPO_ROOT/codegen/native/raylib6-platform"
IMGUI_DIR="$REPO_ROOT/codegen/native/raylib6-with-imgui"

if [[ ! -f "$PREBUILT/libraylib.so" ]]; then
  echo "Missing $PREBUILT/libraylib.so — run step_01_source (linux prebuilt) first." >&2
  exit 1
fi
if [[ ! -f "$HEADER_DIR/raylib.h" ]]; then
  echo "Missing $HEADER_DIR/raylib.h — run step_01_source first." >&2
  exit 1
fi

ensure_cmake() {
  if command -v cmake >/dev/null 2>&1; then
    return 0
  fi
  local ver="3.30.5"
  local prefix="${HOME}/.local/novolis-cmake-${ver}"
  local tarball="cmake-${ver}-linux-x86_64.tar.gz"
  if [[ ! -x "$prefix/bin/cmake" ]]; then
    mkdir -p "$HOME/.local"
    local url="https://github.com/Kitware/CMake/releases/download/v${ver}/${tarball}"
    echo "Downloading portable CMake ${ver}..."
    curl -fsSL "$url" -o "/tmp/${tarball}"
    rm -rf "$prefix"
    mkdir -p "$prefix"
    tar -xzf "/tmp/${tarball}" -C "$prefix" --strip-components=1
  fi
  export PATH="$prefix/bin:$PATH"
  command -v cmake >/dev/null
}

ensure_cmake
command -v g++ >/dev/null || { echo "g++ is required" >&2; exit 1; }

build_shim() {
  local src="$1"
  local build="$src/build-linux"
  rm -rf "$build"
  mkdir -p "$build"
  cmake -S "$src" -B "$build" \
    -DRAYLIB_NATIVE_DIR="$PREBUILT" \
    -DRAYLIB_HEADER_DIR="$HEADER_DIR" \
    -DCMAKE_BUILD_TYPE=Release
  cmake --build "$build" --config Release -j"$(nproc 2>/dev/null || echo 2)"
}

build_shim "$PLATFORM_DIR"

mkdir -p "$STAGE"
cp -f "$PREBUILT/libraylib.so" "$STAGE/libraylib.so"
cp -f "$PLATFORM_DIR/out/libnovolis_raylib_trace.so" "$STAGE/libnovolis_raylib_trace.so"

set +e
build_shim "$IMGUI_DIR"
imgui_rc=$?
set -e
if [[ "$imgui_rc" -eq 0 ]]; then
  cp -f "$IMGUI_DIR/out/libnovolis_imgui.so" "$STAGE/libnovolis_imgui.so"
else
  echo "WARN: novolis_imgui linux build failed (missing GL/X11?). Staged libraylib + trace only." >&2
fi

chmod a+r "$STAGE"/* || true
echo "Staged linux-x64 natives:"
ls -la "$STAGE"
