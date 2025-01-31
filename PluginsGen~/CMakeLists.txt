# CMakeList.txt: CMake-Projekt für "GeneratePlugins". Schließen Sie die Quelle ein, und definieren Sie
# projektspezifische Logik hier.
#
cmake_minimum_required (VERSION 3.8)

# Aktivieren Sie Hot Reload für MSVC-Compiler, sofern unterstützt.
if (POLICY CMP0141)
  cmake_policy(SET CMP0141 NEW)
  set(CMAKE_MSVC_DEBUG_INFORMATION_FORMAT "$<IF:$<AND:$<C_COMPILER_ID:MSVC>,$<CXX_COMPILER_ID:MSVC>>,$<$<CONFIG:Debug,RelWithDebInfo>:EditAndContinue>,$<$<CONFIG:Debug,RelWithDebInfo>:ProgramDatabase>>")
endif()

project ("GeneratePlugins")

set(SolutionRoot ${CMAKE_SOURCE_DIR})

message(STATUS "Source dir   : ${CMAKE_SOURCE_DIR}")
message(STATUS "Binary dir   : ${CMAKE_BINARY_DIR}")
message(STATUS "OS           : ${CMAKE_SYSTEM_NAME}")
message(STATUS "Build type   : ${CMAKE_BUILD_TYPE}")
message(STATUS "Architecture : ${CMAKE_SYSTEM_PROCESSOR}")

set(Guard_NuGetrPackages ${SolutionRoot}/../Plugins/NuGet-Depends.dll)
set(Guard_Win_libcurl ${SolutionRoot}/../Plugins/x64/libcurl.dll)

if(${CMAKE_SYSTEM_PROCESSOR} STREQUAL "AMD64")
	set(PROCESSOR x86_64)
else()
	set(PROCESSOR ${CMAKE_SYSTEM_PROCESSOR})
endif()

add_custom_target(GeneratePlugins DEPENDS libcurl_shared)

if(WIN32)
	add_custom_target(NuGetPackages DEPENDS ${Guard_NuGetrPackages})
	add_custom_command(
		OUTPUT ${Guard_NuGetrPackages}
		COMMAND "msbuild" NuGet-Depends\\NuGet-Depends.csproj /t:Restore,Build -p:Configuration=Release -p:Platform=AnyCPU
		WORKING_DIRECTORY ${SolutionRoot}
		DEPENDS ${SolutionRoot}/NuGet-Depends/NuGet-Depends.csproj
		VERBATIM)

	add_dependencies(GeneratePlugins NuGetPackages)
	
	set(CURL_USE_SCHANNEL ON CACHE BOOL "Use Schannel for SSL" FORCE)
endif()

set(CURL_ENABLE_SSL ON CACHE BOOL "Enable SSL support" FORCE)
set(CURL_LIBCURL_SOVERSION OFF CACHE BOOL "Enable shared library versioning" FORCE)
add_subdirectory("curl")

add_custom_command(TARGET GeneratePlugins POST_BUILD
	COMMAND ${CMAKE_COMMAND} -E make_directory ${SolutionRoot}/../Plugins/${PROCESSOR}
	COMMAND ${CMAKE_COMMAND} -E copy_if_different "${CMAKE_BINARY_DIR}/curl/lib/libcurl${CMAKE_SHARED_LIBRARY_SUFFIX}" ${SolutionRoot}/../Plugins/${PROCESSOR})

#if (CMAKE_VERSION VERSION_GREATER 3.12)
#  set_property(TARGET GeneratePlugins PROPERTY CXX_STANDARD 20)
#endif()

# TODO: Fügen Sie bei Bedarf Tests hinzu, und installieren Sie Ziele.
