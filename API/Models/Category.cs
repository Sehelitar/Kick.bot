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
