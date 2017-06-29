using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Edge
    {
        [JsonProperty(PropertyName = "fId", Required = Required.Always)]
        public uint Fid { get; set; }

        [JsonProperty(PropertyName = "osmId", Required = Required.Always)]
        public uint OsmId { get; set; }

        [JsonProperty(PropertyName = "highwayName", Required = Required.AllowNull)]
        public string HighwayName { get; set; }

        // TO DO [OPTIONAL]: Change to an enum?
        [JsonProperty(PropertyName = "highwayType", Required = Required.Always)]
        public string HighwayType { get; set; }

        [JsonProperty(PropertyName = "isOneWay", Required = Required.Always)]
        public bool IsOneWay { get; set; }

        //[DefaultValue(0)]
        //[JsonProperty(PropertyName = "maxSpeed", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Populate)]
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

        // NOTE: An edge is considered to be valid if both its directions are savable if twoway, or if forward direction is savable if oneway (see UpdateInfo class).
        public bool UpdateEdge(List<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
        {
            var first = directionUpdates.FirstOrDefault()?.Item4;
            var second = directionUpdates.Skip(1).Take(1).FirstOrDefault()?.Item4;

            if ((!UpdateInfo.IsSavableUpdate(first)) || ((!IsOneWay) && (!UpdateInfo.IsSavableUpdate(second))))
            {
                return false;
            }

            Updates.Add(new EdgeUpdate
            {
                Forward = first,
                Backward = second ?? null,
                UpdateHour = directionUpdates[0].Item5.HourRunTime
            });

            return true;
        }

        // NOTE: An edge is considered non-requeuable if both directions are non-requeuable. Nulls are requeuable. See UpdateInfo class for specifics.
        public bool IsRequeuable(List<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
        {
            return (UpdateInfo.IsRequeuableUpdate(directionUpdates.FirstOrDefault()?.Item4)) && (UpdateInfo.IsRequeuableUpdate(directionUpdates.Skip(1).Take(1).FirstOrDefault()?.Item4));
        }
/*
        // NOTE: An edge is considered quittable (i.e., end data requests) if either direction is quittable. Nulls are not quittable. See UpdateInfo class for specifics.
        public bool IsQuittable(List<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
        {
            return (UpdateInfo.IsQuittableUpdate(directionUpdates.FirstOrDefault()?.Item4)) || (!UpdateInfo.IsQuittableUpdate(directionUpdates.Skip(1).Take(1).FirstOrDefault()?.Item4));
        }
*/
    }
}
