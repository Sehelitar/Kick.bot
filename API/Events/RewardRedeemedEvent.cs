using Newtonsoft.Json;
using System;
using Kick.API.Models;

namespace Kick.API.Events
{
    public class RewardRedeemedEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("user")]
        public EventUser User { get; set; }
        
        [JsonProperty("reward")]
        public RedeemedReward Reward { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime Date { get; internal set; }
    }

    public class RedeemedReward
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("reward_title")]
        public string Title { get; set; }
        
        [JsonProperty("user_input")]
        public string UserInput { get; set; }
    }
}