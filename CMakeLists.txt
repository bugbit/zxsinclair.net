cmake_minimum_required(VERSION 3.8.0)

project(main VERSION 0.1.0)

option(BUILD_STATIC_LIBS "Build the static library" ON)

add_subdirectory(src)