using System.Text;

namespace ChristmasTreeUI;

internal class LSystem(string axiom, Dictionary<char, string> rules)
{
    private string current = axiom;
    public string Value => current;

    public void Iterate()
    {
        StringBuilder next = new();
        foreach (char c in current)
        {
            if (rules.TryGetValue(c, out string? value))
            {
                next.Append(value);
            }
            else
            {
                next.Append(c);
            }
        }

        current = next.ToString();
    }
}
