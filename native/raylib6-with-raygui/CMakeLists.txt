cmake_minimum_required(VERSION 3.24)
project(novolis_raygui_raylib6 C)

set(CMAKE_C_STANDARD 99)

if(NOT DEFINED RAYLIB_NATIVE_DIR)
  get_filename_component(_REPO "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)
  set(RAYLIB_NATIVE_DIR "${_REPO}/vendor/raylib-6/prebuilt/win-x64" CACHE PATH "Directory containing raylib.dll (run pipeline/raylib6/fetch-sources.cs).")
endif()

get_filename_component(REPO_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)
set(RAYGUI_HEADER_DIR "${REPO_ROOT}/vendor/raygui-6")
set(RAYLIB_HEADER_DIR "${REPO_ROOT}/vendor/raylib-6/include")

if(NOT EXISTS "${RAYLIB_HEADER_DIR}/raylib.h")
    message(FATAL_ERROR "Missing ${RAYLIB_HEADER_DIR}/raylib.h — run: dotnet run pipeline/raylib6/fetch-sources.cs")
endif()

add_library(novolis_raygui SHARED raygui_shim.c trace_shim.c)
set_target_properties(novolis_raygui PROPERTIES OUTPUT_NAME "novolis_raygui")

# raygui.h only sets RAYGUIAPI __declspec(dllexport) when building a shared lib on Windows.
target_compile_definitions(novolis_raygui PRIVATE BUILD_LIBTYPE_SHARED)

target_include_directories(novolis_raygui PRIVATE "${RAYLIB_HEADER_DIR}" "${RAYGUI_HEADER_DIR}")

if(WIN32)
  if(NOT EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/generated/raylib.win-x64.def")
    message(FATAL_ERROR "Missing generated/raylib.win-x64.def — run fetch then: dotnet run pipeline/raylib6/run.cs native (regenerates .def when raylib.dll is present), or: dotnet run pipeline/raylib6/generate-raylib-windows-def.cs -- <path-to-raylib.dll> native/raylib6-with-raygui/generated/raylib.win-x64.def")
  endif()
  get_filename_component(_MSVC_BIN_DIR "${CMAKE_C_COMPILER}" DIRECTORY)
  find_program(LIB_TOOL NAMES lib lib.exe PATHS "${_MSVC_BIN_DIR}" REQUIRED NO_DEFAULT_PATH)
  set(RAYLIB_IMPLIB "${CMAKE_BINARY_DIR}/raylib_import.lib")
  set(_RAYLIB_DEF "${CMAKE_CURRENT_SOURCE_DIR}/generated/raylib.win-x64.def")
  add_custom_command(
    OUTPUT "${RAYLIB_IMPLIB}"
    COMMAND "${LIB_TOOL}" "/def:${_RAYLIB_DEF}" "/out:${RAYLIB_IMPLIB}" /machine:x64
    DEPENDS "${_RAYLIB_DEF}")
  add_custom_target(raylib_import_lib DEPENDS "${RAYLIB_IMPLIB}")
  add_library(raylib_imp STATIC IMPORTED)
  set_target_properties(raylib_imp PROPERTIES IMPORTED_LOCATION "${RAYLIB_IMPLIB}")
  add_dependencies(raylib_imp raylib_import_lib)
  add_dependencies(novolis_raygui raylib_import_lib)
  target_link_libraries(novolis_raygui PRIVATE raylib_imp)
  set(_OUT "${CMAKE_CURRENT_SOURCE_DIR}/out")
  set_target_properties(novolis_raygui PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL "${_OUT}")
else()
  if(NOT EXISTS "${RAYLIB_NATIVE_DIR}/libraylib.so")
    message(FATAL_ERROR "Missing ${RAYLIB_NATIVE_DIR}/libraylib.so — vendor Linux prebuilt or set RAYLIB_NATIVE_DIR.")
  endif()
  target_link_libraries(novolis_raygui PRIVATE "${RAYLIB_NATIVE_DIR}/libraylib.so")
  set_target_properties(novolis_raygui PROPERTIES LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/out")
endif()
