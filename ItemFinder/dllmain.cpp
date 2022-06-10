// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

#include "ItemFinder.h"

LogCallback s_LogString;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    {
        s_LogString = nullptr;
        wil::SetResultLoggingCallback([](wil::FailureInfo const& failure) noexcept
        {
            constexpr std::size_t bufferSize = 1024;

            wchar_t message[bufferSize];
            if (SUCCEEDED(wil::GetFailureLogString(message, bufferSize, failure)))
            {
                if (s_LogString != nullptr)
                {
                    s_LogString(LogLevel::Warn, message);
                }
                if (IsDebuggerPresent())
                {
                    OutputDebugStringW(message);
                }
            }
        });
        break;
    }
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

