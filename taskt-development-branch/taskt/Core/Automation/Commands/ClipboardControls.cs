using System;
using System.Runtime.InteropServices;

namespace taskt.Core.Automation.Commands
{
    /// <summary>
    /// for clipboard methods
    /// </summary>
    static internal class ClipboardControls
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll")]
        private static extern bool SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;

        /// <summary>
        /// set text to clipboard
        /// </summary>
        /// <param name="textToSet"></param>
        public static void SetClipboardText(string textToSet)
        {
            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            var ptr = Marshal.StringToHGlobalUni(textToSet);
            SetClipboardData(13, ptr);
            CloseClipboard();
        }

        /// <summary>
        /// clear clipboard value
        /// </summary>
        public static void ClearClipboard()
        {
            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            CloseClipboard();
        }

        /// <summary>
        /// get text from clipboard
        /// </summary>
        /// <returns></returns>
        public static string GetClipboardText()
        {
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
            {
                return null;
            }
                
            if (!OpenClipboard(IntPtr.Zero))
            {
                return null;
            }

            string data = null;
            var hGlobal = GetClipboardData(CF_UNICODETEXT);
            if (hGlobal != IntPtr.Zero)
            {
                var lpwcstr = GlobalLock(hGlobal);
                if (lpwcstr != IntPtr.Zero)
                {
                    data = Marshal.PtrToStringUni(lpwcstr);
                    GlobalUnlock(lpwcstr);
                }
            }
            CloseClipboard();

            return data;
        }
    }
}
