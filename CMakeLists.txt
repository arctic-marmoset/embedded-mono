cmake_minimum_required(VERSION 3.21 FATAL_ERROR)
project(EmbeddedMono)

set(CMAKE_CXX_STANDARD 20)

list(APPEND CMAKE_MODULE_PATH
    "${CMAKE_CURRENT_SOURCE_DIR}/cmake/module"
)

string(TOUPPER ${CMAKE_BUILD_TYPE} CMAKE_BUILD_TYPE_UPPER)
if(CMAKE_BUILD_TYPE_UPPER STREQUAL "RELEASE")
    include(CheckIPOSupported)
    check_ipo_supported(RESULT IPO_IS_SUPPORTED)
    if(IPO_IS_SUPPORTED)
        set(CMAKE_INTERPROCEDURAL_OPTIMIZATION ON)
    endif()
endif()

option(EMBEDDED_MONO_USE_EXTENSIVE_WARNINGS "Enable additional compiler warnings" ON)

if(MSVC)
    string(REGEX REPLACE "[-/]W[1-4]" "" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
    set(EMBEDDED_MONO_WARN_FLAGS /W4 /permissive-)
else()
    set(EMBEDDED_MONO_WARN_FLAGS -Wall -Wextra -Wpedantic)
endif()

if(EMBEDDED_MONO_USE_EXTENSIVE_WARNINGS)
    if(MSVC)
        list(APPEND EMBEDDED_MONO_WARN_FLAGS
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
        list(APPEND EMBEDDED_MONO_WARN_FLAGS
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
        list(APPEND EMBEDDED_MONO_WARN_FLAGS
            -Wduplicated-cond
            -Wduplicated-branches
            -Wlogical-op
            -Wuseless-cast
        )
    elseif(CMAKE_CXX_COMPILER_ID MATCHES ".*Clang")
        list(APPEND EMBEDDED_MONO_WARN_FLAGS
            -Wshorten-64-to-32
        )
    endif()
endif()

find_package(fmt   CONFIG REQUIRED)
find_package(Mono2        REQUIRED)

add_executable(embedded-mono
    src/main.cpp
)

target_compile_definitions(embedded-mono
    PRIVATE
        MONO_RELATIVE_DIR="mono"
)

target_compile_options(embedded-mono PRIVATE ${EMBEDDED_MONO_WARN_FLAGS})

target_link_libraries(embedded-mono
    PRIVATE
        fmt::fmt-header-only
        Mono::Mono2
)

# TODO: This should ideally be set up as a custom target to be depended on.
add_custom_command(TARGET embedded-mono POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy ${MONO2_SHARED_LIBRARY} $<TARGET_FILE_DIR:embedded-mono>
    COMMAND_EXPAND_LISTS
)

add_custom_command(TARGET embedded-mono POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E make_directory $<TARGET_FILE_DIR:embedded-mono>/Mono/etc
    COMMAND ${CMAKE_COMMAND} -E make_directory $<TARGET_FILE_DIR:embedded-mono>/Mono/lib/mono/4.5
    COMMAND ${CMAKE_COMMAND} -E copy ${MONO2_MSCORLIB} $<TARGET_FILE_DIR:embedded-mono>/Mono/lib/mono/4.5
    COMMAND ${CMAKE_COMMAND} -E copy_directory ${MONO2_ETC_DIRECTORY} $<TARGET_FILE_DIR:embedded-mono>/Mono/etc
    COMMAND_EXPAND_LISTS
)

# TODO: Copy dependencies (e.g. System, System.Core) to Managed directory.
# TODO: Copy DLL debug symbols for Debug builds.
