using GoogleDataCollection.Model;
using System;
using System.IO;

namespace GoogleDataCollection.DataAccess
{
    public static class CsvAccess
    {
        public enum ColumnIndex : byte { FId = 0, OsmId, HighwayName, HighwayType, IsOneWay, MaxSpeed, Length, FromY, FromX, ToY, ToX, YMid, XMid };

        public static readonly string DefaultFilename = "QLD_Network_Graph.csv";
        public static uint ProgressHurdle = 2000;

        public static DataContainer ParseCsv()
        {
            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing CSV file { DefaultFilename } started.", Logging.Log.PriorityLevels.Medium));

            var container = new DataContainer()
            {
                CsvParsing = new CsvParsing()
                {
                    Time = DateTime.Now
                }
            };

            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultFilename}"))
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"CSV file { DefaultFilename } not found!", Logging.Log.PriorityLevels.UltraHigh));

                return container;
            }

            string[] lines;

            // (At least) a couple of ways of doing this.
            using (StreamReader reader = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultFilename}"))
            {
                // Skip the header.
                reader.ReadLine();

                lines = reader.ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (lines.Length == 0)
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"CSV file does not contain any edges!", Logging.Log.PriorityLevels.UltraHigh));

                return container;
            }

            var row = 0;
            var column = 0;
            var totalEdges = lines.Length;

            for (; row < totalEdges; row++)
            {
                try
                {
                    Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing row #{ row } started.", Logging.Log.PriorityLevels.Low));

                    var currentEdge = new Edge();
                    var parts = lines[row].Split(new char[] { ',' });

                    for (; column < parts.Length; column++)
                    {
                        ParseItem((uint)row, (uint)column, parts[column], currentEdge, container);
                    }

                    container.Edges.Add(currentEdge);

                    column = 0;

                    Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing row #{ row } completed successfully.", Logging.Log.PriorityLevels.Low));
                    Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing { row * 100 / totalEdges }% complete ({ row } of { totalEdges } edges processed).", Logging.Log.PriorityLevels.Low));

                    if (row % ProgressHurdle == 0)
                    {
                        Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing { row * 100 / totalEdges }% complete ({ row } of { totalEdges } edges processed).", Logging.Log.PriorityLevels.Medium));
                    }
                }
                catch (Exception e)
                {
                    Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing row #{ row } failed unexpectedly. Breaking the operation.{ Environment.NewLine }Error: { e.Message }", Logging.Log.PriorityLevels.UltraHigh));
                }
            }

            if (totalEdges > 0 && row == totalEdges)
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing { row * 100 / totalEdges }% complete ({ row } of { totalEdges } edges processed).", Logging.Log.PriorityLevels.Medium));
            }

            // Summary.


            return container;
        }
        private static void ParseItem(uint row, uint column, string value, Edge edge, DataContainer container)
        {
            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing { Enum.GetValues(typeof(ColumnIndex)).GetValue(column) } '{ value }' at [{ row }, { column }] started.", Logging.Log.PriorityLevels.UltraLow));

            try
            {
                switch (column)
                {
                    case (uint)ColumnIndex.FId:
                        edge.Fid = Convert.ToUInt32(value);
                        break;

                    case (uint)ColumnIndex.OsmId:
                        edge.OsmId = Convert.ToUInt32(value);
                        break;

                    case (uint)ColumnIndex.HighwayName:
                        edge.HighwayName = value;
                        break;

                    case (uint)ColumnIndex.HighwayType:
                        edge.HighwayType = value;
                        break;

                    case (uint)ColumnIndex.IsOneWay:
                        edge.IsOneWay = Convert.ToBoolean(Convert.ToUInt32(value));
                        break;

                    case (uint)ColumnIndex.MaxSpeed:
                        edge.MaxSpeed = Convert.ToUInt32(value);
                        break;

                    case (uint)ColumnIndex.Length:
                        edge.Length = Convert.ToDouble(value);
                        break;

                    case (uint)ColumnIndex.FromX:
                        edge.XFromPoint = Convert.ToDouble(value);
                        break;

                    case (uint)ColumnIndex.FromY:
                        edge.YFromPoint = Convert.ToDouble(value);
                        break;

                    case (uint)ColumnIndex.ToX:
                        edge.XToPoint = Convert.ToDouble(value);
                        break;

                    case (uint)ColumnIndex.ToY:
                        edge.YToPoint = Convert.ToDouble(value);
                        break;

                    case (uint)ColumnIndex.XMid:
                        edge.XMidPoint = Convert.ToDouble(value);
                        break;

                    case (uint)ColumnIndex.YMid:
                        edge.YMidPoint = Convert.ToDouble(value);
                        break;

                    default:

                        container.CsvParsing.Errors.Add(new CsvParsingError()
                        {
                            Row = row,
                            Column = column,
                            ErrorType = CsvParsingError.ErrorTypes.UnknownColumn
                        });

                        Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing value { value } at [{ row }, { column }] failed: Unknown column found.", Logging.Log.PriorityLevels.High));

                        return;
                }
            }
            catch (Exception e)
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Parsing value { value } at [{ row + 1 }, { column + 1 }] failed: Conversion failure.", Logging.Log.PriorityLevels.High));

                container.CsvParsing.Errors.Add(new CsvParsingError()
                {
                    Row = row,
                    Column = column,
                    ErrorType = CsvParsingError.ErrorTypes.ConversionError,
                    ExceptionMessage = e.Message
                });
            }
        }
    }
}
