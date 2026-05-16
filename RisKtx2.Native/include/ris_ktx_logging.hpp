#pragma once

#include "macros.hpp"
#include <cstdint>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @brief Explicitly enable debug logging to a file.
 *
 * Logging is disabled by default. It can be enabled in two ways:
 * 1. Call this function before any other API call.
 * 2. Set the environment variable `RIS_KTX2_ENABLE_LOGGING=1` before loading the library.
 *
 * If logFilePath is nullptr, a default file "ris_ktx2.log" is used.
 *
 * @param logFilePath Path to the log file, or nullptr for default.
 */
API_EXPORT
void ris_ktxTexture2_EnableLogging(const char* logFilePath);

/**
 * @brief Check whether debug logging is currently enabled.
 * @return 1 if logging is enabled, 0 otherwise.
 */
API_EXPORT
uint32_t ris_ktxTexture2_IsLoggingEnabled(void);

#ifdef __cplusplus
} // extern "C"

// ---------------------------------------------------------------------------
// Internal C++ helpers (not part of the public C ABI)
// ---------------------------------------------------------------------------

bool ris_ktxLogging_IsEnabled();
void ris_ktxLogging_MaybeInitFromEnv();

#endif
