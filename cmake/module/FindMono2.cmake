if(DEFINED ENV{MONO_ROOT})
    set(MONO2_ROOT "${MONO_ROOT}" CACHE PATH "Mono2 root directory")
endif()

if(WIN32 AND NOT DEFINED MONO2_ROOT)
    message(FATAL_ERROR "Unable to find Mono2 if MONO2_ROOT is not set.")
endif()

find_path(MONO2_INCLUDE_DIR mono/jit/jit.h
    PATHS "${MONO2_ROOT}"
    PATH_SUFFIXES include/mono-2.0
    NO_DEFAULT_PATH
)

find_library(MONO2_LIBRARY mono-2.0-sgen
    PATHS "${MONO2_ROOT}"
    PATH_SUFFIXES lib
    NO_DEFAULT_PATH
)

find_file(MONO2_SHARED_LIBRARY mono-2.0-sgen${CMAKE_SHARED_LIBRARY_SUFFIX}
    PATHS "${MONO2_ROOT}"
    PATH_SUFFIXES bin
    NO_DEFAULT_PATH
)

include(FindPackageHandleStandardArgs)
find_package_handle_standard_args(Mono2
    REQUIRED_VARS MONO2_INCLUDE_DIR MONO2_LIBRARY MONO2_SHARED_LIBRARY
)

if(WIN32)
    set(MONO2_LINK_LIBRARIES
        ws2_32
        psapi
        ole32
        winmm
        oleaut32
        advapi32
        version
    )
endif()

if(MONO2_FOUND)
    add_library(Mono::Mono2 SHARED IMPORTED)

    set_target_properties(Mono::Mono2 PROPERTIES
        IMPORTED_IMPLIB ${MONO2_LIBRARY}
        IMPORTED_LOCATION ${MONO2_SHARED_LIBRARY}
        INTERFACE_INCLUDE_DIRECTORIES ${MONO2_INCLUDE_DIR}
        INTERFACE_LINK_LIBRARIES ${MONO2_LINK_LIBRARIES}
    )
endif()

mark_as_advanced(MONO2_INCLUDE_DIR MONO2_LIBRARY MONO2_SHARED_LIBRARY)
