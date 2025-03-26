using Newtonsoft.Json;

namespace Kick.API.Events
{
    public class ChatMessageDeletedEvent
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("message")]
        public ChatMessageDeletedInfos Message { get; internal set; }
    }

    public class ChatMessageDeletedInfos
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
    }
}
