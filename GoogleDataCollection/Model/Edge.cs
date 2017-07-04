using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Edge
    {
        // REFERENCE: http://wiki.openstreetmap.org/wiki/Key:highway
        public enum HighwayTypes : uint {
            Motorway = 1,       // 1
            Trunk,              // 2
            Primary,            // 3
            Secondary,          // 4
            Tertiary,           // 5
            Unclassified,       // 6
            Residential,        // 7
            Service,            // 8
            MotorwayLink,       // 9
            TrunkLink,          // 10
            PrimaryLink,        // 11
            SecondaryLink,      // 12
            TertiaryLink,       // 13
            LivingStreet,       // 14
            Pedestrian,         // 15
            Track,              // 16
            BusGuideway,        // 17
            Escape,             // 18
            Raceway,            // 19
            Road,               // 20
            Footway,            // 21
            Bridleway,          // 22
            Steps,              // 23
            Path,               // 24
            Cycleway,           // 25
            Construction,       // 26
            BusStop,            // 27
            Crossing,           // 28
            Elevator,           // 29
            EmergencyAccessPoint, // 30
            GiveWay,            // 31
            MiniRoundabout,     // 32
            MotorwayJunction,   // 33
            PassingPlace,       // 34
            RestArea,           // 35
            SpeedCamera,        // 36
            StreetLamp,         // 37
            Services,           // 38
            Stop,               // 39
            TrafficSignals,     // 40
            TurningCircle,      // 41
            PedestrianOverpass, // 42
            Escalator,          // 43
            Private,            // 44 ???
            Unsurfaced,         // 45
            Minor,              // 46
            Ford,               // 47
            ClosedTrunk,        // 48
            Unknown
        }

        public static Dictionary<string, HighwayTypes> HighwayTypeNames = new Dictionary<string, HighwayTypes>
        {
            { "motorway", HighwayTypes.Motorway },
            { "trunk", HighwayTypes.Trunk },
            { "primary", HighwayTypes.Primary },
            { "secondary", HighwayTypes.Secondary },
            { "tertiary", HighwayTypes.Tertiary },
            { "unclassified", HighwayTypes.Unclassified },
            { "residential", HighwayTypes.Residential },
            { "service", HighwayTypes.Service },
            { "motorway_link", HighwayTypes.MotorwayLink },
            { "trunk_link", HighwayTypes.TrunkLink },
            { "primary_link", HighwayTypes.PrimaryLink },
            { "secondary_link", HighwayTypes.SecondaryLink },
            { "tertiary_link", HighwayTypes.TertiaryLink },
            { "living_street", HighwayTypes.LivingStreet },
            { "pedestrian", HighwayTypes.Pedestrian },
            { "track", HighwayTypes.Track },
            { "bus_guidway", HighwayTypes.BusGuideway },
            { "escape", HighwayTypes.Escape },
            { "raceway", HighwayTypes.Raceway },
            { "road", HighwayTypes.Road },
            { "footway", HighwayTypes.Footway },
            { "bridleway", HighwayTypes.Bridleway },
            { "steps", HighwayTypes.Steps },
            { "path", HighwayTypes.Path },
            { "cycleway", HighwayTypes.Cycleway },
            { "construction", HighwayTypes.Construction },
            { "bus_stop", HighwayTypes.BusStop },
            { "crossing", HighwayTypes.Crossing },
            { "elevator", HighwayTypes.Elevator },
            { "emergency_access_point", HighwayTypes.Elevator },
            { "give_way", HighwayTypes.GiveWay },
            { "mini_roundabout", HighwayTypes.MiniRoundabout },
            { "motorway_junction", HighwayTypes.MotorwayJunction },
            { "passing_place", HighwayTypes.PassingPlace },
            { "rest_area", HighwayTypes.RestArea },
            { "speed_camera", HighwayTypes.SpeedCamera },
            { "street_lamp", HighwayTypes.StreetLamp },
            { "services", HighwayTypes.Services },
            { "stop", HighwayTypes.Stop },
            { "traffic_signals", HighwayTypes.TrafficSignals },
            { "turning_circle", HighwayTypes.TurningCircle },
            { "pedestrian_overp", HighwayTypes.PedestrianOverpass },
            { "escalator", HighwayTypes.Escalator },
            { "private", HighwayTypes.Private },
            { "unsurfaced", HighwayTypes.Unsurfaced },
            { "minor", HighwayTypes.Minor },
            { "ford", HighwayTypes.Ford },
            { "closed:trunk", HighwayTypes.ClosedTrunk },
            { "unclassifed", HighwayTypes.Unclassified },            // Typo in data.
            { "unknown", HighwayTypes.Unknown }
        };

        public enum EdgeDirections : byte { Forwards, Backwards }

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "fId", Required = Required.Always)]
        public uint Fid { get; set; }

        [JsonProperty(PropertyName = "osmId", Required = Required.Always)]
        public uint OsmId { get; set; }

        [JsonProperty(PropertyName = "highwayName", Required = Required.AllowNull)]
        public string HighwayName { get; set; }

        // DONE [OPTIONAL]: Change to an enum.
        [JsonProperty(PropertyName = "highwayType", Required = Required.Always)]
        public HighwayTypes? HighwayType { get; set; }

        [JsonProperty(PropertyName = "isOneWay", Required = Required.Always)]
        public bool IsOneWay { get; set; }

        [JsonProperty(PropertyName = "maxSpeed", Required = Required.AllowNull)]
        public uint? MaxSpeed { get; set; }

        [JsonProperty(PropertyName = "length", Required = Required.Always)]
        public double Length { get; set; }

        [JsonProperty(PropertyName = "xFromPoint", Required = Required.Always)]
        public double XFromPoint { get; set; }

        [JsonProperty(PropertyName = "yFromPoint", Required = Required.Always)]
        public double YFromPoint { get; set; }

        [JsonProperty(PropertyName = "xToPoint", Required = Required.Always)]
        public double XToPoint { get; set; }

        [JsonProperty(PropertyName = "yToPoint", Required = Required.Always)]
        public double YToPoint { get; set; }

        [JsonProperty(PropertyName = "xMidPoint", Required = Required.Always)]
        public double XMidPoint { get; set; }

        [JsonProperty(PropertyName = "yMidPoint", Required = Required.Always)]
        public double YMidPoint { get; set; }

        [JsonProperty(PropertyName = "updates", Required = Required.Always)]
        public List<EdgeUpdate> Updates { get; set; }

        public Edge()
        {
            Updates = new List<EdgeUpdate>();
        }

        public static Edge CreateReverseEdge(Edge edgeToReverse)
        {
            if (edgeToReverse == null || string.IsNullOrEmpty(edgeToReverse.Id)) { return null; }

            return new Edge
            {
                Id = GenerateId(edgeToReverse.Fid, EdgeDirections.Backwards),
                Fid = edgeToReverse.Fid,
                OsmId = edgeToReverse.OsmId,
                HighwayName = edgeToReverse.HighwayName,
                HighwayType = edgeToReverse.HighwayType,
                IsOneWay = edgeToReverse.IsOneWay,
                MaxSpeed = edgeToReverse.MaxSpeed,
                Length = edgeToReverse.Length,
                XFromPoint = edgeToReverse.XToPoint,
                YFromPoint = edgeToReverse.YToPoint,
                XToPoint = edgeToReverse.XFromPoint,
                YToPoint = edgeToReverse.YFromPoint,
                XMidPoint = edgeToReverse.XMidPoint,
                YMidPoint = edgeToReverse.YMidPoint
            };
        }

        public static string GenerateId(uint fId, EdgeDirections direction)
        {
            return (direction == EdgeDirections.Forwards) ? $"{ fId }a-->b" : $"{ fId }b-->a";
        }

        public bool Update(EdgeUpdate update)
        {
            if (!EdgeUpdate.IsSavableUpdate(update))
            {
                return false;
            }

            Updates.Add(update);

            return true;
        }

        public bool Requeue(int updateCount, EdgeUpdate update, List<UpdateTime> updateTimes, ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges)
        {
            if (EdgeUpdate.IsRequeuableUpdate(update))
            {
                edges.Enqueue(new Tuple<int, Edge, UpdateTime>(updateCount + 1, this, updateTimes[(updateCount + 1) % updateTimes.Count]));

                return true;
            }

            return false;
        }

        public bool UpdateAndRequeue(int updateCount, EdgeUpdate update, List<UpdateTime> updateTimes, ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges)
        {
            return Update(update) && Requeue(updateCount, update, updateTimes, edges);
        }

        public static HighwayTypes? GetHighwayType(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? null : (HighwayTypeNames.ContainsKey(name) ? (HighwayTypes?)HighwayTypeNames[name] : HighwayTypes.Unknown);
        }

        public static string GenerateCsvHeader(char separator)
        {
            var edge = new Edge();      // Only for reflection purposes.

            var headerRow = new List<string>
            {
                nameof(edge.Id),
                nameof(edge.Fid),
                nameof(edge.OsmId),
                nameof(edge.HighwayName),
                nameof(edge.HighwayType),
                nameof(edge.IsOneWay),
                nameof(edge.MaxSpeed),
                nameof(edge.Length),
                nameof(edge.XFromPoint),
                nameof(edge.YFromPoint),
                nameof(edge.XToPoint),
                nameof(edge.YToPoint),
                nameof(edge.XMidPoint),
                nameof(edge.YMidPoint)
            };

            return string.Join(separator.ToString(), headerRow);
        }
    }
}
