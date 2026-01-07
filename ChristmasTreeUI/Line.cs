using System.Runtime.InteropServices;

namespace ChristmasTreeUI;

internal class Line
{
    public bool IsAnimating { get; private set; } = false;

    public int StartX { get; init; }
    public int StartY { get; init; }
    public int EndX { get; init; }
    public int EndY { get; init; }

    private readonly int DurationTicks;

    private long startTick = 0;
    private readonly double XPerTick;
    private readonly double YPerTick;
    private readonly int Colour;

    public event Action? OnAnimationComplete;

    public Line(int startX, int startY, int endX, int endY, int durationMs, int r, int g, int b)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        DurationTicks = durationMs * 10_000;

        XPerTick = (double)(endX - startX) / (double)DurationTicks;
        YPerTick = (double)(endY - startY) / (double)DurationTicks;

        Colour = RGB(r, g, b);
    }

    public void StartAnimation()
    {
        IsAnimating = true;
        startTick = DateTime.Now.Ticks;
    }

    public void Draw(IntPtr hdc)
    {
        var wasAnimating = IsAnimating;

        var elapsedTicks = IsAnimating ? DateTime.Now.Ticks - startTick : 0;
        IsAnimating = IsAnimating && elapsedTicks < DurationTicks;

        // Get the current pen from the HDC (we'll need this for cleanup)
        IntPtr hObject = SelectObject(hdc, GetStockObject((int)STOCK_OBJECT_TYPE.DC_PEN));
        IntPtr hPen = IntPtr.Zero;

        try
        {
            hPen = SetDCPenColor(hdc, Colour);

            var moved = MoveToEx(hdc, StartX, StartY, IntPtr.Zero);
            if (!moved)
            {
                ThrowLastError("Could not move to initial position");
            }

            if (!IsAnimating)
            {
                if (!LineTo(hdc, EndX, EndY))
                {
                    ThrowLastError("Could not draw completed line");
                }

                if (wasAnimating)
                {
                    OnAnimationComplete?.Invoke();
                }

                return;
            }

            var endX = (int)Math.Round(StartX + (XPerTick * elapsedTicks));
            var endY = (int)Math.Round(StartY + (YPerTick * elapsedTicks));

            if (!LineTo(hdc, endX, endY))
            {
                ThrowLastError("Could not draw partial line");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
        finally
        {
            // Restore the original object and delete the temporary brush
            if (hObject != IntPtr.Zero && hObject != (IntPtr)1)
            {
                SelectObject(hdc, hObject);
                DeleteObject(hObject);
            }

            if (hPen != IntPtr.Zero)
            {
                DeleteObject(hPen);
            }
        }
    }

    private static void ThrowLastError(string message)
    {
        int error = Marshal.GetLastWin32Error();
        throw new Exception($"{message}. Error code: {error}");
    }

    #region Win32
    [DllImport("Gdi32.dll", SetLastError = true)]
    static extern bool MoveToEx(
        IntPtr hdc,
        int x,
        int y,
        IntPtr previousPositionPointPtr
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    static extern bool LineTo(
        IntPtr hdc,
        int x,
        int y
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    static extern IntPtr GetStockObject(
        int objectType
    );

    [DllImport("Gdi32.dll", SetLastError = true)]
    static extern IntPtr SelectObject(
        IntPtr hdc,
        IntPtr objectToSelect
    );

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);

    //[DllImport("Gdi32.dll", SetLastError = true)]
    //static extern IntPtr SetDCPenColor(
    //    IntPtr hdc,
    //    int colorRef
    //);

    [StructLayout(LayoutKind.Sequential)]
    public struct LOGBRUSH
    {
        public uint lbStyle;
        public int lbColor;
        public IntPtr lbHatch;
    }

    private IntPtr SetDCPenColor(IntPtr hdc, int color, int width = 5)
    {
        // Create a new LOGBRUSH structure with the desired color (RGB: 0, 0, 255 - blue)
        //var lb = new LOGBRUSH();
        //lb.lbStyle = (uint)LOG_BRUSH_STYLE.SOLID;
        //lb.lbColor = color;

        //// Convert the brush to an IntPtr
        //IntPtr hBrush = Marshal.AllocHGlobal(Marshal.SizeOf(lb));
        //Marshal.StructureToPtr(lb, hBrush, true);

        // Select the new brush into the HDC (this changes the pen color)
        //IntPtr hObject2 = SelectObject(hdc, hBrush);

        // Create a solid pen with the specified color
        int penStyle = 0; // PS_SOLID (Solid line style)

        var hPen = CreatePen(penStyle, width, color);

        if (hPen == IntPtr.Zero)
        {
            ThrowLastError("Could not set pen colour");
        }

        SelectObject(hdc, hPen);
        return hPen;
    }

    public static int RGB(int r, int g, int b)
    {
        return (r & 0xFF) | ((g & 0xFF) << 8) | ((b & 0xFF) << 16);
    }

    enum LOG_BRUSH_STYLE : uint
    {
        SOLID = 0,
    }

    enum STOCK_OBJECT_TYPE
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
    #endregion
}
