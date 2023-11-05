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

using Kick.Models.Events;
using Kick.Models.API;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Linq;
using LiteDB;

namespace Kick.Bot
{
    public sealed class BotClient
    {
        public static IInlineInvokeProxy CPH;

        internal static LiteDatabase Database;

        internal KickClient Client { get; private set; }

        public User AuthenticatedUser { get; private set; }

        public BotClient() {
            CPH.RegisterCustomTrigger("[Kick] Chat Message", BotEventListener.BotEventType.Message, new string[] { "Kick", "Chat" });

            CPH.RegisterCustomTrigger("[Kick] Chat Command (Any)", BotEventListener.BotEventType.ChatCommand, new string[] { "Kick", "Commands" });
            CPH.RegisterCustomTrigger("[Kick] Chat Command Cooldown (Any)", BotEventListener.BotEventType.ChatCommand, new string[] { "Kick", "Commands" });

            CPH.RegisterCustomTrigger("[Kick] Follow", BotEventListener.BotEventType.Follow, new string[] { "Kick", "Channel" });

            CPH.RegisterCustomTrigger("[Kick] Subscription", BotEventListener.BotEventType.Subscription, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gift (x1)", BotEventListener.BotEventType.SubGift, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gifts (multiple)", BotEventListener.BotEventType.SubGifts, new string[] { "Kick", "Subscriptions" });

            CPH.RegisterCustomTrigger("[Kick] Chat Message Deleted", BotEventListener.BotEventType.MessageDeleted, new string[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] Timeout", BotEventListener.BotEventType.Timeout, new string[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] User Ban", BotEventListener.BotEventType.UserBanned, new string[] { "Kick", "Moderation" });

            CPH.RegisterCustomTrigger("[Kick] Poll Created", BotEventListener.BotEventType.PollCreated, new string[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Updated", BotEventListener.BotEventType.PollUpdated, new string[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Completed", BotEventListener.BotEventType.PollCompleted, new string[] { "Kick", "Polls" });
            CPH.RegisterCustomTrigger("[Kick] Poll Cancelled", BotEventListener.BotEventType.PollCancelled, new string[] { "Kick", "Polls" });

            CPH.RegisterCustomTrigger("[Kick] Stream Started", BotEventListener.BotEventType.StreamStarted, new string[] { "Kick", "Stream" });
            CPH.RegisterCustomTrigger("[Kick] Stream Ended", BotEventListener.BotEventType.StreamEnded, new string[] { "Kick", "Stream" });
            CPH.RegisterCustomTrigger("[Kick] Title/Category Changed", BotEventListener.BotEventType.TitleChanged, new string[] { "Kick", "Stream" });

            CPH.RegisterCustomTrigger("[Kick] Raid", BotEventListener.BotEventType.Raid, new string[] { "Kick" });

            var target = Path.GetTempPath() + "KickLogo.png";
            Properties.Resources.KickLogo.Save(target, ImageFormat.Png);

            CPH.LogDebug("[Kick] Extension loaded. Starting...");
            Client = new KickClient();
            Database = new LiteDatabase(@"data\kick-ext.db");
            CommandCounter.PruneVolatile();
        }

        ~BotClient()
        {
            CPH.LogDebug("[Kick] Extension is shuting down");
            Database.Checkpoint();
            Database = null;
        }

        public async Task Authenticate()
        {
            CPH.LogDebug("[Kick] Authentication...");
            AuthenticatedUser = await Client.Authenticate();
            CPH.LogDebug($"[Kick] Connected as {AuthenticatedUser.Username}");

            var target = Path.GetTempPath() + "KickLogo.png";
            new ToastContentBuilder()
                .AddText("Kick")
                .AddText($"Successfuly connected as {AuthenticatedUser.Username}")
                .AddAppLogoOverride(new Uri(target), ToastGenericAppLogoCrop.None)
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }

        public async Task<BotEventListener> StartListeningTo(string channelName)
        {
            if (AuthenticatedUser == null)
                throw new Exception("authentication required");

            CPH.LogDebug($"[Kick] Listening events for channel {channelName}");
            var channel = await Client.GetChannelInfos(channelName);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public async Task<BotEventListener> StartListeningToSelf()
        {
            if (AuthenticatedUser == null)
                throw new Exception("Authentication required");

            CPH.LogDebug($"[Kick] Listening events for channel {AuthenticatedUser.StreamerChannel.Slug}");
            var channel = await Client.GetChannelInfos(AuthenticatedUser.StreamerChannel.Slug);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public void SendMessage(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("missing argument, chatroomId required");

                Client.SendMessageToChatroom(chatroomId, Convert.ToString(args["message"])).Wait();
            }
            catch(Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while sending a message to chatroom : {ex}");
            }
        }

        public void SendReply(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else if (channel != null)
                    chatroomId = channel.Chatroom.Id;
                else
                    throw new Exception("missing argument, chatroomId required");
                string message = String.Empty;
                if (args.TryGetValue("rawInput", out var rawInput))
                    message = Convert.ToString(rawInput);
                else
                    message = args["message"];
                Client.SendReplyToChatroom(
                    chatroomId,
                    Convert.ToString(args["reply"]),
                    Convert.ToString(args["msgId"]),
                    Convert.ToString(message),
                    Convert.ToInt64(args["userId"]),
                    Convert.ToString(args["user"])
                ).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while sending a reply : {ex}");
            }
        }

        public void GetUserInfos(Dictionary<string, dynamic> args, Channel channel)
        {
            try
            {
                if (args.TryGetValue("targetUserName", out var userName) && userName != null && userName != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(userName), true);
                else if (args.TryGetValue("targetUser", out var user) && user != null && user != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(user));
                else if (args.TryGetValue("userName", out userName) && userName != null && userName != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(userName) , true);
                else if (args.TryGetValue("user", out user) && user != null && user != String.Empty)
                    GetKickChannelInfos(args, channel, Convert.ToString(user));
                else
                    GetKickChannelInfos(args, channel, channel.Slug, true);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while fetching user infos : {ex}");
            }
        }

        public void GetBroadcasterInfos(Dictionary<string, dynamic> args, Channel channel)
        {
            try
            {
                GetKickChannelInfos(args, channel, channel.Slug, true);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while fetching broadcaster infos : {ex}");
            }
        }

        private void GetKickChannelInfos(Dictionary<string, dynamic> args, Channel channel, string username, bool isSlug = false)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if(username.StartsWith("@"))
                    username = username.Substring(1);


                Channel channelInfos = null;
                ChannelUser userInfos = null;

                if(isSlug)
                {
                    channelInfos = Client.GetChannelInfos(username).Result;
                    userInfos = Client.GetChannelUserInfos(channel.Slug, channelInfos.User.Username).Result;
                }
                else
                {
                    userInfos = Client.GetChannelUserInfos(channel.Slug, username).Result;
                    channelInfos = Client.GetChannelInfos(userInfos.Slug).Result;
                }

                CPH.SetArgument("targetUser", channelInfos.User.Username);
                CPH.SetArgument("targetUserName", channelInfos.Slug);
                CPH.SetArgument("targetUserId", channelInfos.User.Id);
                CPH.SetArgument("targetDescription", channelInfos.User.Bio);
                CPH.SetArgument("targetDescriptionEscaped", WebUtility.UrlEncode(channelInfos.User.Bio));
                CPH.SetArgument("targetUserProfileImageUrl", channelInfos.User.ProfilePic);
                CPH.SetArgument("targetUserProfileImageUrlEscaped", WebUtility.UrlEncode(channelInfos.User.ProfilePic));
                CPH.SetArgument("targetUserProfileImageEscaped", WebUtility.UrlEncode(channelInfos.User.ProfilePic));
                CPH.SetArgument("targetUserType", channelInfos.IsVerified ? "partner" : (channelInfos.IsAffiliate ? "affiliate" : String.Empty));
                CPH.SetArgument("targetIsAffiliate", channelInfos.IsAffiliate);
                CPH.SetArgument("targetIsPartner", channelInfos.IsVerified);
                CPH.SetArgument("targetLastActive", DateTime.Now);
                CPH.SetArgument("targetPreviousActive", DateTime.Now);
                CPH.SetArgument("targetIsSubscribed", userInfos.IsSubscriber);
                CPH.SetArgument("targetSubscriptionTier", (userInfos.IsSubscriber && userInfos.SubscribedFor > 0 ? 1000 : 0));
                CPH.SetArgument("targetIsModerator", userInfos.IsModerator);
                CPH.SetArgument("targetIsVip", userInfos.IsVIP);
                CPH.SetArgument("targetIsFollowing", userInfos.IsFollowing);
                CPH.SetArgument("targetChannelTitle", channelInfos.LiveStream?.SessionTitle ?? String.Empty);
                CPH.SetArgument("game", channelInfos.LiveStream?.Categories.First()?.Name);
                CPH.SetArgument("gameId", channelInfos.LiveStream?.Categories.First()?.Id);
                CPH.SetArgument("createdAt", DateTime.Now);
                CPH.SetArgument("accountAge", Convert.ToInt64((DateTime.Now - channelInfos.Chatroom.CreatedAt).TotalSeconds));
                CPH.SetArgument("tagCount", 0);
                CPH.SetArgument("tags", new List<string>());
                CPH.SetArgument("tagsDelimited", String.Empty);

                if (channelInfos.LiveStream != null)
                {
                    CPH.SetArgument("tagCount", channelInfos.LiveStream.Tags.Count);
                    CPH.SetArgument("tags", channelInfos.LiveStream.Tags);
                    int tagId = 0;
                    channelInfos.LiveStream.Tags.ForEach((tag) => {
                        CPH.SetArgument("tag" + (tagId++), tag);
                    });
                    CPH.SetArgument("tagsDelimited", String.Join(", ", channelInfos.LiveStream.Tags.ToArray()));
                    ;
                }
                else if (channelInfos.PreviousLiveStreams.Count > 0)
                {
                    CPH.SetArgument("targetChannelTitle", channelInfos.PreviousLiveStreams[0].SessionTitle);
                    var categories = channelInfos.PreviousLiveStreams[0].Categories;

                    if (categories != null && categories.Count > 0)
                    {
                        var latestCategory = categories.First();
                        CPH.SetArgument("game", latestCategory.Name);
                        CPH.SetArgument("gameId", latestCategory.Id);
                    }

                    CPH.SetArgument("tagCount", channelInfos.PreviousLiveStreams[0].Tags.Count);
                    CPH.SetArgument("tags", channelInfos.PreviousLiveStreams[0].Tags);
                    int tagId = 0;
                    channelInfos.PreviousLiveStreams[0].Tags.ForEach((tag) => {
                        CPH.SetArgument("tag" + (tagId++), tag);
                    });
                    CPH.SetArgument("tagsDelimited", String.Join(", ", channelInfos.PreviousLiveStreams[0].Tags.ToArray()));
                }
                else if (channelInfos.RecentCategories != null && channelInfos.RecentCategories.Count > 0)
                {
                    var latestCategory = channelInfos.RecentCategories.First();
                    CPH.SetArgument("game", latestCategory.Name);
                    CPH.SetArgument("gameId", latestCategory.Id);
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while fetching channel infos : {ex}");
            }
        }

        public void AddChannelVip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.AddChannelVip(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while adding a new VIP : {ex}");
            }
        }

        public void RemoveChannelVip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.RemoveChannelVip(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while removing a VIP : {ex}");
            }
        }

        public void AddChannelOG(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.AddChannelOG(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while adding a new OG : {ex}");
            }
        }

        public void RemoveChannelOG(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.RemoveChannelOG(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while removing an OG : {ex}");
            }
        }

        public void AddChannelModerator(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.AddChannelModerator(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while adding a moderator : {ex}");
            }
        }

        public void RemoveChannelModerator(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.RemoveChannelModerator(channel, username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while removing a moderator : {ex}");
            }
        }

        public void BanUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                if (!args.TryGetValue("banReason", out var banReason))
                    banReason = "";

                Client.BanUser(channel, (string)username, (string)banReason).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while banning a user : {ex}");
            }
        }

        public void TimeoutUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                if (!args.TryGetValue("banDuration", out var banDuration))
                    throw new Exception("missing argument, banDuration required");

                if (!args.TryGetValue("banReason", out var banReason))
                    banReason = "";

                Client.TimeoutUser(channel, (string)username, Convert.ToInt64(banDuration), (string)banReason).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while temporarily banning a user : {ex}");
            }
        }

        public void UnbanUser(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("user", out var username))
                    throw new Exception("missing argument, user required");

                Client.UnbanUser(channel, (string)username).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while unbanning a user : {ex}");
            }
        }

        public void StartPoll(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("pollTitle", out var title))
                    throw new Exception("missing argument, pollTitle required");

                if (!args.TryGetValue("pollDuration", out var duration))
                    throw new Exception("missing argument, pollDuration required");
                 
                if (!args.TryGetValue("pollDisplayTime", out var displayTime))
                    displayTime = 30;

                if (!args.TryGetValue("pollChoices", out var choices))
                    throw new Exception("missing argument, pollChoices required");

                Client.StartPoll(channel, (string)title, ((string)choices).Split('|'), Convert.ToInt32(duration), Convert.ToInt32(displayTime)).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while starting a new poll : {ex}");
            }
        }

        public void ChangeTitle(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("title", out var title))
                    throw new Exception("missing argument, title required");

                Client.SetChannelTitle(channel, (string)title).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while changing stream title : {ex}");
            }
        }

        public void ChangeCategory(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("category", out var category))
                    throw new Exception("missing argument, category required");

                Client.SetChannelCategory(channel, (string)category).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while changing stream category : {ex}");
            }
        }

        public void ClearChat(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                Client.ClearChat(channel).Wait();
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while clearing chat : {ex}");
            }
        }

        public void MakeClip(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                args.TryGetValue("title", out var title);

                if (args.TryGetValue("duration", out var duration))
                {
                    duration = Convert.ToInt32(duration);
                } else
                {
                    duration = 30;
                }
                    

                if (args.TryGetValue("skip", out var skip))
                {
                    skip = Convert.ToInt32(skip);
                } else
                {
                    skip = 0;
                }
                
                var maxDuration = 90 - Convert.ToInt32(skip);
                duration = Math.Min(duration, maxDuration);
                var startTime = maxDuration - duration;

                // Update channel infos to have fresh livestream data
                channel = Client.GetChannelInfos(channel.Slug).Result;
                try
                {
                    var clip = Client.MakeClip(channel, Convert.ToInt32(duration), (string)title, startTime).Result;
                    CPH.SetArgument("createClipSuccess", true);
                    CPH.SetArgument("createClipId", clip.Id);
                    CPH.SetArgument("createClipCreatedAt", DateTime.Now);
                    CPH.SetArgument("createClipUrl", $"https://kick.com/{channel.Slug}?clip={clip.Id}");
                }
                catch
                {
                    CPH.SetArgument("createClipSuccess", false);
                }
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to create to clip : {ex}");
            }
        }

        public void ChatEmotesOnly(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("enable", out var enable))
                {
                    channel = Client.GetChannelInfos(channel.Slug).Result;
                    enable = !channel.Chatroom.IsEmotesOnly;
                }

                ChatUpdatedEvent updated = Client.SetChannelChatroomEmotesOnly(channel, enable).Result;

                CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
        }

        public void ChatSubsOnly(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("enable", out var enable))
                {
                    channel = Client.GetChannelInfos(channel.Slug).Result;
                    enable = !channel.Chatroom.IsSubOnly;
                }

                ChatUpdatedEvent updated = Client.SetChannelChatroomSubscribersOnly(channel, enable).Result;

                CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
        }

        public void ChatBotProtection(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                if (!args.TryGetValue("enable", out var enable))
                {
                    enable = true;
                }

                ChatUpdatedEvent updated = Client.SetChannelChatroomBotProtection(channel, enable).Result;

                CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat protection : {ex}");
            }
        }

        public void ChatFollowersOnly(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                int? duration = null;
                if (args.TryGetValue("duration", out var rawDuration))
                {
                    duration = Math.Min(Convert.ToInt32(rawDuration), 600); // Kick limite à 10h (600 minutes) max
                }

                ChatUpdatedEvent updated = Client.SetChannelChatroomFollowersMode(channel, duration).Result;

                CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
        }

        public void ChatSlowMode(Dictionary<string, dynamic> args, Channel channel = null)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentication required");

                int? interval = null;
                if (args.TryGetValue("interval", out var rawInterval))
                {
                    interval = Math.Min(Convert.ToInt32(rawInterval), 300); // Kick limite l'intervale à 5 minutes (300 secondes)
                }

                ChatUpdatedEvent updated = Client.SetChannelChatroomSlowMode(channel, interval).Result;

                CPH.SetArgument("chatEmotesOnly", updated.EmotesMode.Enabled);
                CPH.SetArgument("chatFollowersOnly", updated.FollowersMode.Enabled);
                CPH.SetArgument("chatFollowersSince", updated.FollowersMode.MinDuration);
                CPH.SetArgument("chatSlowMode", updated.SlowMode.Enabled);
                CPH.SetArgument("chatSlowModeInterval", updated.SlowMode.MessageInterval);
                CPH.SetArgument("chatSubsOnly", updated.SubscribersMode.Enabled);
                CPH.SetArgument("chatBotProtection", updated.AdvancedBotProtection.Enabled);
                CPH.SetArgument("chatBotProtectionRemaining", updated.AdvancedBotProtection.RemainingTime);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] An error occurred while trying to change chat mode : {ex}");
            }
        }
    }
}
