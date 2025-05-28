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
using System.Collections.Generic;
// ReSharper disable UnusedMember.Local

namespace Kick.API.Models
{
    internal class CreateClipResponse {
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("url")]
        public string Url { get; internal set; }
    }

    public class Clip
    {
        [JsonProperty("category_id")]
        public string CategoryId { get; internal set; }
        [JsonProperty("channel_id")]
        public long ChannelId { get; internal set; }
        [JsonProperty("clip_url")]
        public string ClipUrl { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        [JsonProperty("duration")]
        public long Duration { get; internal set; }
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("likes")]
        public long Likes { get; internal set; }
        [JsonProperty("livestream_id")]
        public string LivestreamId { get; internal set; }
        [JsonProperty("privacy")]
        public string Privacy {  get; internal set; }
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; internal set; }
        [JsonProperty("title")]
        public string Title { get; internal set; }
        [JsonProperty("user_id")]
        public long UserId { get; internal set; }
        [JsonProperty("views")]
        public long Views { get; internal set; }
    }

    public class ClipsResponse
    {
        [JsonProperty("clips")]
        public List<Clip> Clips { get; internal set; }

        [JsonProperty("nextCursor")]
        public string NextCursor { get; internal set; }
    }

    public static class ClipOrder
    {
        const string ByDate = "date";
        const string ByLikes = "like";
        const string ByViews = "view";
    }

    public static class ClipTimeRange
    {
        const string All = "all";
        const string LastMonth = "month";
        const string LastWeek = "week";
        const string LastDay = "day";
    }
}
