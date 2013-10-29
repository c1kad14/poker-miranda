using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.Text;

namespace miranda.ui
{
    public class WinApi
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "";
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);


        const uint MAPVK_VK_TO_VSC = 0x00;
        const uint MAPVK_VSC_TO_VK = 0x01;
        const uint MAPVK_VK_TO_CHAR = 0x02;
        const uint MAPVK_VSC_TO_VK_EX = 0x03;
        const uint MAPVK_VK_TO_VSC_EX = 0x04;

        const int KEYEVENTF_EXTENDEDKEY = 0x1;
        const int KEYEVENTF_KEYUP = 0x2;

        public enum KeyCode: byte
        {
            _0 = 0x30,
            _1 = 0x31,
            _2 = 0x32,
            _3 = 0x33,
            _4 = 0x34,
            _5 = 0x35,
            _6 = 0x36,
            _7 = 0x37,
            _8 = 0x38,
            _9 = 0x39,
            comma = 0xBC,
            dec = Keys.Decimal
        }

        
        public static void TypeNumber(decimal number)
        {
            var str = number.ToString(CultureInfo.InvariantCulture);

            foreach (var c in str)
            {
                if (c == '0') PressKey(KeyCode._0);
                if (c == '1') PressKey(KeyCode._1);
                if (c == '2') PressKey(KeyCode._2);
                if (c == '3') PressKey(KeyCode._3);
                if (c == '4') PressKey(KeyCode._4);
                if (c == '5') PressKey(KeyCode._5);
                if (c == '6') PressKey(KeyCode._6);
                if (c == '7') PressKey(KeyCode._7);
                if (c == '8') PressKey(KeyCode._8);
                if (c == '9') PressKey(KeyCode._9);
                if (c == ',') PressKey(KeyCode.dec);
                if (c == '.') PressKey(KeyCode.dec);

                
                var val = _random.Next(100, 300);
                Thread.Sleep(val);
            }
        }

        static readonly Random _random = new Random();

        private static void PressKey(KeyCode keyCode)
        {
            keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), 0, UIntPtr.Zero);
            keybd_event((byte)keyCode, (byte)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC), KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static void LinearSmoothMove(Point newPosition, TimeSpan duration)
        {
            Point start = Cursor.Position;

            var rnd = new Random();
            // Find the vector between start and newPosition
            double deltaX = newPosition.X - start.X;
            double deltaY = newPosition.Y - start.Y;

            // start a timer
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            double timeFraction = 0.0;

            do
            {
                var v = rnd.Next(0, 1000);
                Trace.WriteLine(stopwatch.Elapsed.Ticks + ", " + v + ", " + (double)(stopwatch.Elapsed.Ticks + v) / duration.Ticks);
                timeFraction = (double)(stopwatch.Elapsed.Ticks + v * 5) / duration.Ticks;
                if (timeFraction > 1.0)
                    timeFraction = 1.0;

                var addX = rnd.Next(0, 10);
                var addY = rnd.Next(0, 10);
                var curPoint = new Point(start.X + (int)(timeFraction * deltaX) + addX,
                                             start.Y + (int)(timeFraction * deltaY) + addY);
                Cursor.Position = curPoint;

                Thread.Sleep(20);
            } while (timeFraction < 1.0);

            Cursor.Position = newPosition;
        }

        public static void MoveOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var newPos = new Point(clientPoint.X, clientPoint.Y);
            LinearSmoothMove(newPos, TimeSpan.FromMilliseconds(200));
        }
        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPos = Cursor.Position;

            // get screen coordinates
            //ClientToScreen(wndHandle, ref clientPoint);

            var newPos = new Point(clientPoint.X, clientPoint.Y);

            // set cursor on coords, and press mouse
            LinearSmoothMove(newPos, TimeSpan.FromMilliseconds(200));
            //Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            mouse_event(0x00000002, 0, 0, 0, UIntPtr.Zero); // left mouse button down
            mouse_event(0x00000004, 0, 0, 0, UIntPtr.Zero); // left mouse button up

            // return mouse 
#if DEBUG
            Cursor.Position = oldPos;
#endif
        } 
    }
}