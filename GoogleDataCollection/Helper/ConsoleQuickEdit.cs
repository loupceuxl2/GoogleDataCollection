using System;
using System.Runtime.InteropServices;

namespace GoogleDataCollection.Helper
{
    // REFERENCE: https://stackoverflow.com/questions/13656846/how-to-programmatic-disable-c-sharp-console-applications-quick-edit-mode
    public static class ConsoleQuickEdit
    {
        private const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        private const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static uint? GetConsoleMode()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // Get current console mode.
            if (!GetConsoleMode(consoleHandle, out uint consoleMode))
            {
                // ERROR: Unable to get console mode.
                return null;
            }

            return consoleMode;
        }

        public static bool SetConsoleMode(uint consoleMode)
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // Set the new mode.
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode.
                return false;
            }

            return true;
        }

        internal static bool Enable()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // Get current console mode.
            if (!GetConsoleMode(consoleHandle, out uint consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Add the quick edit bit in the mode flags.
            consoleMode &= ENABLE_QUICK_EDIT;

            // Set the new mode.
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode.
                return false;
            }

            return true;
        }

        internal static bool Disable()
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // Get current console mode.
            if (!GetConsoleMode(consoleHandle, out uint consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags.
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // Set the new mode.
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode.
                return false;
            }

            return true;
        }
    }
}
