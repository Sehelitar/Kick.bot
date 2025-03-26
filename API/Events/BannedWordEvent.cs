using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class BannedWordEvent : KickBaseEvent
    {
        [JsonProperty("word")]
        public string Word { get; internal set; }

        public bool IsWordBanned { get; internal set; } = true;
    }
}
