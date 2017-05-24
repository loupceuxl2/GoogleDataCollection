using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoogleDataCollection.Model;
using Excel = Microsoft.Office.Interop.Excel;

// REFERENCE: https:coderwall.com/p/app3ya/read-excel-file-in-c
namespace GoogleDataCollection.DataAccess
{
    public static class SpreadsheetAccess
    {
        public enum ColumnIndex : byte { FId = 1, OsmId, Name, Type, OneWay, MaxSpeed, Length, FromX, FromY, ToX, ToY, XMid, YMid };

        public static readonly string DefaultFilename = @"D:\Project 4\Programming\C#\GoogleDataCollection\GoogleDataCollection\bin\Debug\QLD_Network_Graph[Modified].csv";

        public static List<PointToPoint> LoadData(string filename, uint sheetNum)
        {
            const uint PROGRESS_RATIO = 1000;
            var nodeCollection = new List<PointToPoint>(100000);

            try
            {
                //Console.WriteLine(Environment.CurrentDirectory);
                //return;
                
                // Create COM Objects. Create a COM object for everything that is referenced.
                //Console.WriteLine(((int)ColumnIndex.FId).ToString());
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filename);
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[sheetNum];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;

                // Iterate over the rows and columns and print to the console as it appears in the file.
                // Excel is not zero based!
                for (int i = 1; i <= rowCount; i++)
                {
                    var currentNode = new PointToPoint();

                    // Skip the header line.
                    if (i == 1) { continue;  }


                    for (int j = 1; j <= colCount; j++)
                    {
                        // Null value check.
                        // More info: https://stackoverflow.com/questions/17359835/what-is-the-difference-between-text-value-and-value2
                        if (xlRange.Cells[i, j] == null || xlRange.Cells[i, j].Value2 == null)
                        {
                            Console.WriteLine($"Data error: Null value found at [{i}, {j}].");
                            return null;
                        }

                        switch (j)
                        {
                            case (int)ColumnIndex.FId:
                                break;

                            case (int)ColumnIndex.OsmId:
                                break;

                            case (int)ColumnIndex.Name:
                                break;

                            case (int)ColumnIndex.Type:
                                break;

                            case (int)ColumnIndex.OneWay:
                                break;

                            case (int)ColumnIndex.MaxSpeed:
                                break;

                            case (int)ColumnIndex.Length:
                                break;

                            case (int)ColumnIndex.FromX:
                                break;

                            case (int)ColumnIndex.FromY:
                                break;

                            case (int)ColumnIndex.ToX:
                                break;

                            case (int)ColumnIndex.ToY:
                                break;

                            case (int)ColumnIndex.XMid:
                                break;

                            case (int)ColumnIndex.YMid:
                                break;

                            default:
                                Console.WriteLine($"Data error: Unknown column found at [{i}, {j}].");
                                break;
                        }

                        
/*
                        //new line
                        if (j == 1)
                            Console.Write("\r\n");

                        //write the value to the console
                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                            Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
*/
                        if (j % PROGRESS_RATIO == 0)
                        {
                            //Console.WriteLine($"{} of  [{i}, {j}].");
                        }
                    }

                    Console.Write("█");

                    if (i == 5) { break; }
                }

                //Cleanup.
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Rule of thumb for releasing com objects:
                // Never use two dots, all COM objects must be referenced and released individually
                // E.g.: [somthing].[something].[something] is bad

                // Release com objects to fully kill excel process from running in the background.
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                //Close and release.
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                // Quit and release.
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return nodeCollection;
        }
    }
}
