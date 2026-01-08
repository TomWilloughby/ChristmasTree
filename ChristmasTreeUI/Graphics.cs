using System.Runtime.InteropServices;

namespace ChristmasTreeUI;

internal class Graphics
{
    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern bool MoveToEx(
        IntPtr hdc,
        int x,
        int y,
        IntPtr previousPositionPointPtr
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern bool LineTo(
        IntPtr hdc,
        int x,
        int y
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern IntPtr GetStockObject(
        int objectType
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    public static extern IntPtr SelectObject(
        IntPtr hdc,
        IntPtr objectToSelect
    );

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);

    [StructLayout(LayoutKind.Sequential)]
    public struct LOGBRUSH
    {
        public uint lbStyle;
        public int lbColor;
        public IntPtr lbHatch;
    }

    public static IntPtr SetDCBrushColor(IntPtr hdc, int color)
    {
        //Create a new LOGBRUSH structure with the desired color
        var lb = new LOGBRUSH
        {
            lbStyle = (uint)LOG_BRUSH_STYLE.SOLID,
            lbColor = color
        };

        // Convert the brush to an IntPtr
        IntPtr hBrush = Marshal.AllocHGlobal(Marshal.SizeOf(lb));
        Marshal.StructureToPtr(lb, hBrush, true);

        //Select the new brush into the HDC
        SelectObject(hdc, hBrush);
        return hBrush;
    }

    public static IntPtr SetDCPenColor(IntPtr hdc, int color, int width = 5)
    {
        // Create a solid pen with the specified color
        int penStyle = (int)LOG_BRUSH_STYLE.SOLID;

        var hPen = CreatePen(penStyle, width, color);

        if (hPen == IntPtr.Zero)
        {
            Win32.ThrowLastError("Could not set pen colour");
        }

        SelectObject(hdc, hPen);
        return hPen;
    }

    public static int RGB(int r, int g, int b)
    {
        return (r & 0xFF) | ((g & 0xFF) << 8) | ((b & 0xFF) << 16);
    }

    public enum LOG_BRUSH_STYLE : uint
    {
        SOLID = 0,
    }

    public enum STOCK_OBJECT_TYPE
    {
        WHITE_BRUSH = 0,
        GRAY_BRUSH = 1,
        LTGRAY_BRUSH = 2,
        DKGRAY_BRUSH = 3,
        BLACK_BRUSH = 4,
        NULL_BRUSH = 5,
        HOLLOW_BRUSH = NULL_BRUSH,
        WHITE_PEN = 6,
        BLACK_PEN = 7,
        NULL_PEN = 8,
        OEM_FIXED_FONT = 10,
        ANSI_FIXED_FONT = 11,
        ANSI_VAR_FONT = 12,
        SYSTEM_FONT = 13,
        DEVICE_DEFAULT_FONT = 14,
        DEFAULT_PALETTE = 15,
        SYSTEM_FIXED_FONT = 16,
        DEFAULT_GUI_FONT = 17,
        DC_BRUSH = 18, // (Windows 2000/XP only)
        DC_PEN = 19, // (Windows 2000/XP only)
    }
}
