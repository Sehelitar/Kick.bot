using Newtonsoft.Json;

namespace Kick.API.Models
{
    public class Reward
    {
        [JsonProperty("id" )]
        public string Id { get; set; }
        
        [JsonProperty("title" )]
        public string Title { get; set; }
        
        [JsonProperty("description" )]
        public string Description { get; set; }
        
        [JsonProperty("background_color" )]
        public string BackgroundColor { get; set; }
        
        [JsonProperty("cost")]
        public long Cost { get; set; }
        
        [JsonProperty("is_enabled" )]
        public bool IsEnabled { get; set; }
        
        [JsonProperty("is_paused" )]
        public bool IsPaused { get; set; }
        
        [JsonProperty("is_user_input_required" )]
        public bool IsUserInputRequired { get; set; }
        
        [JsonProperty("prompt" )]
        public string Prompt { get; set; }
        
        [JsonProperty("should_redemptions_skip_request_queue" )]
        public bool ShouldRedemptionsSkipRequestQueue { get; set; }
    }
}