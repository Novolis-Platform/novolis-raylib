cmake_minimum_required(VERSION 3.24)
project(novolis_imgui_raylib6 C CXX)

set(CMAKE_C_STANDARD 99)
set(CMAKE_CXX_STANDARD 11)

if(NOT DEFINED RAYLIB_NATIVE_DIR)
  get_filename_component(_REPO "${CMAKE_CURRENT_SOURCE_DIR}/../../.." ABSOLUTE)
  set(RAYLIB_NATIVE_DIR "${_REPO}/codegen/pipeline/raylib6/steps/step_01_source/artifacts/raylib/prebuilt/win-x64" CACHE PATH "Directory containing raylib.dll")
endif()

get_filename_component(REPO_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../../.." ABSOLUTE)
set(RAYLIB_HEADER_DIR "${REPO_ROOT}/codegen/pipeline/raylib6/steps/step_01_source/artifacts/raylib/include")
set(RAYLIB_CIMGUI_PATH "${REPO_ROOT}/codegen/pipeline/raylib6/steps/step_01_source/artifacts/raylib-cimgui")
if(NOT DEFINED CIMGUI_PATH)
  file(GLOB _CIMGUI_CANDIDATES LIST_DIRECTORIES true "${RAYLIB_CIMGUI_PATH}/cimgui-*")
  set(_CIMGUI_DIRECTORIES)
  foreach(_candidate IN LISTS _CIMGUI_CANDIDATES)
    if(IS_DIRECTORY "${_candidate}" AND EXISTS "${_candidate}/CMakeLists.txt")
      list(APPEND _CIMGUI_DIRECTORIES "${_candidate}")
    endif()
  endforeach()
  list(SORT _CIMGUI_DIRECTORIES COMPARE NATURAL ORDER DESCENDING)
  if(_CIMGUI_DIRECTORIES)
    list(GET _CIMGUI_DIRECTORIES 0 CIMGUI_PATH)
  else()
    message(FATAL_ERROR "No cimgui source directory found under ${RAYLIB_CIMGUI_PATH} — run step_01_source to fetch the pinned source.")
  endif()
endif()

if(NOT EXISTS "${RAYLIB_HEADER_DIR}/raylib.h")
    message(FATAL_ERROR "Missing ${RAYLIB_HEADER_DIR}/raylib.h — run: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source")
endif()
if(NOT EXISTS "${CIMGUI_PATH}/cimgui.h")
    message(FATAL_ERROR "Missing ${CIMGUI_PATH}/cimgui.h — run step_01_source to fetch the pinned cimgui and ImGui sources")
endif()
if(NOT EXISTS "${RAYLIB_CIMGUI_PATH}/src/raycimgui.c")
    message(FATAL_ERROR "Missing ${RAYLIB_CIMGUI_PATH}/src/raycimgui.c — run step_01_source to fetch the pinned ray-cimgui source")
endif()

set(IMGUI_STATIC "yes" CACHE STRING "" FORCE)
add_subdirectory("${CIMGUI_PATH}" "${CMAKE_BINARY_DIR}/cimgui-build" EXCLUDE_FROM_ALL)
if(TARGET cimgui)
  set_target_properties(cimgui PROPERTIES POSITION_INDEPENDENT_CODE ON)
endif()

add_library(novolis_imgui SHARED
    novolis_imgui_exports.cpp
    "${RAYLIB_CIMGUI_PATH}/src/raycimgui.c"
)
set_target_properties(novolis_imgui PROPERTIES OUTPUT_NAME "novolis_imgui")

target_compile_definitions(novolis_imgui PRIVATE NO_FONT_AWESOME BUILD_LIBTYPE_SHARED)
target_include_directories(novolis_imgui PRIVATE
    "${RAYLIB_HEADER_DIR}"
    "${CIMGUI_PATH}"
    "${CIMGUI_PATH}/imgui"
    "${RAYLIB_CIMGUI_PATH}/src")
set(RAYCIMGUI_SOURCE "${RAYLIB_CIMGUI_PATH}/src/raycimgui.c")
if(MSVC)
  set_source_files_properties("${RAYCIMGUI_SOURCE}" PROPERTIES
    COMPILE_OPTIONS "/FI${CMAKE_CURRENT_SOURCE_DIR}/raycimgui_compat.h")
else()
  set_source_files_properties("${RAYCIMGUI_SOURCE}" PROPERTIES
    COMPILE_OPTIONS "-include;${CMAKE_CURRENT_SOURCE_DIR}/raycimgui_compat.h")
endif()

if(WIN32)
  if(NOT EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/../raylib6-with-raygui/generated/raylib.win-x64.def")
    message(FATAL_ERROR "Missing raylib.win-x64.def — run: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_02_native")
  endif()
  get_filename_component(_MSVC_BIN_DIR "${CMAKE_C_COMPILER}" DIRECTORY)
  find_program(LIB_TOOL NAMES lib lib.exe PATHS "${_MSVC_BIN_DIR}" REQUIRED NO_DEFAULT_PATH)
  set(RAYLIB_IMPLIB "${CMAKE_BINARY_DIR}/raylib_import.lib")
  set(_RAYLIB_DEF "${CMAKE_CURRENT_SOURCE_DIR}/../raylib6-with-raygui/generated/raylib.win-x64.def")
  add_custom_command(
    OUTPUT "${RAYLIB_IMPLIB}"
    COMMAND "${LIB_TOOL}" "/def:${_RAYLIB_DEF}" "/out:${RAYLIB_IMPLIB}" /machine:x64
    DEPENDS "${_RAYLIB_DEF}")
  add_custom_target(raylib_import_lib_imgui DEPENDS "${RAYLIB_IMPLIB}")
  add_library(raylib_imp_imgui STATIC IMPORTED)
  set_target_properties(raylib_imp_imgui PROPERTIES IMPORTED_LOCATION "${RAYLIB_IMPLIB}")
  add_dependencies(raylib_imp_imgui raylib_import_lib_imgui)
  add_dependencies(novolis_imgui raylib_import_lib_imgui)
  target_link_libraries(novolis_imgui PRIVATE cimgui raylib_imp_imgui opengl32 gdi32 winmm)
  set(_OUT "${CMAKE_CURRENT_SOURCE_DIR}/out")
  set_target_properties(novolis_imgui PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL "${_OUT}")
else()
  if(NOT EXISTS "${RAYLIB_NATIVE_DIR}/libraylib.so")
    message(FATAL_ERROR "Missing ${RAYLIB_NATIVE_DIR}/libraylib.so")
  endif()
  # libraylib.so already links X11/GL; allow unresolved at shim link time so WSL/minimal hosts can build.
  target_link_libraries(novolis_imgui PRIVATE cimgui "${RAYLIB_NATIVE_DIR}/libraylib.so")
  target_link_options(novolis_imgui PRIVATE "LINKER:--allow-shlib-undefined")
  set_target_properties(novolis_imgui PROPERTIES LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/out")
endif()
