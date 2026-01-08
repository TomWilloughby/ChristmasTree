using ChristmasTreeUI;

var rnd = new Random();

var complete = false;
var completeLock = new object();

// var system = new LSystem("0", new Dictionary<char, string>() { { '0', "1[0]0" }, { '1', "11" } });
var system = new LSystem("2", new Dictionary<char, string>() { { '2', "1[[[0[[[00[[[0[[[[[[[2" } });

using var win = new CustomWindow("Hello World");

List<Line> allLines = [];

var task = Task.Run(() =>
{
    try
    {
        CustomWindow.OnPaint += CustomWindow_OnPaint;

        while (true)
        {
            lock (completeLock)
            {
                if (complete)
                {
                    break;
                }
            }

            int x = 500, y = 500, angle = 0;
            var positions = new Stack<(int, int, int)>();
            allLines.Clear();
            win.Repaint();
            Line? nextLine = null;

            foreach (char symbol in system.Value)
            {
                nextLine = null;
                switch (symbol)
                {
                    case '0':
                        nextLine = Draw(x, y, angle, true); // Draw leaf
                        break;
                    case '1':
                        nextLine = Draw(x, y, angle, false); // Draw line
                        break;
                    case '[':
                        // push position and angle, turn left 45 degrees
                        positions.Push((x, y, angle));
                        angle -= 45;
                        break;
                    case ']':
                        // pop position and angle, turn right 45 degrees
                        if (positions.Count > 0)
                        {
                            (x, y, angle) = positions.Pop();
                        }

                        angle += 45;
                        break;
                }

                if (angle < 0)
                {
                    angle = 315;
                }

                if (angle >= 360)
                {
                    angle = 0;
                }

                if (nextLine != null)
                {
                    x = nextLine.EndX;
                    y = nextLine.EndY;
                }
            }

            system.Iterate();
        }

        CustomWindow.OnPaint -= CustomWindow_OnPaint;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        throw;
    }
});

Line Draw(int startX, int startY, int angle, bool isLeaf)
{
    int r = 0, g = 0, b = 0, lineLength, delay;

    if (isLeaf)
    {
        g = 255;
        lineLength = 10;
        delay = 400;
    }
    else
    {
        r = 105;
        g = 80;
        lineLength = 50;
        delay = 2_000;
    }

    var (endX, endY) = CalculateEndPosition(startX, startY, angle, lineLength);
    var line = new Line(startX, startY, endX, endY, delay, r, g, b);
    allLines.Add(line);

    line.StartAnimation();
    win.TriggerPaint();
    line.WaitForAnimationEnd();

    return line;
}

/// Calculate the end position of a line starting at x,y with a given angle and length.
/// It's assumed angle is a multiple of 45 degrees.
(int, int) CalculateEndPosition(int x, int y, int angle, int lineLength)
{
    var angledLength = lineLength;

    return angle switch
    {
        0 => (x, y - lineLength),// line go up
        45 => (x + angledLength, y - angledLength),// line go up & right
        90 => (x + lineLength, y),// line go right
        135 => (x + angledLength, y + angledLength),// line go down & right
        180 => (x, y + lineLength),// line go down
        225 => (x - angledLength, y + angledLength),// line go down & left
        270 => (x - lineLength, y),// line go left
        315 => (x - angledLength, y - angledLength),// line go up & left
        _ => throw new NotImplementedException($"Angles that aren't multiples of 45 degrees aren't supported (trying to calculate line for angle {angle})."),
    };
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

    if (lines.Any(line => line.IsAnimating))
    {
        win.TriggerPaint();
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

lock (completeLock)
{
    complete = true;
}

/*
0: draw a line segment ending in a leaf
1: draw a line segment
[: push position and angle, turn left 45 degrees
]: pop position and angle, turn right 45 degrees
axiom  : 0
rules  : (1 → 11), (0 → 1[0]0)
*/
