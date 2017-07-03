using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Edge
    {
        public enum EdgeDirections : byte { Forwards, Backwards }

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

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

        /*
                // NOTE: An edge is considered to be valid if both its directions are savable if twoway, or if forward direction is savable if oneway (see UpdateInfo class).
                public bool UpdateEdge(List<Tuple<int, uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
                {
                    var first = directionUpdates.FirstOrDefault()?.Item5;
                    var second = directionUpdates.Skip(1).Take(1).FirstOrDefault()?.Item5;

                    if ((!UpdateInfo.IsSavableUpdate(first)) || ((!IsOneWay) && (!UpdateInfo.IsSavableUpdate(second))))
                    {
                        return false;
                    }

                    Updates.Add(new EdgeUpdate
                    {
                        Forward = first,
                        Backward = second ?? null,
                        UpdateHour = directionUpdates[0].Item6.HourRunTime
                    });

                    return true;
                }

                // NOTE: An edge is considered non-requeuable if both directions are non-requeuable. Nulls are requeuable. See UpdateInfo class for specifics.
                public bool IsRequeuable(List<Tuple<int, uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
                {
                    return (UpdateInfo.IsRequeuableUpdate(directionUpdates.FirstOrDefault()?.Item5)) && (UpdateInfo.IsRequeuableUpdate(directionUpdates.Skip(1).Take(1).FirstOrDefault()?.Item5));
                }

                // NOTE: An edge is considered quittable (i.e., end data requests) if either direction is quittable. Nulls are not quittable. See UpdateInfo class for specifics.
                public bool IsQuittable(List<Tuple<int, uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
                {
                    return (UpdateInfo.IsQuittableUpdate(directionUpdates.FirstOrDefault()?.Item5)) || (!UpdateInfo.IsQuittableUpdate(directionUpdates.Skip(1).Take(1).FirstOrDefault()?.Item5);
                }
        */
    }
}
