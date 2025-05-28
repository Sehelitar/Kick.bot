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
using Kick.API.Models;

namespace Kick.API.Events
{
    public class LivestreamStartedEvent
    {
        [JsonProperty("livestream")]
        public ActiveLivestream Livestream { get; internal set; }
    }

    public class LivestreamStoppedEvent
    {
        [JsonProperty("livestream")]
        public InactiveLivestream Livestream { get; internal set; }
    }

    public class ActiveLivestream
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("channel_id")]
        public long ChannelId { get; internal set; }
        [JsonProperty("session_title")]
        public string SessionTitle { get; internal set; } = string.Empty;
        [JsonProperty("source")]
        public object Source { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
    }

    public class InactiveLivestream
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("channel")]
        public LivestreamChannel Channel { get; internal set; }
    }

    public class LivestreamChannel
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_banned")]
        public bool IsBanned { get; internal set; }
    }

    public class TitleChangedEvent : KickBaseEvent
    {
        [JsonProperty("title")]
        public string Title { get; internal set; }
    }

    public class LivestreamUpdatedEvent
    {
        [JsonIgnore]
        public Channel Channel { get; internal set; }
        [JsonProperty("category")]
        public Category Category { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime StartDate { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_mature")]
        public bool IsMature { get; internal set; }
        [JsonProperty("language")]
        public string Language { get; internal set; }
        [JsonProperty("session_title")]
        public string SessionTitle { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("viewers")]
        public int Viewers { get; internal set; }
    }
}
