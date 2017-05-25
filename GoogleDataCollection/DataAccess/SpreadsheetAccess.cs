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
        // TO DO: Change to reflect inversion of x & y's.
        public enum ColumnIndex : byte { FId = 1, OsmId, HighwayName, HighwayType, IsOneWay, MaxSpeed, Length, FromX, FromY, ToX, ToY, XMid, YMid };

        //public static readonly string DefaultFilename = @"D:\Project1\Programming\C#\GoogleDataCollection\GoogleDataCollection\Data\QLD_Network_Graph.csv";
        public static readonly string DefaultFilename = @"D:\Project4\Programming\C#\GoogleDataCollection\GoogleDataCollection\Data\QLD_Network_Graph.csv";

        private static Excel.Application _xlApp;
        private static Excel.Workbook _xlWorkbook;
        private static Excel._Worksheet _xlWorksheet;
        private static Excel.Range _xlRange;

        public static PointToPointContainer LoadData(string filename, uint sheetNum)
        {
            //var nodeCollection = new List<PointToPoint>(100000);
            var container = new PointToPointContainer();

            try
            {
                _xlApp = new Excel.Application();
                _xlWorkbook = _xlApp.Workbooks.Open(filename);
                _xlWorksheet = _xlWorkbook.Sheets[1];
                _xlRange = _xlWorksheet.UsedRange;

                var rowCount = _xlRange.Rows.Count;
                var colCount = _xlRange.Columns.Count;
                var errors = new ErrorContainer();

                ShellProgressBar.ProgressBar bar = new ShellProgressBar.ProgressBar(rowCount, null, ConsoleColor.DarkRed);

                // Iterate over the rows and columns and print to the console as it appears in the file.
                // Excel is not zero based!
                // Skip the header line.
                for (var i = 2; i <= rowCount; i++)
                {
                    try
                    {
                        var currentNode = new PointToPoint();

                        for (var j = 1; j <= colCount; j++)
                        {
                            // Null value check.
                            // More info: https://stackoverflow.com/questions/17359835/what-is-the-difference-between-text-value-and-value2
                            if (_xlRange.Cells[i, j] == null || _xlRange.Cells[i, j].Value2 == null)
                            {
                                //errorMessage = $"Data error: Null value found at [{i}, {j}].";
                                //Debug.WriteLine(errorMessage);
                                //File.WriteAllLines($"error_report_{ DateTime.Now.ToShortDateString() }", new string[] { errorMessage });
                                continue;
                            }

                            // TO DO: Try/Catch? Or just print to file.
                            switch (j)
                            {
                                case (int)ColumnIndex.FId:
                                    currentNode.Fid = Convert.ToUInt32(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.OsmId:
                                    currentNode.OsmId = Convert.ToUInt32(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.HighwayName:
                                    currentNode.HighwayName = _xlRange.Cells[i, j].Value2.ToString();
                                    break;

                                case (int)ColumnIndex.HighwayType:
                                    currentNode.HighwayType = _xlRange.Cells[i, j].Value2.ToString();
                                    break;

                                case (int)ColumnIndex.IsOneWay:
                                    currentNode.IsOneWay = Convert.ToBoolean(Convert.ToInt32(_xlRange.Cells[i, j].Value2));
                                    break;

                                case (int)ColumnIndex.MaxSpeed:
                                    currentNode.MaxSpeed = Convert.ToUInt32(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.Length:
                                    currentNode.Length = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.FromX:
                                    currentNode.XFromPoint = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.FromY:
                                    currentNode.YFromPoint = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.ToX:
                                    currentNode.XToPoint = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.ToY:
                                    currentNode.YToPoint = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.XMid:
                                    currentNode.XMidPoint = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                case (int)ColumnIndex.YMid:
                                    currentNode.YMidPoint = Convert.ToDouble(_xlRange.Cells[i, j].Value2);
                                    break;

                                default:
                                    //Debug.WriteLine($"Data error: Unknown column found at [{i}, {j}].");
                                    break;
                            }

                            if (j == colCount)
                            {
                                container.PointToPoints.Add(currentNode);
                            }

                        }

                        bar.Tick();

                        if (i == 100)
                        {
                            bar.Dispose();
                            return container;
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {

            }
            // TO DO: Run on exit button click.
            finally
            {
                DisposeExcel();
            }

            return container;
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
/*
    public class InvalidExcelDataException : Exception
    {
        public InvalidExcelDataException()
        {

        }

        public InvalidExcelDataException(string message) : base(message)
        {

        }

        public InvalidExcelDataException(string message, Exception inner) : base(message, inner)
        {

        }
    }
*/
}
