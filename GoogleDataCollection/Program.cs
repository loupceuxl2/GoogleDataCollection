using GoogleDataCollection.DataAccess;
using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GoogleDataCollection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = SpreadsheetAccess.LoadData(SpreadsheetAccess.DefaultFilename, 1);

            string output = JsonConvert.SerializeObject(container, Formatting.Indented);

            File.WriteAllText(JsonAccess.DefaultFilename, output);

            //var test = JsonAccess.DeserializePointToPoints();

            //Console.WriteLine(test.PointToPoints.Count);

            //Console.WriteLine($"BOOL CONVERSION OF 0: {Convert.ToBoolean(0)}");
            //Console.WriteLine($"BOOL CONVERSION OF 1: {Convert.ToBoolean(1)}");
            //Console.WriteLine($"BOOL CONVERSION OF -1: {Convert.ToBoolean(-1)}");
        }
    }
}
