cmake_minimum_required(VERSION 3.8.0)

project(test VERSION 0.1.0)

set(CMAKE_EXECUTABLE_SUFFIX ".html")

add_executable(test main.cpp regstest.cpp)

target_precompile_headers(test PUBLIC pch.h)
