using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GoogleDataCollection.Model;
using Excel = Microsoft.Office.Interop.Excel;

// REFERENCE: https:coderwall.com/p/app3ya/read-excel-file-in-c
namespace GoogleDataCollection.DataAccess
{
    public static class SpreadsheetAccess
    {
        public enum ColumnIndex : byte { FId = 1, OsmId, HighwayName, HighwayType, OneWay, MaxSpeed, Length, FromX, FromY, ToX, ToY, XMid, YMid };

        //public static readonly string DefaultFilename = @"D:\Project 4\Programming\C#\GoogleDataCollection\GoogleDataCollection\bin\Debug\QLD_Network_Graph[Modified].csv";
        public static readonly string DefaultFilename = @"D:\Project1\Programming\C#\GoogleDataCollection\GoogleDataCollection\Data\QLD_Network_Graph.csv";

        private static Excel.Application _xlApp;
        private static Excel.Workbook _xlWorkbook;
        private static Excel._Worksheet _xlWorksheet;
        private static Excel.Range _xlRange;

        public static List<PointToPoint> LoadData(string filename, uint sheetNum)
        {
            var nodeCollection = new List<PointToPoint>(100000);

            var bar = new ShellProgressBar.ProgressBar(0, null, ConsoleColor.DarkRed);

            try
            {
                _xlApp = new Excel.Application();
                _xlWorkbook = _xlApp.Workbooks.Open(filename);
                _xlWorksheet = _xlWorkbook.Sheets[2];
                _xlRange = _xlWorksheet.UsedRange;

                var rowCount = _xlRange.Rows.Count;
                var colCount = _xlRange.Columns.Count;

                bar.UpdateMaxTicks(rowCount);

                // Iterate over the rows and columns and print to the console as it appears in the file.
                // Excel is not zero based!
                // Skip the header line.
                for (var i = 2; i <= rowCount; i++)
                {
                    var currentNode = new PointToPoint();

                    for (var j = 1; j <= colCount; j++)
                    {
                        // Null value check.
                        // More info: https://stackoverflow.com/questions/17359835/what-is-the-difference-between-text-value-and-value2
                        if (_xlRange.Cells[i, j] == null || _xlRange.Cells[i, j].Value2 == null)
                        {
                            Debug.WriteLine($"Data error: Null value found at [{i}, {j}].");
                            continue;
                        }

                        // TO DO: Try/Catch? Or just print to file.
                        switch (j)
                        {
                            case (int) ColumnIndex.FId:
                                currentNode.Fid = Convert.ToUInt32(_xlRange.Cells[i, j].Value2);
                                break;

                            case (int) ColumnIndex.OsmId:
                                currentNode.Fid = Convert.ToUInt32(_xlRange.Cells[i, j].Value2);
                                break;

                            case (int) ColumnIndex.HighwayName:
                                currentNode.Fid = _xlRange.Cells[i, j].Value2.ToString();
                                break;

                            case (int) ColumnIndex.HighwayType:
                                currentNode.Fid = _xlRange.Cells[i, j].Value2.ToString();
                                break;

                            case (int) ColumnIndex.OneWay:
                                currentNode.Fid = Convert.ToBoolean(Convert.ToInt32(_xlRange.Cells[i, j].Value2));
                                break;

                            case (int) ColumnIndex.MaxSpeed:
                                break;

                            case (int) ColumnIndex.Length:
                                break;

                            case (int) ColumnIndex.FromX:
                                break;

                            case (int) ColumnIndex.FromY:
                                break;

                            case (int) ColumnIndex.ToX:
                                break;

                            case (int) ColumnIndex.ToY:
                                break;

                            case (int) ColumnIndex.XMid:
                                break;

                            case (int) ColumnIndex.YMid:
                                break;

                            default:
                                Debug.WriteLine($"Data error: Unknown column found at [{i}, {j}].");
                                break;
                        }


/*
                                                
                        //write the value to the console
                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                            Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
*/

                    }

                    Debug.WriteLine($"ROW: {i}.");

                    bar.Tick();

                    if (i == 300)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                // TO DO: Add row, col number info.
                Debug.WriteLine($"");
            }
            // TO DO: Run on exit button click.
            finally
            {
                bar.Dispose();
                DisposeExcel();
            }

            return nodeCollection;
        }

        public static void DisposeExcel()
        {
            // Cleanup.
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Rule of thumb for releasing com objects:
            // Never use two dots, all COM objects must be referenced and released individually
            // E.g.: [somthing].[something].[something] is bad
            // Release com objects to fully kill excel process from running in the background.
            if (_xlRange != null) { Marshal.ReleaseComObject(_xlRange); }
            
            if (_xlWorksheet != null) { Marshal.ReleaseComObject(_xlWorksheet); }
            
            // Close and release.
            // REFERENCE: https://stackoverflow.com/questions/19977337/closing-excel-application-with-excel-interop-without-save-message
            if (_xlWorkbook != null)
            {
                object misValue = System.Reflection.Missing.Value;
                _xlWorkbook.Close(false, misValue, misValue);
                Marshal.ReleaseComObject(_xlWorkbook);
            }

            // Quit and release.
            if (_xlApp != null)
            {
                _xlApp.Quit();
                Marshal.ReleaseComObject(_xlApp);
            }
        }
    }
}
