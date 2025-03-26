using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class EventChannel
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
    }
}
