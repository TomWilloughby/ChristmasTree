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
    public readonly ManualResetEvent AnimationEndSignal = new(true);

    public Line(int startX, int startY, int endX, int endY, int durationMs, int r, int g, int b)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        DurationTicks = durationMs * 10_000;

        XPerTick = (double)(endX - startX) / (double)DurationTicks;
        YPerTick = (double)(endY - startY) / (double)DurationTicks;

        Colour = Graphics.RGB(r, g, b);
    }

    public void StartAnimation()
    {
        AnimationEndSignal.Reset();
        IsAnimating = true;
        startTick = DateTime.Now.Ticks;
    }

    public void WaitForAnimationEnd()
    {
        AnimationEndSignal.WaitOne();
    }

    public void Draw(IntPtr hdc)
    {
        var wasAnimating = IsAnimating;

        var elapsedTicks = IsAnimating ? DateTime.Now.Ticks - startTick : 0;
        IsAnimating = IsAnimating && elapsedTicks < DurationTicks;

        // Get the current pen from the HDC (we'll need this for cleanup)
        IntPtr hObject = Graphics.SelectObject(hdc, Graphics.GetStockObject((int)Graphics.STOCK_OBJECT_TYPE.DC_PEN));
        IntPtr hPen = IntPtr.Zero;

        try
        {
            hPen = Graphics.SetDCPenColor(hdc, Colour);

            var moved = Graphics.MoveToEx(hdc, StartX, StartY, IntPtr.Zero);
            if (!moved)
            {
                Win32.ThrowLastError("Could not move to initial position");
            }

            if (!IsAnimating)
            {
                if (!Graphics.LineTo(hdc, EndX, EndY))
                {
                    Win32.ThrowLastError("Could not draw completed line");
                }

                if (wasAnimating)
                {
                    AnimationEndSignal.Set();
                    OnAnimationComplete?.Invoke();
                }

                return;
            }

            var endX = (int)Math.Round(StartX + (XPerTick * elapsedTicks));
            var endY = (int)Math.Round(StartY + (YPerTick * elapsedTicks));

            if (!Graphics.LineTo(hdc, endX, endY))
            {
                Win32.ThrowLastError("Could not draw partial line");
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
                Graphics.SelectObject(hdc, hObject);
                Graphics.DeleteObject(hObject);
            }

            if (hPen != IntPtr.Zero)
            {
                Graphics.DeleteObject(hPen);
            }
        }
    }
}
