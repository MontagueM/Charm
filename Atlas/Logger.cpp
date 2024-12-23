#include "Logger.h"

#include <stdarg.h>

#include <iostream>
#include <ostream>

void Logger::Log(const char* format, ...)
{
    va_list args;
    va_start(args, format);
    LogImpl(format, args);
    va_end(args);
}

void Logger::Verbose(const char* format, ...)
{
    va_list args;
    va_start(args, format);
    LogImpl(format, args);
    va_end(args);
}

void Logger::LogImpl(const char* format, va_list args)
{
    char buffer[512];
    vsprintf_s(buffer, format, args);
    std::cout << buffer << std::endl;
}
