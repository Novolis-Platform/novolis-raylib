/* Stable C exports for Novolis codegen / P/Invoke (wraps raylib-cimgui + cimgui). */
#include "raylib.h"
#include "cimgui.h"
#include "rlcimgui.h"

#if defined(_WIN32)
#define NOVOLIS_IMGUI_API __declspec(dllexport)
#else
#define NOVOLIS_IMGUI_API __attribute__((visibility("default")))
#endif

static int g_setup;

NOVOLIS_IMGUI_API void novolis_rlimgui_setup(int darkTheme)
{
    if (g_setup)
        return;
    rligSetup(darkTheme != 0);
    g_setup = 1;
}

NOVOLIS_IMGUI_API void novolis_rlimgui_shutdown(void)
{
    if (!g_setup)
        return;
    rligShutdown();
    g_setup = 0;
}

NOVOLIS_IMGUI_API void novolis_rlimgui_begin(void)
{
    rligBegin();
}

NOVOLIS_IMGUI_API void novolis_rlimgui_end(void)
{
    rligEnd();
}

NOVOLIS_IMGUI_API int novolis_igBegin(const char *name, int *p_open, int flags)
{
    return igBegin(name, (bool *)p_open, flags) ? 1 : 0;
}

NOVOLIS_IMGUI_API void novolis_igEnd(void)
{
    igEnd();
}

NOVOLIS_IMGUI_API int novolis_igButton(const char *label)
{
    return igButton(label) ? 1 : 0;
}

NOVOLIS_IMGUI_API void novolis_igText(const char *text)
{
    igText(text);
}

NOVOLIS_IMGUI_API int novolis_igCheckbox(const char *label, int *value)
{
    bool v = value != NULL && *value != 0;
    const int changed = igCheckbox(label, &v) ? 1 : 0;
    if (value != NULL)
        *value = v ? 1 : 0;
    return changed;
}

NOVOLIS_IMGUI_API int novolis_igSliderFloat(const char *label, float *value, float minValue, float maxValue)
{
    return igSliderFloat(label, value, minValue, maxValue, "%.3f") ? 1 : 0;
}

NOVOLIS_IMGUI_API void novolis_igSameLine(float offsetFromStartX, float spacing)
{
    igSameLine(offsetFromStartX, spacing);
}

NOVOLIS_IMGUI_API void novolis_igSeparator(void)
{
    igSeparator();
}
