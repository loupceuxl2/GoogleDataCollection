using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoogleDataCollection.DataAccess
{
    // TO DO: DELETE!
    // GDC1: AIzaSyD_EFI7UTnUSKJk_R8_66tDD0_XHEujQVc
    // GDC2: AIzaSyCAJzU9R8Y8UgtD1QoUHswUgRjnLMA7VJ4
    // GDC3: AIzaSyCtoG6JK_SAu_On2rW4fZ_Wypp3K-xZ1WI
    public static class CsvAccess
    {
        public enum ColumnIndex : byte { FId = 0, OsmId, HighwayName, HighwayType, IsOneWay, MaxSpeed, Length, FromY, FromX, ToY, ToX, YMid, XMid };

        public static readonly string DefaultCsvFilename = "QLD_Network_Graph.csv";
        public static readonly string DefaultReportFilename = "report_by_update_time.csv";
        public static uint ProgressHurdle = 5000;

        private static DataContainer ParseCsv(string filename)
        {
            Log.GlobalLog.AddToLog(new LogMessage($"Parsing CSV file '{ filename }' started.", Log.PriorityLevels.Medium));

            var container = new DataContainer()
            {
                CsvParsing = new CsvParsing()
                {
                    Time = DateTime.Now
                }
            };

            if (!File.Exists($"{ filename }"))
            {
                Log.GlobalLog.AddToLog(new LogMessage($"CSV file '{ filename }' not found! Aborting operation.", Log.PriorityLevels.UltraHigh));

                return container;
            }

            string[] lines;

            // (At least) a couple of ways of doing this.
            using (StreamReader reader = new StreamReader($"{ filename }"))
            {
                // Skip the header.
                reader.ReadLine();

                lines = reader.ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (lines.Length == 0)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"CSV file does not contain any edges!", Log.PriorityLevels.UltraHigh));

                return container;
            }

            var row = 0;
            var column = 0;
            var totalEdges = lines.Length;

            for (; row < totalEdges; row++)
            {
                try
                {
                    Log.GlobalLog.AddToLog(new LogMessage($"Parsing row #{ row } started.", Log.PriorityLevels.Low));

                    var currentEdge = new Edge();
                    var parts = lines[row].Split(new char[] { ',' });

                    for (; column < parts.Length; column++)
                    {
                        ParseItem((uint)row, (uint)column, parts[column], currentEdge, container);
                    }

                    currentEdge.Id = Edge.GenerateId(currentEdge.Fid, Edge.EdgeDirections.Forwards);
                    container.Edges.Add(currentEdge);

                    if (!currentEdge.IsOneWay)
                    {
                        container.Edges.Add(Edge.CreateReverseEdge(currentEdge));
                    }

                    column = 0;

                    Log.GlobalLog.AddToLog(new LogMessage($"Parsing row #{ row } completed successfully.", Log.PriorityLevels.Low));
                    Log.GlobalLog.AddToLog(new LogMessage($"Parsing { row * 100 / totalEdges }% complete ({ row } of { totalEdges } edges processed).", Log.PriorityLevels.Low));

                    if (row % ProgressHurdle == 0)
                    {
                        Log.GlobalLog.AddToLog(new LogMessage($"Parsing { row * 100 / totalEdges }% complete ({ row } of { totalEdges } edges processed).", Log.PriorityLevels.Medium));
                    }
                }
                catch (Exception e)
                {
                    Log.GlobalLog.AddToLog(new LogMessage($"Parsing row #{ row } failed unexpectedly. Breaking the operation.{ Environment.NewLine }Error: { e.Message }", Log.PriorityLevels.UltraHigh));
                }
            }

            if (totalEdges > 0 && row == totalEdges)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"Parsing { row * 100 / totalEdges }% complete ({ row } of { totalEdges } edges processed).", Log.PriorityLevels.Medium));
            }

            return container;
        }

        public static DataContainer ParseCsv()
        {
            return ParseCsv($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultCsvFilename }");
        }
        
        private static void ParseItem(uint row, uint column, string value, Edge edge, DataContainer container)
        {
            Log.GlobalLog.AddToLog(new LogMessage($"Parsing { Enum.GetValues(typeof(ColumnIndex)).GetValue(column) } '{ value }' at [{ row }, { column }] started.", Log.PriorityLevels.UltraLow));

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
                        edge.HighwayType = Edge.GetHighwayType(value);
                        break;

                    case (uint)ColumnIndex.IsOneWay:
                        edge.IsOneWay = Convert.ToBoolean(Convert.ToUInt32(value));
                        break;

                    case (uint)ColumnIndex.MaxSpeed:
                        edge.MaxSpeed = Convert.ToUInt32(value);
                        if (edge.MaxSpeed == 0) { edge.MaxSpeed = null; }
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

                        Log.GlobalLog.AddToLog(new LogMessage($"Parsing value { value } at [{ row }, { column }] failed: Unknown column found.", Log.PriorityLevels.High));

                        return;
                }
            }
            catch (Exception e)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"Parsing value { value } at [{ row }, { column }] failed: Conversion failure.", Log.PriorityLevels.High));

                container.CsvParsing.Errors.Add(new CsvParsingError()
                {
                    Row = row,
                    Column = column,
                    ErrorType = CsvParsingError.ErrorTypes.ConversionError,
                    ExceptionMessage = e.Message
                });
            }
        }

        private static void GenerateCsvReportGroupedByUpdateTime(DataContainer data, string filename)
        {
            Log.GlobalLog.AddToLog(new LogMessage($"Generating CSV report started -- grouping edges by update hour.", Log.PriorityLevels.Medium));

            if (!File.Exists($"{ filename }"))
            {
                Log.GlobalLog.AddToLog(new LogMessage($"CSV file '{ filename }' not found! Aborting operation.", Log.PriorityLevels.UltraHigh));

                return;
            }

            var reportData = data.Edges
                .SelectMany(e => e.Updates)                                     // Flatten all updates (EdgeUpdates).
                .GroupBy(u => u.UpdateHour)                                     // Group by hour.
                .Select(h => new
                {
                    Hour = h.Key,
                    Edges = data.Edges                                          // Look into related edges.
                            .Where(e => e.Updates                               // Check the updates:
                                .Where(u => u.GoogleDuration != null)               // have a duration (i.e., is not null).
                                .Count(u => u.UpdateHour == h.Key) > 0)             // have gt 0 updates.
                            .Select(e => new { Hour = h.Key, Edge = e, LatestDuration = e.Updates.Last(u => u.UpdateHour == h.Key).GoogleDuration })        // Transform into Hour, Edge, LatestDuration
                            .OrderBy(e => e.Hour)                               // Order by hour (nested/transformed one).
                            .ThenBy(e => e.Edge.Fid)                            // and then by Fid.
                            .ToList()                                           // To allow Linq 'ForEach' (see below).
                })
                .ToList();                                                      // To allow Linq 'ForEach' (see below).

            var separator = ',';
            var report = $"Hour{ separator }LatestDuration{ separator }{ Edge.GenerateCsvHeader(separator) }{ Environment.NewLine }";

            reportData.ForEach(g => g.Edges.ForEach(e => report += GenerateGroupedByUpdateTimeRow(separator, e.Hour, e.Edge, e.LatestDuration) + Environment.NewLine));

            Log.GlobalLog.AddToLog(new LogMessage($"Report generated. Writing report to file '{ filename }' started.", Log.PriorityLevels.Medium));
            File.WriteAllText($"{ filename }", report);
            Log.GlobalLog.AddToLog(new LogMessage($"Writing report to file '{ filename }' completed.", Log.PriorityLevels.Medium));
        }

        public static void GenerateCsvReportGroupedByUpdateTime(DataContainer data)
        {
            GenerateCsvReportGroupedByUpdateTime(data, $"{ AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultReportFilename }");
        }

        private static string GenerateGroupedByUpdateTimeRow(char separator, uint hour, Edge edge, TimeSpan? latestDuration)
        {
            var row = new List<string>
            {
                hour.ToString(),
                latestDuration?.ToString() ?? string.Empty,
                edge.Id,
                edge.Fid.ToString(),
                edge.OsmId.ToString(),
                edge.HighwayName ?? string.Empty,
                edge.HighwayType.ToString(),
                edge.IsOneWay.ToString(),
                edge.MaxSpeed?.ToString() ?? string.Empty,
                edge.Length.ToString(),
                edge.XFromPoint.ToString(),
                edge.YFromPoint.ToString(),
                edge.XToPoint.ToString(),
                edge.YToPoint.ToString(),
                edge.XMidPoint.ToString(),
                edge.YMidPoint.ToString()
            };

            return string.Join(separator.ToString(), row);
        }
    }
}
