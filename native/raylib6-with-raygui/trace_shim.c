/* Formats raylib TraceLogCallback (va_list) and forwards to a managed-friendly C function pointer. */
#include <stdarg.h>
#include <stddef.h>
#include <stdio.h>
#include "raylib.h"

#if defined(_WIN32)
#define SC_EXPORT __declspec(dllexport)
#else
#define SC_EXPORT __attribute__((visibility("default")))
#endif

typedef void (*StarconflictsManagedTraceFn)(int logLevel, const char *message);

static StarconflictsManagedTraceFn g_managed_trace;

static void NOVOLIS_RAYLIB_trace_shim(int logLevel, const char *text, va_list args)
{
    va_list cpy;
    va_copy(cpy, args);
    char buf[4096];
#if defined(_WIN32) && defined(_MSC_VER)
    int n = _vsnprintf(buf, sizeof(buf), text, cpy);
#else
    int n = vsnprintf(buf, sizeof(buf), text, cpy);
#endif
    va_end(cpy);
    if (n < 0 || (size_t)n >= sizeof(buf))
        buf[sizeof(buf) - 1] = '\0';
    else
        buf[n] = '\0';

    if (g_managed_trace != NULL)
        g_managed_trace(logLevel, buf);
}

SC_EXPORT void NOVOLIS_RAYLIB_trace_forwarder_install(StarconflictsManagedTraceFn fn)
{
    g_managed_trace = fn;
    SetTraceLogCallback(NOVOLIS_RAYLIB_trace_shim);
}

SC_EXPORT void NOVOLIS_RAYLIB_trace_forwarder_clear(void)
{
    g_managed_trace = NULL;
    SetTraceLogCallback(NULL);
}
