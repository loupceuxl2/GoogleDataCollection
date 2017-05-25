using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GoogleDataCollection.DataAccess;

namespace GoogleDataCollection
{
    internal class Program
    {
        // REFERENCE: https://stackoverflow.com/questions/4646827/on-exit-for-a-console-application
        // Pinvoke.
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]

        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static ConsoleEventDelegate _handler;   // Keeps it from getting garbage collected.

        private static void Main(string[] args)
        {
            SpreadsheetAccess.LoadData(SpreadsheetAccess.DefaultFilename, 1);

            //Console.WriteLine($"BOOL CONVERSION OF 0: {Convert.ToBoolean(0)}");
            //Console.WriteLine($"BOOL CONVERSION OF 1: {Convert.ToBoolean(1)}");
            //Console.WriteLine($"BOOL CONVERSION OF -1: {Convert.ToBoolean(-1)}");
        }

        private static bool ConsoleEventCallback(int eventType)
        {
            Console.WriteLine("CLOSING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");


            
            return false;
        }
    }
}
