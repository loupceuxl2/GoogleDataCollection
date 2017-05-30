using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateSession
    {
        public enum UpdateDirections : byte { Forwards, Backwards }

        [JsonProperty(PropertyName = "lastRunTime", Required = Required.Always)]
        public DateTime RunTime { get; set; }

        [JsonProperty(PropertyName = "lastTimeBracketId", Required = Required.Always)]
        public Guid TimeBracketId { get; set; }

        [JsonProperty(PropertyName = "lastEdgeFid", Required = Required.Always)]
        public uint EdgeFid { get; set; }

        [JsonProperty(PropertyName = "lastDirection", Required = Required.Always)]
        public UpdateDirections Direction { get; set; }


        public static UpdateSession GetNextUpdateSession(uint totalFids, UpdateSession currentSession, List<TimeBracket> brackets)
        {
            // First time running (no previous sessions to continue from).
            if (currentSession == null)
            {
                return new UpdateSession
                {
                    EdgeFid = 0,
                    Direction = UpdateDirections.Forwards,
                    TimeBracketId = brackets.First().Id
                };
            }

            // If we haven't reached the end of the collection go to the next Fid.
            if (currentSession.EdgeFid != totalFids - 1)
            {
                return new UpdateSession
                {
                    EdgeFid = currentSession.EdgeFid + 1,
                    Direction = currentSession.Direction,
                    TimeBracketId = currentSession.TimeBracketId
                };
            }

            // End of collection, decide where to go next.
            return new UpdateSession
            {
                EdgeFid = 0,
                Direction = currentSession.Direction == UpdateDirections.Forwards ? UpdateDirections.Backwards : UpdateDirections.Forwards,
                TimeBracketId = (currentSession.Direction == UpdateDirections.Forwards) ? currentSession.TimeBracketId : TimeBracket.GetNextTimeBracket(brackets, currentSession.TimeBracketId).Id
            };
        }
    }
}
