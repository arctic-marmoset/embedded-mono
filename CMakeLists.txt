cmake_minimum_required(VERSION 3.14 FATAL_ERROR)
project(CsInterop)

set(CMAKE_CXX_STANDARD 20)

string(TOUPPER ${CMAKE_BUILD_TYPE} CMAKE_BUILD_TYPE_UPPER)
if(CMAKE_BUILD_TYPE_UPPER STREQUAL "RELEASE")
    include(CheckIPOSupported)
    check_ipo_supported(RESULT IPO_IS_SUPPORTED)
    if(IPO_IS_SUPPORTED)
        set(CMAKE_INTERPROCEDURAL_OPTIMIZATION ON)
    endif()
endif()

option(CSINTEROP_USE_EXTENSIVE_WARNINGS "Enable additional compiler warnings" ON)

if(MSVC)
    string(REGEX REPLACE "[-/]W[1-4]" "" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
    set(CSINTEROP_WARN_FLAGS /W4 /permissive-)
else()
    set(CSINTEROP_WARN_FLAGS -Wall -Wextra -Wpedantic)
endif()

if(CSINTEROP_USE_EXTENSIVE_WARNINGS)
    if(MSVC)
        list(APPEND CSINTEROP_WARN_FLAGS
            /w14242 # possible loss of data: conversion from `type1` to `type2`
            /w14254 # possible loss of data: conversion from `type1:field_bits`
                    # to `type2:field_bits`
            /w14263 # member function does not override any base class virtual
                    # member function
            /w14265 # class has virtual functions, but destructor is not
                    # virtual; instances of this class may not be destructed
                    # correctly
            /w14287 # unsigned/negative constant mismatch
            /we4289 # loop control variable declared in the for-loop is used
                    # outside the for-loop scope
            /w14296 # expression is always [true | false]
            /w14311 # pointer truncation from `type1` to `type2`
            /w14545 # expression before comma evaluates to a function which is
                    # missing an argument list
            /w14546 # function call before comma missing argument list
            /w14547 # operator before comma has no effect; expected operator
                    # with side-effect
            /w14549 # operator before comma has no effect; did you intend
                    # 'operator'?
            /w14555 # expression has no effect; expected expression with
                    # side-effect
            /w14619 # pragma warning: there is no warning number `number`
            /w14640 # thread un-safe static member initialization
            /w14826 # conversion from 'type1' to 'type_2' is sign-extended which
                    # may cause unexpected runtime behavior
            /w14928 # illegal copy-initialization; more than one user-defined
                    # conversion has been implicitly applied
        )
    else()
        list(APPEND CSINTEROP_WARN_FLAGS
            -Wshadow
            -Wnon-virtual-dtor
            -Wold-style-cast
            -Wcast-align
            -Wunused
            -Woverloaded-virtual
            -Wconversion
            -Wsign-conversion
            -Wnull-dereference
            -Wformat=2
            -Wfloat-equal
        )
    endif()

    if(CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
        list(APPEND CSINTEROP_WARN_FLAGS
            -Wduplicated-cond
            -Wduplicated-branches
            -Wlogical-op
            -Wuseless-cast
        )
    elseif(CMAKE_CXX_COMPILER_ID MATCHES ".*Clang")
        list(APPEND CSINTEROP_WARN_FLAGS
            -Wshorten-64-to-32
        )
    endif()
endif()

find_path(NETHOST_INCLUDE_DIRS nethost.h)
find_library(NETHOST_LIBRARY NAMES libnethost nethost)

find_package(fmt           CONFIG REQUIRED)
find_package(Microsoft.GSL CONFIG REQUIRED)

# TODO: Fix search method for Mono. This is just a temporary solution.
set(MONO_ROOT $ENV{MONO_ROOT})
cmake_path(CONVERT $ENV{MONO_ROOT} TO_CMAKE_PATH_LIST MONO_ROOT)
set(MONO_INCLUDE_DIRS ${MONO_ROOT}/include/mono-2.0)
set(MONO_LIBRARY_DIR ${MONO_ROOT}/lib)
set(MONO_LIBRARIES
    ${MONO_LIBRARY_DIR}/mono-2.0-sgen.lib
    ws2_32
    psapi
    ole32
    winmm
    oleaut32
    advapi32
    version
)

message(STATUS "MONO_ROOT=${MONO_ROOT}")

add_executable(csinterop
    src/main.cpp
)

target_include_directories(csinterop
    PRIVATE
        ${MONO_INCLUDE_DIRS}
)

target_compile_definitions(csinterop
    PRIVATE
        MONO_ROOT="${MONO_ROOT}"
)

target_compile_options(csinterop PRIVATE ${CSINTEROP_WARN_FLAGS})

target_link_libraries(csinterop
    PRIVATE
        fmt::fmt
        Microsoft.GSL::GSL
        ${MONO_LIBRARIES}
)
