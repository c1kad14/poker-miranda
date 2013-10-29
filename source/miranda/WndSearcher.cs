using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
namespace miranda
{
    public static class WndSearcher
    {
        public static IntPtr SearchForWindow(string wndclass, string title)
        {
            SearchData sd = new SearchData { Wndclass = wndclass, Title = title };
            EnumWindows(new EnumWindowsProc(EnumProc), ref sd);
            return sd.hWnd;
        }

        public static bool EnumProc(IntPtr hWnd, ref SearchData data)
        {
            // Check classname and title 
            // This is different from FindWindow() in that the code below allows partial matches
            StringBuilder sb = new StringBuilder(1024);
            GetClassName(hWnd, sb, sb.Capacity);
            //if (sb.ToString().StartsWith(data.Wndclass))
            {
                sb = new StringBuilder(1024);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().Contains(data.Title))
                {
                    data.hWnd = hWnd;
                    return false;    // Found the wnd, halt enumeration
                }
            }
            return true;
        }

        public class SearchData
        {
            // You can put any vars in here...
            public string Wndclass;
            public string Title;
            public IntPtr hWnd;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        class WinData
        {
            public IntPtr Hwnd;
            public string Class;
        }

        static WinData GetWindow(IntPtr parent, string className)
        {
            //List<IntPtr> result = new List<IntPtr>();
            var result = new WinData { Class = className, Hwnd = IntPtr.Zero };
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new Win32Callback(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
                return result;
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
        }
        public static IntPtr FindOurWindow()
        {

            // msctls_statusbar32
            IntPtr ptr = WndSearcher.SearchForWindow("InfoClass", "");

            if (ptr == IntPtr.Zero)
                return IntPtr.Zero;

            var mdi = GetWindow(ptr, "MDIClient");
            //Logger.Info("MDIClient " + mdi.Hwnd);

            if (mdi.Hwnd == IntPtr.Zero)
                return IntPtr.Zero;

            var inf = GetWindow(mdi.Hwnd, "InfoNews");
            //Logger.Info("InfoNews " + inf.Hwnd);

            if (inf.Hwnd == IntPtr.Zero)
                return IntPtr.Zero;

            var res = GetWindow(inf.Hwnd, "SysListView32");
            //Logger.Info("SysListView32 " + res.Hwnd);


            if (res.Hwnd == IntPtr.Zero)
                return IntPtr.Zero;

            /*
            IntPtr mdi = FindWindowEx(ptr, IntPtr.Zero, "MDIClient", string.Empty);

            LoggingLib.Logger.Info(mdi.ToInt32().ToString());

            IntPtr res = FindWindowEx(mdi, IntPtr.Zero, "InfoNews", string.Empty);

            LoggingLib.Logger.Info(res.ToInt32().ToString());

            IntPtr win = FindWindowEx(res, IntPtr.Zero, "SysListView32", string.Empty);
            //*/
            return res.Hwnd;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            var list = gch.Target as WinData;

            int nRet;
            StringBuilder ClassName = new StringBuilder(100);
            //Get the window class name
            nRet = GetClassName(handle, ClassName, ClassName.Capacity);
            if (nRet != 0)
            {
                if (string.Compare(ClassName.ToString(), list.Class, true, CultureInfo.InvariantCulture) == 0)
                    list.Hwnd = handle;
            }


            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }


        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

    }
}
