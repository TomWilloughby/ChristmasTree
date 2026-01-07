using ChristmasTreeUI;

var signal = new AutoResetEvent(false);

using var win = new CustomWindow("Hello World");
var line = new Line(50, 50, 250, 250, 10_000);

var task = Task.Run(() =>
{
    try
    {
        CustomWindow.OnPaint += CustomWindow_OnPaint;
        line.StartAnimation();
        win.Repaint();
        signal.WaitOne();
        CustomWindow.OnPaint -= CustomWindow_OnPaint;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }
});


void CustomWindow_OnPaint(IntPtr hdc)
{
    line.Draw(hdc);

    if (line.IsAnimating)
    {
        win.Repaint();
    }
}

try
{
    win.Loop();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    throw;
}
signal.Set();

/*
0: draw a line segment ending in a leaf
1: draw a line segment
[: push position and angle, turn left 45 degrees
]: pop position and angle, turn right 45 degrees
axiom  : 0
rules  : (1 → 11), (0 → 1[0]0)
*/
