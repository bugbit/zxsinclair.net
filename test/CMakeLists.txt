cmake_minimum_required(VERSION 3.8.0)

project(test VERSION 0.1.0)

set(CMAKE_EXECUTABLE_SUFFIX ".html")

include_directories(../src)
add_executable(test main.cpp regstest.cpp z80opcodestest.cpp
)
#add_executable(test ../src/z80_core.cpp)
add_definitions( -DZ80_OPCODES_TEST )
if (EMSCRIPTEN)
    set_target_properties(
        test
        PROPERTIES
        LINK_FLAGS
        "--preload-file ../../test/assets/@"
    ) 
endif(EMSCRIPTEN)

target_precompile_headers(test PUBLIC pch.h)
