using System.Runtime.InteropServices;

namespace ChristmasTreeUI;

internal class Win32
{
    public static void ThrowLastError(string message)
    {
        int error = Marshal.GetLastWin32Error();
        throw new Exception($"{message}. Error code: {error}");
    }
}
