﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AutoMinesweeper
{
    public class Control
    {
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        static public Color GetPixelColor(IntPtr hwnd, int x, int y)
        {
            IntPtr hdc = GetDC(hwnd);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(hwnd, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }
        public static IntPtr MakeLParamFromXY(int x, int y)
        {
            return (IntPtr)((y << 16) | x);
        }
        public static IntPtr FindWindowHandle(string className, string windowName)
        {
            return FindWindow(className, windowName);
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT lpRect = default(RECT);
            GetWindowRect(hWnd, ref lpRect);
            return lpRect;
        }

        public static void ControlClick(IntPtr controlHandle, int x, int y)
        {
            IntPtr lParam = MakeLParamFromXY(x, y);
            PostMessage(controlHandle, 513, new IntPtr(0), lParam);
            PostMessage(controlHandle, 514, new IntPtr(0), lParam);
        }

        public static void SendKeyBoardDown(IntPtr handle, int key)
        {
            PostMessage(handle, 256, new IntPtr((int)key), new IntPtr(0));
        }
        public static int GetPixelFromWindow(string SType, string SValue, int X, int Y, bool PW = false)
        {
            int result = -1;
            using (WebClient client = new WebClient())
            {
                try
                {
                    string response = client.UploadString(
                        "http://localhost:2020/getPixelFromWindow",
                        "POST",
                        "{" +
                            $"\n\t\"SType\": \"{SType}\"," +
                            $"\n\t\"SValue\": \"{SValue}\"," +
                            $"\n\t\"X\": {X}," +
                            $"\n\t\"Y\": {Y}," +
                            $"\n\t\"PW\": {PW.ToString().ToLower()}" +
                        "}"
                    );
                    try
                    {
                        string pattern = @"{\""Color\"":\""(\w+)\""}";
                        Match m = Regex.Matches(response, pattern)[0];
                        result = Int32.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
                    }
                    catch
                    {
                        return result;
                    }
                }
                catch (WebException)
                {
                    return result;
                }
            }
            return result;
        }
        public static int GetPixelFromWindow(IntPtr hWnd, int x, int y)
        {
            Color color = GetPixelColor(hWnd, x, y);
            int result = 65536 * color.R + 256 * color.G + color.B;
            return result;
        }
    }
}