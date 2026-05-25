#include "ris_ktx_logging.hpp"
#include <spdlog/spdlog.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <cstdlib>
#include <cstring>
#include <mutex>

static bool g_loggingEnabled = false;
static std::once_flag g_envCheckFlag;

API_EXPORT void ris_ktxTexture2_EnableLogging(const char* logFilePath)
{
	if (g_loggingEnabled) return;
	const char* path = logFilePath ? logFilePath : "ris_ktx2.log";
	auto logger = spdlog::basic_logger_mt("global_logger", path);
	spdlog::set_default_logger(logger);
	spdlog::set_level(spdlog::level::debug);
	g_loggingEnabled = true;
}

API_EXPORT uint32_t ris_ktxTexture2_IsLoggingEnabled(void)
{
	return g_loggingEnabled ? 1 : 0;
}

bool ris_ktxLogging_IsEnabled()
{
	return g_loggingEnabled;
}

void ris_ktxLogging_MaybeInitFromEnv()
{
	std::call_once(g_envCheckFlag, []() {
		const char* env = std::getenv("RIS_KTX2_ENABLE_LOGGING");
		if (env && std::strcmp(env, "1") == 0) {
			ris_ktxTexture2_EnableLogging(nullptr);
		}
	});
}
