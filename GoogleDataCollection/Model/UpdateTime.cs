using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    // DONE: Test 0 or 24 hour; Result = 0.
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateTime
    {
        public static List<UpdateTime> DefaultUpdateTimes = new List<UpdateTime>()
        {
            new UpdateTime("Morning on peak", 7),
            new UpdateTime("Morning off peak", 10),
            new UpdateTime("Afternoon on peak 1", 14),
            new UpdateTime("Afternoon on peak 2", 18)
        };

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "hourRunTime", Required = Required.Always)]
        public uint HourRunTime { get; set; }

        public UpdateTime()
        {
            
        }

        public UpdateTime(string name, uint hourRunTime)
        {
            Name = name;
            HourRunTime = hourRunTime;
        }

        // 0-23, i.e., midnight to eleven PM.
        // REFERENCE: https://stackoverflow.com/questions/42724629/get-time-until-next-occurrence-of-6pm
        public static DateTime GetNextOccurrence(int hour)
        {
            var today = DateTime.Now;
            var tomorrow = today.Add(new TimeSpan(1, 0, 0, 0));
            var tomorrowAtHour = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, hour, 0, 0);
            var diff = tomorrowAtHour.Subtract(DateTime.Now);

            var minutesFromNow = 0d;

            if (diff.TotalHours > 24d)
            {
                //hoursFromNow = diff.TotalHours - 24d;
                minutesFromNow = diff.TotalMinutes - (24d * 60d);
            }
            else
            {
                //hoursFromNow = diff.TotalHours;
                minutesFromNow = diff.TotalMinutes;
            }

            return today.Add(new TimeSpan(0, 0, (int)Math.Ceiling(minutesFromNow), 0));
        }

        // REFERENCE: https://stackoverflow.com/questions/3354893/how-can-i-convert-a-datetime-to-the-number-of-seconds-since-1970
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            
            return Math.Ceiling(diff.TotalSeconds);
        }
    }
}
