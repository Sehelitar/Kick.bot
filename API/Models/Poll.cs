using Newtonsoft.Json;

namespace Kick.API.Models
{
    public class Poll
    {
        [JsonProperty("duration")]
        public int Duration { get; internal set; }
        [JsonProperty("remaining")]
        public int Remaining { get; internal set; }
        [JsonProperty("result_display_duration")]
        public int ResultDisplayDuration { get; internal set; }
        [JsonProperty("title")]
        public string Title {  get; internal set; }
        [JsonProperty("has_voted")]
        public bool HasVoted { get; internal set; }
        [JsonProperty("options")]
        public PollOption[] Options { get; internal set; }
    }

    public class PollOption
    {
        [JsonProperty("id")]
        public int Id { get; internal set; }
        [JsonProperty("label")]
        public string Label { get; internal set; }
        [JsonProperty("votes")]
        public int Votes { get; internal set; }
    }
}
