using Newtonsoft.Json;
using System;

namespace Kick.API.Events
{
    public class KickBaseEvent
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("channel")]
        public EventChannel Channel { get; internal set; }
        [JsonProperty("user")]
        public EventUser User { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime Date { get; internal set; }
    }
}
