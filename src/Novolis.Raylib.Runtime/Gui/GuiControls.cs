using System.Runtime.InteropServices;
using System.Text;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib;

/// <summary>UTF-8 marshalling for Dear ImGui via <see cref="ImguiShimHost"/>.</summary>
public static unsafe class GuiControls
{
    private const int Utf8StackCap = 512;

    /// <summary>Initializes ImGui context and optional dark theme.</summary>
    /// <param name="darkTheme">When true, applies the dark style preset.</param>
    public static void Setup(bool darkTheme = true)
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_setup_ptr(darkTheme ? 1 : 0);
    }

    /// <summary>Shuts down the ImGui context.</summary>
    public static void Shutdown()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_shutdown_ptr();
    }

    /// <summary>Starts a new ImGui frame (call once per raylib frame before widgets).</summary>
    public static void NewFrame()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_begin_ptr();
    }

    /// <summary>Submits ImGui draw data to raylib (call after widgets, before <c>EndDrawing</c>).</summary>
    public static void Render()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_rlimgui_end_ptr();
    }

    /// <summary>Begins an ImGui window.</summary>
    /// <param name="title">UTF-8 window title.</param>
    /// <param name="open">When false on return, the user closed the window.</param>
    /// <returns>Whether the window contents are visible this frame.</returns>
    public static bool BeginWindow(string title, ref bool open)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(title, utf8, out _))
            return false;

        var openInt = open ? 1 : 0;
        int visible;
        fixed (byte* name = utf8)
        {
            visible = ImguiShimExports.novolis_igBegin_ptr(name, &openInt, 0);
        }

        open = openInt != 0;
        return visible != 0;
    }

    /// <summary>Ends the current ImGui window.</summary>
    public static void EndWindow()
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_igEnd_ptr();
    }

    /// <summary>Draws a labeled button.</summary>
    /// <param name="label">Button label.</param>
    /// <returns>Whether the button was pressed this frame.</returns>
    public static bool Button(string label)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* text = utf8)
            return ImguiShimExports.novolis_igButton_ptr(text) != 0;
    }

    /// <summary>Draws static text.</summary>
    /// <param name="text">Label text.</param>
    public static void Text(string text)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(text, utf8, out _))
            return;

        fixed (byte* label = utf8)
            ImguiShimExports.novolis_igText_ptr(label);
    }

    /// <summary>Draws a checkbox.</summary>
    /// <param name="label">Checkbox label.</param>
    /// <param name="value">Checked state; updated when toggled.</param>
    /// <returns>Whether the value changed this frame.</returns>
    public static bool Checkbox(string label, ref bool value)
    {
        ImguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        var v = value ? 1 : 0;
        int changed;
        fixed (byte* name = utf8)
            changed = ImguiShimExports.novolis_igCheckbox_ptr(name, &v);

        value = v != 0;
        return changed != 0;
    }

    /// <summary>Draws a float slider.</summary>
    /// <param name="label">Slider label.</param>
    /// <param name="value">Current value; updated when dragged.</param>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>Whether the value changed this frame.</returns>
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

    /// <summary>Continues layout on the same line as the previous widget.</summary>
    /// <param name="offsetFromStartX">Horizontal offset from the line start.</param>
    /// <param name="spacing">Spacing before this item.</param>
    public static void SameLine(float offsetFromStartX, float spacing)
    {
        ImguiShimHost.EnsureInitialized();
        ImguiShimExports.novolis_igSameLine_ptr(offsetFromStartX, spacing);
    }

    /// <summary>Draws a horizontal separator.</summary>
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
