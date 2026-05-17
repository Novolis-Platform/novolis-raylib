using System.Runtime.InteropServices;
using System.Text;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib;

/// <summary>UTF-8 marshalling for Dear ImGui via <see cref="ImguiShimHost"/>.</summary>
public static unsafe class GuiControls
{
    private const int Utf8StackCap = 512;

    public static void Setup(bool darkTheme = true)
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_setup_ptr(darkTheme ? 1 : 0);
    }

    public static void Shutdown()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_shutdown_ptr();
    }

    public static void NewFrame()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_begin_ptr();
    }

    public static void Render()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_end_ptr();
    }

    public static bool BeginWindow(string title, ref bool open)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(title, utf8, out _))
            return false;

        var openInt = open ? 1 : 0;
        fixed (byte* name = utf8)
        fixed (int* pOpen = &openInt)
        {
            var visible = ImguiShimExports.novolis_igBegin_ptr(name, pOpen, 0) != 0;
            open = openInt != 0;
            return visible;
        }
    }

    public static void EndWindow()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_igEnd_ptr();
    }

    public static bool Button(string label)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* text = utf8)
            return ImguiShimExports.novolis_igButton_ptr(text) != 0;
    }

    public static void Text(string text)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(text, utf8, out _))
            return;

        fixed (byte* label = utf8)
            ImguiShimExports.novolis_igText_ptr(label);
    }

    public static bool Checkbox(string label, ref bool value)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        var v = value ? 1 : 0;
        int changed;
        fixed (byte* name = utf8)
        fixed (int* pValue = &v)
            changed = ImguiShimExports.novolis_igCheckbox_ptr(name, pValue);

        value = v != 0;
        return changed != 0;
    }

    public static bool Slider(string label, ref float value, float min, float max)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* name = utf8)
        fixed (float* pValue = &value)
            return ImguiShimExports.novolis_igSliderFloat_ptr(name, pValue, min, max) != 0;
    }

    public static void SameLine(float offsetFromStartX, float spacing)
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_igSameLine_ptr(offsetFromStartX, spacing);
    }

    public static void Separator()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_igSeparator_ptr();
    }

    private static bool TryEncodeUtf8NullTerminated(string? value, Span<byte> buffer, out int byteCount)
    {
        byteCount = 0;
        if (string.IsNullOrEmpty(value))
        {
            if (buffer.Length < 1)
                return false;
            buffer[0] = 0;
            return true;
        }

        var maxChars = buffer.Length - 1;
        var charCount = Math.Min(value.Length, maxChars);
        var written = Encoding.UTF8.GetBytes(value.AsSpan(0, charCount), buffer);
        if (written >= buffer.Length - 1 && value.Length > charCount)
            return false;

        buffer[written] = 0;
        byteCount = written + 1;
        return true;
    }
}
