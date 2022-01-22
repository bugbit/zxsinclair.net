cmake_minimum_required(VERSION 3.8.0)

project(zxsinclair VERSION 0.1.0)

set(CMAKE_EXECUTABLE_SUFFIX ".html")

add_executable(zxsinclair main.cpp z80_core.cpp z80_opcodes.cpp)

target_precompile_headers(zxsinclair PUBLIC pch.h)
