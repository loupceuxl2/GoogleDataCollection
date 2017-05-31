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

        [JsonProperty(PropertyName = "runTimeCompletedAt", Required = Required.Always)]
        public DateTime RunTimeCompletedAt { get; set; }

        [JsonProperty(PropertyName = "currentTimeBracketId", Required = Required.Always)]
        public Guid CurrentTimeBracketId { get; set; }

        [JsonProperty(PropertyName = "currentEdgeFid", Required = Required.Always)]
        public uint CurrentEdgeFid { get; set; }

        [JsonProperty(PropertyName = "currentDirection", Required = Required.Always)]
        public UpdateDirections CurrentDirection { get; set; }


        public static UpdateSession GetNextUpdateSession(uint totalFids, UpdateSession currentSession, List<TimeBracket> brackets)
        {
            // First time running (no previous sessions to continue from).
            if (currentSession == null)
            {
                return new UpdateSession
                {
                    CurrentEdgeFid = 0,
                    CurrentDirection = UpdateDirections.Forwards,
                    CurrentTimeBracketId = brackets.First().Id
                };
            }

            // If we haven't reached the end of the collection go to the next Fid.
            if (currentSession.CurrentEdgeFid != totalFids - 1)
            {
                return new UpdateSession
                {
                    CurrentEdgeFid = currentSession.CurrentEdgeFid + 1,
                    CurrentDirection = currentSession.CurrentDirection,
                    CurrentTimeBracketId = currentSession.CurrentTimeBracketId
                };
            }

            // End of collection, decide where to go next.
            return new UpdateSession
            {
                CurrentEdgeFid = 0,
                CurrentDirection = currentSession.CurrentDirection == UpdateDirections.Forwards ? UpdateDirections.Backwards : UpdateDirections.Forwards,
                CurrentTimeBracketId = (currentSession.CurrentDirection == UpdateDirections.Forwards) ? currentSession.CurrentTimeBracketId : TimeBracket.GetNextTimeBracket(brackets, currentSession.CurrentTimeBracketId).Id
            };
        }
    }
}
