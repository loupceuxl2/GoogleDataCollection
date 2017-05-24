using System;
using GoogleDataCollection.DataAccess;

namespace GoogleDataCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            SpreadsheetAccess.LoadData(SpreadsheetAccess.DefaultFilename, 1);
        }
    }
}
