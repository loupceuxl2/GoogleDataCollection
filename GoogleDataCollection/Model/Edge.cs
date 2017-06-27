using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Edge
    {
        [JsonProperty(PropertyName = "fId", Required = Required.Always)]
        public uint Fid { get; set; }

        [JsonProperty(PropertyName = "osmId", Required = Required.Always)]
        public uint OsmId { get; set; }

        //52054
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

        public bool Validate(List<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates)
        {
            if (!IsOneWay)
            {
                if (directionUpdates.Count != 2
                    || (!directionUpdates[0].Item4.IsValid())
                    || (!directionUpdates[1].Item4.IsValid())
                    || (!UpdateInfo.IsSavable(directionUpdates[0].Item4))
                    || (!UpdateInfo.IsSavable(directionUpdates[1].Item4))
                    || (directionUpdates[0].Item5.HourRunTime != directionUpdates[1].Item5.HourRunTime))
                {
                    return false;
                }

                Updates.Add(new EdgeUpdate
                {
                    Forward = directionUpdates[0].Item4,
                    Backward = directionUpdates[1].Item4,
                    UpdateHour = directionUpdates[0].Item5.HourRunTime
                });

            }
            else
            {
                if (directionUpdates.Count != 1
                    || (!directionUpdates[0].Item4.IsValid())
                    || (!UpdateInfo.IsSavable(directionUpdates[0].Item4)))
                {
                    return false;
                }

                Updates.Add(new EdgeUpdate
                {
                    Forward = directionUpdates[0].Item4,
                    UpdateHour = directionUpdates[0].Item5.HourRunTime
                });
            }

            return true;
        }

        public void ValidateAndRequeue(List<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> directionUpdates, ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges, List<UpdateTime> updateTimes)
        {
            if (Validate(directionUpdates))
            {
                edges.Enqueue(new Tuple<int, Edge, UpdateTime>((int)directionUpdates[0].Item1 + 1, directionUpdates[0].Item2, updateTimes[((int)directionUpdates[0].Item1 + 1) % updateTimes.Count]));
            }
        }
    }
}
