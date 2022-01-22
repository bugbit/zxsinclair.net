cmake_minimum_required(VERSION 3.8.0)

project(test VERSION 0.1.0)

set(CMAKE_EXECUTABLE_SUFFIX ".html")

include_directories(../src)
add_executable(test main.cpp regstest.cpp z80opcodestest.cpp
../src/z80_core.cpp ../src/z80_opcodes.cpp)
#add_executable(test ../src/z80_core.cpp)

target_precompile_headers(test PUBLIC pch.h)
