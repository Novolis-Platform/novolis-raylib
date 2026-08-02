cmake_minimum_required(VERSION 3.24)
project(novolis_raylib_android_host C)

set(CMAKE_C_STANDARD 99)

if(NOT ANDROID)
  message(FATAL_ERROR "raylib6-android-host requires an Android NDK toolchain (ANDROID=1).")
endif()

if(NOT DEFINED RAYLIB_ANDROID_LIB)
  message(FATAL_ERROR "Set RAYLIB_ANDROID_LIB to libraylib.a (android-arm64 static build).")
endif()

if(NOT DEFINED RAYLIB_ANDROID_INCLUDE)
  message(FATAL_ERROR "Set RAYLIB_ANDROID_INCLUDE to raylib src/ (raylib.h).")
endif()

if(NOT EXISTS "${RAYLIB_ANDROID_LIB}")
  message(FATAL_ERROR "Missing RAYLIB_ANDROID_LIB: ${RAYLIB_ANDROID_LIB}")
endif()

if(NOT EXISTS "${RAYLIB_ANDROID_INCLUDE}/raylib.h")
  message(FATAL_ERROR "Missing raylib.h under RAYLIB_ANDROID_INCLUDE: ${RAYLIB_ANDROID_INCLUDE}")
endif()

add_library(novolis_raylib_android SHARED main.c)

target_include_directories(novolis_raylib_android PRIVATE
  "${RAYLIB_ANDROID_INCLUDE}"
  "${ANDROID_NDK}/sources/android/native_app_glue")

target_compile_definitions(novolis_raylib_android PRIVATE PLATFORM_ANDROID GRAPHICS_API_OPENGL_ES2)

# Match raylib Android shared-link flags (ANativeActivity entry + dynamic main lookup).
# Align ELF segments for Android 15+ 16 KB page size (XA0141).
target_link_options(novolis_raylib_android PRIVATE
  -Wl,--exclude-libs,libatomic.a
  -Wl,--build-id
  -Wl,-z,noexecstack
  -Wl,-z,relro
  -Wl,-z,now
  -Wl,-z,max-page-size=16384
  -u ANativeActivity_onCreate
  -Wl,--wrap=fopen)

target_link_libraries(novolis_raylib_android PRIVATE
  "${RAYLIB_ANDROID_LIB}"
  log
  android
  EGL
  GLESv2
  OpenSLES
  atomic
  m)

set_target_properties(novolis_raylib_android PROPERTIES
  OUTPUT_NAME "novolis_raylib_android"
  LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/out"
  LIBRARY_OUTPUT_DIRECTORY_RELEASE "${CMAKE_CURRENT_SOURCE_DIR}/out"
  LIBRARY_OUTPUT_DIRECTORY_DEBUG "${CMAKE_CURRENT_SOURCE_DIR}/out")
