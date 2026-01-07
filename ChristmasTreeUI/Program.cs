using ChristmasTreeUI;

var rnd = new Random();
var signal = new AutoResetEvent(false);

using var win = new CustomWindow("Hello World");

var allLines = new List<Line>();
var currentLine = new Line(50, 50, 250, 250, 5_000);
allLines.Add(currentLine);

var task = Task.Run(() =>
{
    try
    {
        CustomWindow.OnPaint += CustomWindow_OnPaint;
        currentLine.OnAnimationComplete += Line_OnAnimationComplete;
        currentLine.StartAnimation();
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

void Line_OnAnimationComplete()
{
    try
    {
        currentLine.OnAnimationComplete -= Line_OnAnimationComplete;
        var replacementLine = new Line(currentLine.EndX, currentLine.EndY, rnd.Next(0, 500), rnd.Next(0, 500), 5_000);
        replacementLine.OnAnimationComplete += Line_OnAnimationComplete;
        replacementLine.StartAnimation();
        allLines.Add(replacementLine);
        currentLine = replacementLine;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }
}

void CustomWindow_OnPaint(IntPtr hdc)
{
    var lines = new List<Line>(allLines);
    foreach (var line in lines)
    {
        try
        {
            line.Draw(hdc);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    if (currentLine.IsAnimating)
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
