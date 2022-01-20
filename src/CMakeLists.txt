cmake_minimum_required(VERSION 3.8.0)

project(zxsinclair VERSION 0.1.0)

option(BUILD_STATIC_LIBS "Build the static library" ON)

set(CMAKE_EXECUTABLE_SUFFIX ".html")

add_executable(zxsinclair main.cpp)

target_precompile_headers(zxsinclair PUBLIC pch.h)
