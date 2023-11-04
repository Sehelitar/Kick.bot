using Kick.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kick.Bot
{
    public class UserActivity : IDisposable
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Slug { get; set; }
        public bool IsModerator { get; set; } = false;
        public bool IsFollower { get; set; } = false;
        public DateTime? FollowerSince { get; set; } = null;
        public bool IsSubscriber { get; set; }
        public DateTime? SubscriberSince { get; set; } = null;
        public bool IsVip { get; set; } = false;
        public bool IsOG { get; set; } = false;
        public DateTime? FirstMessage { get; set; } = null;
        public DateTime? LastMessage { get; set; } = null;
        public DateTime? FirstActivity { get; set; } = null;
        public DateTime? LastActivity { get; set; } = null;

        public void Dispose()
        {
            var dbCollection = BotClient.Database.GetCollection<UserActivity>("users");
            dbCollection.Upsert(this);
            dbCollection.EnsureIndex(x => x.UserId, true);
        }

        public static UserActivity ForUser(long userId)
        {
            var dbCollection = BotClient.Database.GetCollection<UserActivity>("users");
            var activityQuery = from activityObject in dbCollection.Query() where activityObject.UserId == userId select activityObject;
            return activityQuery.FirstOrDefault();
        }
    }
}
