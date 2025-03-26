using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Models.API
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
        public int Duration { get; internal set; }
        [JsonProperty("id")]
        public string Id { get; internal set; }
        [JsonProperty("likes")]
        public int Likes { get; internal set; }
        [JsonProperty("livestream_id")]
        public string LivestreamId { get; internal set; }
        [JsonProperty("privacy")]
        public string Privacy {  get; internal set; }
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; internal set; }
        [JsonProperty("title")]
        public string Title { get; internal set; }
        [JsonProperty("user_id")]
        public int UserId { get; internal set; }
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
