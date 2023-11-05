/*
    Copyright (C) 2023 Sehelitar

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

using LiteDB;
using System;

namespace Kick.Bot
{
    public class UserActivity : IDisposable
    {
        [BsonId(false)]
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
            dbCollection.EnsureIndex("ByUserId", x => x.UserId, true);
        }

        public static UserActivity ForUser(long userId)
        {
            var dbCollection = BotClient.Database.GetCollection<UserActivity>("users");
            var activityQuery = from activityObject in dbCollection.Query() where activityObject.UserId == userId select activityObject;
            return activityQuery.FirstOrDefault() ?? new UserActivity();
        }
    }
}
