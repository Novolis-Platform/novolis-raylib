using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib;

/// <summary>UTF-8 marshalling for raygui via <see cref="RayguiShimHost"/>.</summary>
public static unsafe class GuiControls
{
    private const int Utf8StackCap = 512;

    public static void Enable()
    {
        RayguiShimHost.EnsureInitialized();
        RayguiShimExports.GuiEnable_ptr();
    }

    public static void Disable()
    {
        RayguiShimHost.EnsureInitialized();
        RayguiShimExports.GuiDisable_ptr();
    }

    public static void Lock()
    {
        RayguiShimHost.EnsureInitialized();
        RayguiShimExports.GuiLock_ptr();
    }

    public static void Unlock()
    {
        RayguiShimHost.EnsureInitialized();
        RayguiShimExports.GuiUnlock_ptr();
    }

    public static void SetAlpha(float alpha)
    {
        RayguiShimHost.EnsureInitialized();
        RayguiShimExports.GuiSetAlpha_ptr(alpha);
    }

    public static void LoadStyleDefault()
    {
        RayguiShimHost.EnsureInitialized();
        RayguiShimExports.GuiLoadStyleDefault_ptr();
    }

    public static bool Button(RectangleF bounds, string label)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* text = utf8)
            return RayguiShimExports.GuiButton_ptr(ToRaygui(bounds), text) != 0;
    }

    public static void Label(RectangleF bounds, string text)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(text, utf8, out _))
            return;

        fixed (byte* label = utf8)
            _ = RayguiShimExports.GuiLabel_ptr(ToRaygui(bounds), label);
    }

    public static bool Panel(RectangleF bounds, string title)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(title, utf8, out _))
            return false;

        fixed (byte* text = utf8)
            return RayguiShimExports.GuiPanel_ptr(ToRaygui(bounds), text) != 0;
    }

    public static void GroupBox(RectangleF bounds, string title)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(title, utf8, out _))
            return;

        fixed (byte* text = utf8)
            _ = RayguiShimExports.GuiGroupBox_ptr(ToRaygui(bounds), text);
    }

    public static bool Toggle(RectangleF bounds, string label, ref bool active)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        Span<byte> stateBuf = stackalloc byte[1];
        stateBuf[0] = active ? (byte)1 : (byte)0;
        fixed (byte* text = utf8)
        fixed (byte* statePtr = stateBuf)
        {
            var ok = RayguiShimExports.GuiToggle_ptr(ToRaygui(bounds), text, statePtr) != 0;
            active = stateBuf[0] != 0;
            return ok;
        }
    }

    public static bool CheckBox(RectangleF bounds, string label, ref bool active)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        Span<byte> stateBuf = stackalloc byte[1];
        stateBuf[0] = active ? (byte)1 : (byte)0;
        fixed (byte* text = utf8)
        fixed (byte* statePtr = stateBuf)
        {
            var ok = RayguiShimExports.GuiCheckBox_ptr(ToRaygui(bounds), text, statePtr) != 0;
            active = stateBuf[0] != 0;
            return ok;
        }
    }

    public static bool ComboBox(RectangleF bounds, string text, ref int active)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(text, utf8, out _))
            return false;

        fixed (byte* label = utf8)
        fixed (int* activePtr = &active)
            return RayguiShimExports.GuiComboBox_ptr(ToRaygui(bounds), label, activePtr) != 0;
    }

    public static bool Slider(RectangleF bounds, string label, ref float value, float min, float max)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* textLeft = utf8)
        fixed (float* valuePtr = &value)
            return RayguiShimExports.GuiSlider_ptr(ToRaygui(bounds), textLeft, textLeft, valuePtr, min, max) != 0;
    }

    public static bool SliderBar(RectangleF bounds, string label, ref float value, float min, float max)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* textLeft = utf8)
        fixed (float* valuePtr = &value)
            return RayguiShimExports.GuiSliderBar_ptr(ToRaygui(bounds), textLeft, textLeft, valuePtr, min, max) != 0;
    }

    public static bool ProgressBar(RectangleF bounds, string label, ref float value, float min, float max)
    {
        RayguiShimHost.EnsureInitialized();
        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* textLeft = utf8)
        fixed (float* valuePtr = &value)
            return RayguiShimExports.GuiProgressBar_ptr(ToRaygui(bounds), textLeft, textLeft, valuePtr, min, max) != 0;
    }

    private static RayguiShimExports.RayguiRectangle ToRaygui(RectangleF b) =>
        new()
        {
            X = b.X,
            Y = b.Y,
            Width = b.Width,
            Height = b.Height,
        };

    private static bool TryEncodeUtf8NullTerminated(string label, Span<byte> utf8, out int written)
    {
        written = 0;
        if (string.IsNullOrEmpty(label))
        {
            if (utf8.Length < 1)
                return false;
            utf8[0] = 0;
            return true;
        }

        var max = utf8.Length - 1;
        var count = Encoding.UTF8.GetBytes(label.AsSpan(), utf8[..max]);
        if (count >= max)
            return false;

        utf8[count] = 0;
        written = count + 1;
        return true;
    }
}
