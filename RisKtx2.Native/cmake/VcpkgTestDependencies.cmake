# Dependencies that are specific for test project


# Install dependencies as suggested by vcpkg.
find_package(Catch2 CONFIG REQUIRED)


set(TEST_PACKAGES_LIBRARIES
        Catch2::Catch2
        Catch2::Catch2WithMain
)

set(TEST_PACKAGES_INCLUDE
)