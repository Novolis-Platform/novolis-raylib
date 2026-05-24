cmake_minimum_required(VERSION 3.24)
project(novolis_raylib_platform C)

set(CMAKE_C_STANDARD 99)

if(NOT DEFINED RAYLIB_NATIVE_DIR)
  get_filename_component(_REPO "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)
  set(RAYLIB_NATIVE_DIR "${_REPO}/pipeline/raylib6/steps/step_01_source/artifacts/raylib-6/prebuilt/win-x64" CACHE PATH "Directory containing raylib.dll (run pipeline step_01_source).")
endif()

get_filename_component(REPO_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)
set(RAYLIB_HEADER_DIR "${REPO_ROOT}/pipeline/raylib6/steps/step_01_source/artifacts/raylib-6/include")

if(NOT EXISTS "${RAYLIB_HEADER_DIR}/raylib.h")
    message(FATAL_ERROR "Missing ${RAYLIB_HEADER_DIR}/raylib.h — run: dotnet run --project codegen/Novolis.Raylib.Pipeline -- run step_01_source")
endif()

add_library(novolis_raylib_trace SHARED trace_shim.c)
set_target_properties(novolis_raylib_trace PROPERTIES OUTPUT_NAME "novolis_raylib_trace")

target_include_directories(novolis_raylib_trace PRIVATE "${RAYLIB_HEADER_DIR}")

if(WIN32)
  if(NOT EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/../raylib6-with-raygui/generated/raylib.win-x64.def")
    message(FATAL_ERROR "Missing raylib.win-x64.def — run pipeline/raylib6/run.cs native (raygui shim generates .def) or generate-raylib-windows-def.cs")
  endif()
  get_filename_component(_MSVC_BIN_DIR "${CMAKE_C_COMPILER}" DIRECTORY)
  find_program(LIB_TOOL NAMES lib lib.exe PATHS "${_MSVC_BIN_DIR}" REQUIRED NO_DEFAULT_PATH)
  set(RAYLIB_IMPLIB "${CMAKE_BINARY_DIR}/raylib_import.lib")
  set(_RAYLIB_DEF "${CMAKE_CURRENT_SOURCE_DIR}/../raylib6-with-raygui/generated/raylib.win-x64.def")
  add_custom_command(
    OUTPUT "${RAYLIB_IMPLIB}"
    COMMAND "${LIB_TOOL}" "/def:${_RAYLIB_DEF}" "/out:${RAYLIB_IMPLIB}" /machine:x64
    DEPENDS "${_RAYLIB_DEF}")
  add_custom_target(raylib_import_lib_platform DEPENDS "${RAYLIB_IMPLIB}")
  add_library(raylib_imp_platform STATIC IMPORTED)
  set_target_properties(raylib_imp_platform PROPERTIES IMPORTED_LOCATION "${RAYLIB_IMPLIB}")
  add_dependencies(raylib_imp_platform raylib_import_lib_platform)
  add_dependencies(novolis_raylib_trace raylib_import_lib_platform)
  target_link_libraries(novolis_raylib_trace PRIVATE raylib_imp_platform)
  set(_OUT "${CMAKE_CURRENT_SOURCE_DIR}/out")
  set_target_properties(novolis_raylib_trace PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_RELWITHDEBINFO "${_OUT}"
    RUNTIME_OUTPUT_DIRECTORY_MINSIZEREL "${_OUT}")
else()
  if(NOT EXISTS "${RAYLIB_NATIVE_DIR}/libraylib.so")
    message(FATAL_ERROR "Missing ${RAYLIB_NATIVE_DIR}/libraylib.so — vendor Linux prebuilt or set RAYLIB_NATIVE_DIR.")
  endif()
  target_link_libraries(novolis_raylib_trace PRIVATE "${RAYLIB_NATIVE_DIR}/libraylib.so")
  set_target_properties(novolis_raylib_trace PROPERTIES LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/out")
endif()
