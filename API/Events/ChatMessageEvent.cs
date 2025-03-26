using Newtonsoft.Json;
using System;

namespace Kick.API.Events
{
    public class ChatMessageEvent
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("chatroom_id")]
        public long ChatroomId { get; internal set; }
        [JsonProperty("content")]
        public string Content { get; internal set; }
        [JsonProperty("type")]
        public string Type { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        [JsonProperty("sender")]
        public ChatUser Sender { get; internal set; }

        [JsonProperty("metadata")]
        public MessageMetadata Metadata { get; internal set; }

        [JsonIgnore]
        public bool IsReply
        {
            get
            {
                return Type == "reply";
            }
        }

        public class MessageMetadata
        {
            [JsonProperty("original_message")]
            public OriginalMessage OriginalMessage;
            [JsonProperty("original_sender")]
            public OriginalSender OriginalSender;
        }

        public class OriginalMessage
        {
            [JsonProperty("id")]
            public string Id { get; internal set; }
            [JsonProperty("content")]
            public string Content { get; internal set; }
        }

        public class OriginalSender
        {
            [JsonProperty("id")]
            public string Id { get; internal set; }
            [JsonProperty("username")]
            public string Username { get; internal set; }
        }
    }
}
