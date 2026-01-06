using ChristmasTreeUI;

var signal = new AutoResetEvent(false);

using var win = new CustomWindow("Hello World");
var line = new Line(50, 50, 250, 250, 10_000);

var task = Task.Run(() =>
{
    CustomWindow.OnPaint += CustomWindow_OnPaint;
    line.StartAnimation();
    win.Repaint();
    signal.WaitOne();
    CustomWindow.OnPaint -= CustomWindow_OnPaint;
});

void CustomWindow_OnPaint(IntPtr hdc)
{
    line.Draw(hdc);

    if (line.IsAnimating)
    {
        win.Repaint();
    }
}

win.Loop();
signal.Set();
