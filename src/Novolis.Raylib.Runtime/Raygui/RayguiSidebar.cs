using System.Text;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Transformations;

namespace Novolis.Raylib.Raygui;

/// <summary>UTF-8 marshalling for raygui controls via the required in-box native shim.</summary>
public static unsafe class RayguiSidebar
{
    private const int Utf8StackCap = 512;

    public static bool TryDrawNavRow(Rectangle bounds, string label, out bool clicked)
    {
        clicked = false;
        RayguiShimHost.EnsureInitialized();

        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(label, utf8, out _))
            return false;

        fixed (byte* text = utf8)
        {
            var r = ToRaygui(bounds);
            clicked = RayguiShimExports.GuiButton_ptr(r, text) != 0;
        }

        return true;
    }

    /// <summary>Draws a raygui panel (UTF-8 title).</summary>
    public static bool TryDrawPanel(Rectangle bounds, string title, out int controlState)
    {
        controlState = 0;
        RayguiShimHost.EnsureInitialized();

        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(title, utf8, out _))
            return false;

        fixed (byte* text = utf8)
        {
            var r = ToRaygui(bounds);
            controlState = RayguiShimExports.GuiPanel_ptr(r, text);
        }

        return true;
    }

    /// <summary>Immediate-mode toggle; <paramref name="active"/> is read/written as a 1-byte C bool.</summary>
    public static bool TryDrawToggle(Rectangle bounds, string label, ref bool active)
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
            var r = ToRaygui(bounds);
            _ = RayguiShimExports.GuiToggle_ptr(r, text, statePtr);
            active = stateBuf[0] != 0;
        }

        return true;
    }

    /// <summary>Combo box; <paramref name="active"/> is the selected item index (read/write).</summary>
    public static bool TryDrawComboBox(Rectangle bounds, string text, ref int active)
    {
        RayguiShimHost.EnsureInitialized();

        Span<byte> utf8 = stackalloc byte[Utf8StackCap];
        if (!TryEncodeUtf8NullTerminated(text, utf8, out _))
            return false;

        fixed (byte* label = utf8)
        fixed (int* activePtr = &active)
        {
            var r = ToRaygui(bounds);
            _ = RayguiShimExports.GuiComboBox_ptr(r, label, activePtr);
        }

        return true;
    }

    private static RayguiShimExports.RayguiRectangle ToRaygui(Rectangle b) =>
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
