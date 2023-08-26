#pragma once
#include <vadefs.h>

struct Logger
{
    static void Log(const char* format, ...);
    static void Verbose(const char* format, ...);

private:
    static void LogImpl(const char* format, va_list args);
};
