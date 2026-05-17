/* Stable C exports for Novolis codegen / P/Invoke (wraps raylib-cimgui + cimgui). */
#define CIMGUI_DEFINE_ENUMS_AND_STRUCTS
#include "cimgui.h"

extern "C" {
#include "raycimgui.h"
}

#if defined(_WIN32)
#define NOVOLIS_IMGUI_API extern "C" __declspec(dllexport)
#else
#define NOVOLIS_IMGUI_API extern "C" __attribute__((visibility("default")))
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
    bool open = p_open == nullptr || *p_open != 0;
    const bool visible = igBegin(name, p_open != nullptr ? &open : nullptr, flags);
    if (p_open != nullptr)
        *p_open = open ? 1 : 0;
    return visible ? 1 : 0;
}

NOVOLIS_IMGUI_API void novolis_igEnd(void)
{
    igEnd();
}

NOVOLIS_IMGUI_API int novolis_igButton(const char *label)
{
    const ImVec2 size = { 0.0f, 0.0f };
    return igButton(label, size) ? 1 : 0;
}

NOVOLIS_IMGUI_API void novolis_igText(const char *text)
{
    igText(text);
}

NOVOLIS_IMGUI_API int novolis_igCheckbox(const char *label, int *value)
{
    bool v = value != nullptr && *value != 0;
    const int changed = igCheckbox(label, &v) ? 1 : 0;
    if (value != nullptr)
        *value = v ? 1 : 0;
    return changed;
}

NOVOLIS_IMGUI_API int novolis_igSliderFloat(const char *label, float *value, float minValue, float maxValue)
{
    return igSliderFloat(label, value, minValue, maxValue, "%.3f", 0) ? 1 : 0;
}

NOVOLIS_IMGUI_API void novolis_igSameLine(float offsetFromStartX, float spacing)
{
    igSameLine(offsetFromStartX, spacing);
}

NOVOLIS_IMGUI_API void novolis_igSeparator(void)
{
    igSeparator();
}
