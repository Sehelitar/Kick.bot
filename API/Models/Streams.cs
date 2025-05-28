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

namespace Kick.API.Models
{
    public class LiveStream
    {
        /*[JsonProperty("category")]
        public Category Category { get; internal set; }*/
        [JsonProperty("categories")]
        public List<Category> Categories { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime StartDate { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_mature")]
        public bool IsMature { get; internal set; }
        [JsonProperty("duration")]
        public long Duration { get; internal set; }
        [JsonProperty("language")]
        public string Language { get; internal set; }
        [JsonProperty("session_title")]
        public string SessionTitle { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; internal set; }
        [JsonProperty("thumbnail")]
        public StreamPreview Thumbnail { get; internal set; }
        [JsonProperty("viewer_count")]
        public int ViewerCount { get; internal set; }
        [JsonProperty("viewers")]
        public int Viewers { get; internal set; }
    }

    public class PreviousStream
    {
        [JsonProperty("categories")]
        public List<Category> Categories { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime StartDate { get; internal set; }
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("is_mature")]
        public bool IsMature { get; internal set; }
        [JsonProperty("duration")]
        public long Duration { get; internal set; }
        [JsonProperty("language")]
        public string Language { get; internal set; }
        [JsonProperty("session_title")]
        public string SessionTitle { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; internal set; }
        [JsonProperty("thumbnail")]
        public StreamThumbnail Thumbnail { get; internal set; }
        [JsonProperty("video")]
        public StreamVideo Video { get; internal set; }
        [JsonProperty("viewer_count")]
        public int ViewerCount { get; internal set; }
        [JsonProperty("views")]
        public int Views { get; internal set; }
    }

    public class StreamPreview
    {
        [JsonProperty("responsive")]
        public string Responsive { get; internal set; } = string.Empty;
        [JsonProperty("url")]
        public string Url { get; internal set; } = string.Empty;
    }

    public class StreamThumbnail
    {
        [JsonProperty("src")]
        public string Src { get; internal set; }
        [JsonProperty("srcset")]
        public string SrcSet { get; internal set; }
    }

    public class StreamVideo
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("live_stream_id")]
        public long LiveStreamId { get; internal set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; internal set; }
        [JsonProperty("uuid")]
        public string Uuid { get; internal set; }
    }

    public class StreamInfo
    {
        [JsonProperty("category")]
        public StreamCategory Category { get; internal set; }
        [JsonProperty("is_mature")]
        public bool IsMature { get; internal set; }
        [JsonProperty("is_live")]
        public bool? IsLive { get; internal set; }
        [JsonProperty("language")]
        public string Language { get; internal set; }
        [JsonProperty("start_time")]
        public DateTime? StartTime { get; internal set; }
        [JsonProperty("stream_title")]
        public string StreamTitle { get; internal set; }
        [JsonProperty("viewer_count")]
        public int? ViewerCount { get; internal set; }
    }

    public class StreamCategory
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("thumbnail")]
        public StreamCategoryThumbnail Thumbnail { get; internal set; }
        [JsonProperty("tags")]
        public string[] Tags { get; internal set; }
    }

    public class StreamCategoryThumbnail
    {
        [JsonProperty("src")]
        public string Src { get; internal set; }
        [JsonProperty("srcset")]
        public string SrcSet { get; internal set; }
    }
}
