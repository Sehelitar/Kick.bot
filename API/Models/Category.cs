using Newtonsoft.Json;
using System.Collections.Generic;

namespace Kick.API.Models
{
    public class Category
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("description")]
        public string Description { get; internal set; }
        [JsonProperty("banner")]
        public CategoryBanner Banner { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; internal set; }
        [JsonProperty("viewers")]
        public int Viewers { get; internal set; }
        [JsonProperty("category")]
        public ParentCategory ParentCategory { get; internal set; }
    }

    public class ParentCategory
    {
        [JsonProperty("id")]
        public long Id { get; internal set; }
        [JsonProperty("name")]
        public string Name { get; internal set; }
        [JsonProperty("slug")]
        public string Slug { get; internal set; }
        [JsonProperty("icon")]
        public string Icon { get; internal set; }
    }

    public class CategoryBanner
    {
        [JsonProperty("responsive")]
        public string Responsive { get; internal set; } = string.Empty;
        [JsonProperty("url")]
        public string Url { get; internal set; } = string.Empty;

    }
}
