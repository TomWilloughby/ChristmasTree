// Source - https://stackoverflow.com/a/138468
// Posted by morechilli, modified by community. See post 'Timeline' for change history
// Retrieved 2025-12-09, License - CC BY-SA 3.0

using System.Runtime.InteropServices;

namespace ChristmasTreeUI;

public delegate void OnPaint(IntPtr hdc);

class CustomWindow : IDisposable
{
    delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(
        LayoutKind.Sequential,
       CharSet = CharSet.Unicode
    )]
    struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern System.UInt16 RegisterClassW(
        [In] ref WNDCLASS lpWndClass
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr CreateWindowExW(
       UInt32 dwExStyle,
       [MarshalAs(UnmanagedType.LPWStr)]
       string lpClassName,
       [MarshalAs(UnmanagedType.LPWStr)]
       string lpWindowName,
       UInt32 dwStyle,
       Int32 x,
       Int32 y,
       Int32 nWidth,
       Int32 nHeight,
       IntPtr hWndParent,
       IntPtr hMenu,
       IntPtr hInstance,
       IntPtr lpParam
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern System.IntPtr DefWindowProcW(
        IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool ShowWindow(
        IntPtr hWnd,
        int nCmdShow
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool DestroyWindow(
        IntPtr hWnd
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern void PostQuitMessage(
        int exitCode
    );

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
        public uint lPrivate;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct POINT
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetMessage(IntPtr msgPtrOut, IntPtr hWnd, uint wMsgFilterMin, int wMsgFilterMax);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool TranslateMessage(IntPtr msgPtr);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool DispatchMessage(IntPtr msgPtr);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr BeginPaint(
      IntPtr hWnd,
      IntPtr paintStructOut
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool EndPaint(
      IntPtr hWnd,
      IntPtr paintStruct
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern int FillRect(
      IntPtr hDC,
      IntPtr rectPtrIn,
      IntPtr brushHandleIn
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool InvalidateRect(
      IntPtr hWnd,
      IntPtr rectPtrIn,
      bool erase
    );

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UpdateWindow(
      IntPtr hWnd
    );

    private const int ERROR_CLASS_ALREADY_EXISTS = 1410;

    private bool disposed;
    private IntPtr m_hwnd;

    private IntPtr msgPtr = IntPtr.Zero;

    public static event OnPaint? OnPaint;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (msgPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(msgPtr);
                }
            }

            // Dispose unmanaged resources
            if (m_hwnd != IntPtr.Zero)
            {
                DestroyWindow(m_hwnd);
                m_hwnd = IntPtr.Zero;
            }

        }
    }

    public CustomWindow(string class_name)
    {
        if (class_name == String.Empty) throw new System.Exception("class_name is empty");

        m_wnd_proc_delegate = CustomWndProc;

        // Create WNDCLASS
        WNDCLASS wind_class = new WNDCLASS();
        wind_class.lpszClassName = class_name;
        wind_class.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(m_wnd_proc_delegate);

        UInt16 class_atom = RegisterClassW(ref wind_class);

        int last_error = Marshal.GetLastWin32Error();

        if (class_atom == 0 && last_error != ERROR_CLASS_ALREADY_EXISTS)
        {
            throw new System.Exception("Could not register window class");
        }

        // Create window
        m_hwnd = CreateWindowExW(
            0,
            class_name,
            "Foo Bar Baz Bat",
            (uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
            CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (m_hwnd == IntPtr.Zero)
        {
            throw new System.Exception("Could not create window");
        }

        ShowWindow(m_hwnd, (int)SHOW_WINDOW.SW_NORMAL);
    }

    public void Loop()
    {
        MSG msg = new();
        msgPtr = Marshal.AllocHGlobal(Marshal.SizeOf(msg));
        Marshal.StructureToPtr(msg, msgPtr, true);

        while (GetMessage(msgPtr, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(msgPtr);
            DispatchMessage(msgPtr);
        }
    }

    private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch ((WINDOWS_MESSAGE)msg)
        {
            case WINDOWS_MESSAGE.WM_DESTROY:
                PostQuitMessage(0);
                return 0;
            case WINDOWS_MESSAGE.WM_CLOSE:
                DestroyWindow(hWnd);
                return 0;
            case WINDOWS_MESSAGE.WM_PAINT:
            {
                PAINTSTRUCT ps = new();
                var psPtr = IntPtr.Zero;
                var rectPtr = IntPtr.Zero;

                try
                {
                    psPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ps));
                    Marshal.StructureToPtr(ps, psPtr, true);
                    var hdc = BeginPaint(hWnd, psPtr);

                    // All painting occurs here, between BeginPaint and EndPaint.
                    rectPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ps.rcPaint));
                    Marshal.StructureToPtr(ps.rcPaint, rectPtr, true);

                    // We need to add 1 to the colour enum value because the enum starts at 0 which is otherwise indistinguishable from IntPtr.Zero / Win32 NULL
                    FillRect(hdc, rectPtr, (IntPtr)(COLOR.COLOR_WINDOW + 1));

                    OnPaint?.Invoke(hdc);

                    EndPaint(hWnd, psPtr);
                    return 0;
                }
                finally
                {
                    if (rectPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(rectPtr);
                    }

                    if (psPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(psPtr);
                    }
                }
            }
            default:
                return DefWindowProcW(hWnd, msg, wParam, lParam);
        }
    }

    public void Repaint()
    {
        InvalidateRect(m_hwnd, 0, true);
    }

    private WndProc m_wnd_proc_delegate;

    const int CW_USEDEFAULT = (unchecked((int)0x80000000));

    enum WINDOW_STYLE : uint
    {
        WS_BORDER = 0x00800000,          //  The window has a thin-line border
        WS_CAPTION = 0x00C00000,          //  The window has a title bar (includes the WS_BORDER style).
        WS_CHILD = 0x40000000,            //  The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
        WS_CHILDWINDOW = 0x40000000,        //  Same as the WS_CHILD style.
        WS_CLIPCHILDREN = 0x02000000,        //  Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.
        WS_CLIPSIBLINGS = 0x04000000,        //  Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated. If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
        WS_DISABLED = 0x08000000,          //  The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.
        WS_DLGFRAME = 0x00400000,          //  The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
        WS_GROUP = 0x00020000,            //  The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style. The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
        WS_HSCROLL = 0x00100000,          //  The window has a horizontal scroll bar.
        WS_ICONIC = 0x20000000,          //  The window is initially minimized. Same as the WS_MINIMIZE style.
        WS_MAXIMIZE = 0x01000000,          //  The window is initially maximized.
        WS_MAXIMIZEBOX = 0x00010000,        //  The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
        WS_MINIMIZE = 0x20000000,          //  The window is initially minimized. Same as the WS_ICONIC style.
        WS_MINIMIZEBOX = 0x00020000,        //  The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
        WS_OVERLAPPED = 0x00000000,        //  The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_TILED style.
        WS_POPUP = 0x80000000,            //  The window is a pop-up window. This style cannot be used with the WS_CHILD style.
        WS_SIZEBOX = 0x00040000,          //  The window has a sizing border. Same as the WS_THICKFRAME style.
        WS_SYSMENU = 0x00080000,          //  The window has a window menu on its title bar. The WS_CAPTION style must also be specified.
        WS_TABSTOP = 0x00010000,          //  The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function. For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
        WS_THICKFRAME = 0x00040000,        //  The window has a sizing border. Same as the WS_SIZEBOX style.
        WS_TILED = 0x00000000,            //  The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.
        WS_VISIBLE = 0x10000000,          //  The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.
        WS_VSCROLL = 0x00200000,          //  The window has a vertical scroll bar.
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,  //  The window is an overlapped window. Same as the WS_TILEDWINDOW style.
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,  //  The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,  //  The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.
    }

    enum SHOW_WINDOW
    {
        SW_HIDE = 0,            // Hides the window and activates another window.
        SW_NORMAL = 1,          // Activates and displays a window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.
        SW_SHOWMINIMIZED = 2,   // Activates the window and displays it as a minimized window.
        SW_MAXIMIZE = 3,        // Activates the window and displays it as a maximized window.
        SW_SHOWNOACTIVATE = 4,  // Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.
        SW_SHOW = 5,            // Activates the window and displays it in its current size and position.
        SW_MINIMIZE = 6,        // Minimizes the specified window and activates the next top-level window in the Z order.
        SW_SHOWMINNOACTIVE = 7, // Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
        SW_SHOWNA = 8,          // Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.
        SW_RESTORE = 9,         // Activates and displays the window. If the window is minimized, maximized, or arranged, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
        SW_SHOWDEFAULT = 10,    // Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application.
        SW_FORCEMINIMIZE = 11,  // Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
    }

    enum WINDOWS_MESSAGE : uint
    {
        WM_DESTROY = 0x0002,
        WM_PAINT = 0x000F,
        WM_CLOSE = 0x0010,
    }

    enum COLOR
    {
        CTLCOLOR_MSGBOX = 0,
        CTLCOLOR_EDIT = 1,
        CTLCOLOR_LISTBOX = 2,
        CTLCOLOR_BTN = 3,
        CTLCOLOR_DLG = 4,
        CTLCOLOR_SCROLLBAR = 5,
        CTLCOLOR_STATIC = 6,
        CTLCOLOR_MAX = 7,
        COLOR_SCROLLBAR = 0,
        COLOR_BACKGROUND = 1,
        COLOR_ACTIVECAPTION = 2,
        COLOR_INACTIVECAPTION = 3,
        COLOR_MENU = 4,
        COLOR_WINDOW = 5,
        COLOR_WINDOWFRAME = 6,
        COLOR_MENUTEXT = 7,
        COLOR_WINDOWTEXT = 8,
        COLOR_CAPTIONTEXT = 9,
        COLOR_ACTIVEBORDER = 10,
        COLOR_INACTIVEBORDER = 11,
        COLOR_APPWORKSPACE = 12,
        COLOR_HIGHLIGHT = 13,
        COLOR_HIGHLIGHTTEXT = 14,
        COLOR_BTNFACE = 15,
        COLOR_BTNSHADOW = 16,
        COLOR_GRAYTEXT = 17,
        COLOR_BTNTEXT = 18,
        COLOR_INACTIVECAPTIONTEXT = 19,
        COLOR_BTNHIGHLIGHT = 20,

        /* The following only available on Windows NT 4.0 or newer */
        COLOR_3DDKSHADOW = 21,
        COLOR_3DLIGHT = 22,
        COLOR_INFOTEXT = 23,
        COLOR_INFOBK = 24,
        COLOR_DESKTOP = COLOR_BACKGROUND,
        COLOR_3DFACE = COLOR_BTNFACE,
        COLOR_3DSHADOW = COLOR_BTNSHADOW,
        COLOR_3DHIGHLIGHT = COLOR_BTNHIGHLIGHT,
        COLOR_3DHILIGHT = COLOR_BTNHIGHLIGHT,
        COLOR_BTNHILIGHT = COLOR_BTNHIGHLIGHT,

        /* The following only available on Windows 2000 or newer */
        COLOR_HOTLIGHT = 26,
        COLOR_GRADIENTACTIVECAPTION = 27,
        COLOR_GRADIENTINACTIVECAPTION = 28,

        /* The following only available on Windows XP or newer */
        COLOR_MENUHILIGHT = 29,
        COLOR_MENUBAR = 30,
    }
}
