/*
    Copyright (C) 2023-2025 Sehelitar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
