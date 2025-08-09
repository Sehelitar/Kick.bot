using System;
using Newtonsoft.Json;

namespace Kick.Bot
{
    public class UpdateMeta
    {
        [JsonProperty("lastUpdate")]
        public DateTime LastUpdate { get; set; }
        
        [JsonProperty("v")]
        public int Revision { get; set; }
        
        [JsonProperty("versions")]
        public UpdateRelease[] Releases { get; set; }
    }

    public class UpdateRelease
    {
        [JsonProperty("version")]
        public Version Version { get; set; }
        
        [JsonProperty("rev")]
        public Version Revision { get; set; }
        
        [JsonProperty("file")]
        public string File { get; set; }
    }
}