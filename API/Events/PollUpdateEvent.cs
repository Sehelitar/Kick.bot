using Newtonsoft.Json;
using System;
using Kick.API.Models;

namespace Kick.API.Events
{
    public class PollUpdateEvent
    {
        [JsonIgnore]
        public Channel Channel { get; internal set; }

        [JsonIgnore]
        public DateTime Date { get; } = DateTime.Now;

        [JsonIgnore]
        public PollState State { get; internal set; } = PollState.InProgress;

        [JsonProperty("poll")]
        public Poll Poll { get; internal set; }
    }

    public enum PollState
    {
        InProgress,
        Completed,
        Cancelled
    }
}
