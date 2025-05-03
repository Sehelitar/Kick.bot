/*
    Copyright (C) 2023-2024 Sehelitar

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

using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kick.API;
using Kick.API.Events;
using Kick.API.Models;
using System.Linq;

namespace Kick.Bot
{
    public sealed class BotEventListener
    {
        internal static IInlineInvokeProxy CPH => BotClient.CPH;

        private readonly KickEventListener _eventListener = null;
        public Channel Channel;

        private readonly List<long> _followers = new List<long>();
        private readonly Queue<ChatMessageEvent> _messagesHistory = new Queue<ChatMessageEvent>(300);

        internal BotEventListener(KickEventListener listener, Channel channel)
        {
            _eventListener = listener;
            Channel = channel;

            _eventListener.OnViewerFollow += Kick_OnViewerFollow;
            _eventListener.OnChatMessage += Kick_OnChatMessage;
            _eventListener.OnChatMessageDeleted += Kick_OnChatMessageDeleted;
            _eventListener.OnSubscription += Kick_OnSubscription;
            _eventListener.OnSubGift += Kick_OnSubGift;
            _eventListener.OnUserBanned += Kick_OnUserBanned;
            _eventListener.OnChatUpdated += Kick_OnChatUpdated;
            _eventListener.OnPollCreated += Kick_OnPollCreated;
            _eventListener.OnPollUpdated += Kick_OnPollUpdated;
            _eventListener.OnPollCompleted += Kick_OnPollCompleted;
            _eventListener.OnPollCancelled += Kick_OnPollCancelled;
            _eventListener.OnStreamStarted += Kick_OnStreamStarted;
            _eventListener.OnStreamEnded += Kick_OnStreamEnded;
            _eventListener.OnStreamUpdated += Kick_OnStreamUpdated;
            _eventListener.OnRaid += Kick_OnRaid;
            _eventListener.OnMessagePinned += Kick_OnMessagePinned;
            _eventListener.OnMessageUnpinned += Kick_OnMessageUnpinned;
            _eventListener.OnRewardRedeemed += Kick_OnRewardRedeemed;

            _eventListener.JoinAsync(Channel).Wait();

            try
            {
                StreamerBotAppSettings.Load();
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred while loading the bot settings : {ex}");
            }
        }

        ~BotEventListener() {
            _ = _eventListener.LeaveAsync(Channel);
        }

        private static void SendToQueue(BotEvent botEvent)
        {
            CPH.TriggerCodeEvent(botEvent.ActionId, botEvent.Arguments);
        }

        private void Kick_OnChatMessage(ChatMessageEvent message)
        {
            try
            {
                if (message.ChatroomId != Channel.Chatroom.Id)
                    return;

                if (_messagesHistory.Count >= 300)
                    _messagesHistory.Dequeue();
                _messagesHistory.Enqueue(message);

                CPH.LogVerbose($"[Kick] Chat :: {message.Sender.Username} : {message.Content}");
                var isCommand = false;

                var firstMessage = true;
                using (var activity = UserActivity.ForUser(message.Sender.Id))
                {
                    firstMessage = activity.FirstMessage == null;

                    activity.UserId = message.Sender.Id;
                    activity.Username = message.Sender.Username;
                    activity.Slug = message.Sender.Slug;
                    activity.IsOG = message.Sender.IsOG;
                    activity.IsVip = message.Sender.IsVip;
                    activity.IsModerator = message.Sender.IsModerator;
                    activity.IsSubscriber = message.Sender.IsSubscriber;
                    if (activity.IsFollower)
                        activity.FollowerSince = activity.FollowerSince ?? DateTime.Now;
                    else
                        activity.FollowerSince = null;
                    if (activity.IsSubscriber)
                        activity.SubscriberSince = activity.SubscriberSince ?? DateTime.Now;
                    else
                        activity.SubscriberSince = null;
                    activity.FirstMessage = activity.FirstMessage ?? DateTime.Now;
                    activity.FirstActivity = activity.FirstActivity ?? DateTime.Now;
                    activity.LastMessage = DateTime.Now;
                    activity.LastActivity = DateTime.Now;
                }

                try
                {
                    // Si on trouve une commande qui correspond, on ne la traite pas comme un message
                    isCommand = BotChatCommander.Evaluate(message);
                }
                catch (Exception inEx)
                {
                    CPH.LogError($"[Kick] An error occurred while searching for chat commands : {inEx}");
                }

                var emoteRE = new Regex(@"\[emote:(?<emoteId>\d+):(?<emoteText>\w+)\]");
                var messageStripped = emoteRE.Replace(message.Content, "");
                var emotes = emoteRE.Matches(message.Content);
                List<string> emotesList = new List<string>();
                for (int i = 0; i < emotes.Count; ++i)
                {
                    emotesList.Add(emotes[i].Value);
                }

                int role = 1;
                if (message.Sender.IsVip)
                    role = 2;
                if (message.Sender.IsModerator)
                    role = 3;
                if (message.Sender.IsBroadcaster)
                    role = 4;

                var args = new Dictionary<string, object>() {
                    { "user", message.Sender.Username },
                    { "userName", message.Sender.Slug },
                    { "userId", message.Sender.Id },
                    { "userType", "kick" },
                    { "isSubscribed", message.Sender.IsSubscriber },
                    { "isModerator", message.Sender.IsModerator },
                    { "isVip", message.Sender.IsVip },
                    { "eventSource", "kick" },

                    { "msgId", message.Id },
                    { "chatroomId", message.ChatroomId },
                    { "role", role },
                    { "color", message.Sender.Identity.Color },
                    { "message", message.Content },
                    { "emoteCount", emotes.Count },
                    { "emotes", string.Join(",", emotesList) },
                    { "messageStripped", messageStripped },
                    { "messageCheermotesStripped", messageStripped },
                    { "isHighlight", false },
                    { "bits", 0 },
                    { "isAction", false },
                    { "firstMessage", firstMessage },

                    { "isReply", message.IsReply },

                    { "pinnableMessage", Newtonsoft.Json.JsonConvert.SerializeObject(message) },

                    { "isCommand", isCommand },
                    { "fromKick", true }
                };

                if (message.IsReply)
                {
                    var replyArgs = new Dictionary<string, object>() {
                        { "reply.msgId", message.Metadata?.OriginalMessage?.Id },
                        { "reply.message", message.Metadata?.OriginalMessage?.Content },
                        { "reply.userId", message.Metadata?.OriginalSender?.Id },
                        { "reply.userName", message.Metadata?.OriginalSender?.Username },
                    };
                    args = args.Concat(replyArgs).ToDictionary(k => k.Key, v => v.Value);
                }

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.Message,
                    Arguments = args
                });

                BotTimedActionManager.MessageReceived();
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred while handling an incoming chat message : {ex}");
            }
        }

        private void Kick_OnChatMessageDeleted(ChatMessageDeletedEvent message)
        {
            try
            {
                // Chat cleared
                if(message.Message == null)
                {
                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.MessageDeleted,
                        Arguments = new Dictionary<string, object>() {
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                    return;
                }

                Dictionary<string, object> arguments = null;
                var deletedMessage = (from msg in _messagesHistory where msg.Id == message.Message.Id select msg).FirstOrDefault();
                if(deletedMessage == null)
                {
                    arguments = new Dictionary<string, object>() {
                        { "msgId", message.Message.Id },
                        { "eventSource", "kick" },
                        { "fromKick", true }
                    };
                }
                else
                {
                    int role = 1;
                    if (deletedMessage.Sender.IsVip)
                        role = 2;
                    if (deletedMessage.Sender.IsModerator)
                        role = 3;
                    if (deletedMessage.Sender.IsBroadcaster)
                        role = 4;

                    arguments = new Dictionary<string, object>() {
                        { "eventSource", "kick" },
                        { "fromKick", true },

                        { "user", deletedMessage.Sender.Username },
                        { "userName", deletedMessage.Sender.Slug },
                        { "userId", deletedMessage.Sender.Id },
                        { "userType", "kick" },
                        { "isSubscribed", deletedMessage.Sender.IsSubscriber },
                        { "isModerator", deletedMessage.Sender.IsModerator },
                        { "isVip", deletedMessage.Sender.IsVip },

                        { "msgId", deletedMessage.Id },
                        { "chatroomId", deletedMessage.ChatroomId },
                        { "role", role },
                        { "color", deletedMessage.Sender.Identity.Color },
                        { "message", deletedMessage.Content }
                    };
                }

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.MessageDeleted,
                    Arguments = arguments
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred on message deletion : {ex}");
            }
        }

        private void Kick_OnViewerFollow(ChannelFollowEvent followEvent)
        {
            try
            {
                if (followEvent.Channel.Id != Channel.Id || !followEvent.IsFollowing)
                    return;

                CPH.LogDebug($"[Kick] Nouveau follower : {followEvent.User.Username}");
                UpdateActivityDB(followEvent.User);
                using (var activity = UserActivity.ForUser(followEvent.User.Id))
                {
                    activity.IsFollower = true;
                    activity.FollowerSince = activity.FollowerSince ?? DateTime.Now;
                }

                if (_followers.Contains(followEvent.User.Id))
                {
                    // On a déjà reçu un event de follow pour cet utilisateur !
                    return;
                }
                _followers.Add(followEvent.User.Id);

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.Follow,
                    Arguments = new Dictionary<string, object>() {
                        { "user", followEvent.User.Username },
                        { "userName", followEvent.User.Slug },
                        { "userId", followEvent.User.Id },
                        { "userType", "kick" },
                        { "isSubscribed", false },
                        { "isModerator", false },
                        { "isVip", false },
                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when a new viewer followed : {ex}");
            }
        }

        private void Kick_OnUserBanned(BannedUserEvent bannedUserEvent)
        {
            try
            {
                if (bannedUserEvent.Channel.Id != Channel.Id)
                    return;

                UpdateActivityDB(bannedUserEvent.User);
                UpdateActivityDB(bannedUserEvent.Banned);

                if (!bannedUserEvent.IsBanned)
                {
                    CPH.LogDebug($"[Kick] Unban :: {bannedUserEvent.Banned.Username}");

                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.UserUnbanned,
                        Arguments = new Dictionary<string, object>() {
                            { "user", bannedUserEvent.Banned.Username },
                            { "userName", bannedUserEvent.Banned.Slug },
                            { "userId", bannedUserEvent.Banned.Id },
                            { "userType", "kick" },
                            { "deletedById", bannedUserEvent.User.Id },
                            { "deletedByUsername", bannedUserEvent.User.Slug },
                            { "deletedByDisplayName", bannedUserEvent.User.Username },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });

                    return;
                }

                if (bannedUserEvent.Ban.BannedUntil != null && bannedUserEvent.Ban.BannedUntil.HasValue)
                {
                    // Timeout
                    var duration = bannedUserEvent.Ban.BannedUntil.Value.Subtract(bannedUserEvent.Ban.BannedSince).TotalSeconds;
                    CPH.LogDebug($"[Kick] Timeout :: {bannedUserEvent.Banned.Username} ({duration}s).");
                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.Timeout,
                        Arguments = new Dictionary<string, object>() {
                            { "user", bannedUserEvent.Banned.Username },
                            { "userName", bannedUserEvent.Banned.Slug },
                            { "userId", bannedUserEvent.Banned.Id },
                            { "userType", "kick" },
                            { "duration", duration },
                            { "createdAt", bannedUserEvent.Date },
                            { "createdById", bannedUserEvent.User.Id },
                            { "createdByUsername", bannedUserEvent.User.Slug },
                            { "createdByDisplayName", bannedUserEvent.User.Username },
                            { "reason", bannedUserEvent.Ban.Reason },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
                else
                {
                    // Ban
                    CPH.LogDebug($"[Kick] Ban :: {bannedUserEvent.Banned.Username}");
                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.UserBanned,
                        Arguments = new Dictionary<string, object>() {
                            { "user", bannedUserEvent.Banned.Username },
                            { "userName", bannedUserEvent.Banned.Slug },
                            { "userId", bannedUserEvent.Banned.Id },
                            { "userType", "kick" },
                            { "createdAt", bannedUserEvent.Date },
                            { "createdById", bannedUserEvent.User.Id },
                            { "createdByUsername", bannedUserEvent.User.Slug },
                            { "createdByDisplayName", bannedUserEvent.User.Username },
                            { "reason", bannedUserEvent.Ban.Reason },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when a viewer got banned : {ex}");
            }
        }

        private void Kick_OnSubGift(GiftedSubscriptionEvent giftEvent)
        {
            try
            {
                if (giftEvent.Channel.Id != Channel.Id)
                    return;

                // OnSubGift
                CPH.LogDebug($"[Kick] New gifts from {giftEvent.User.Username} ({giftEvent.GiftedUsers.Length} subs gifted)");

                UpdateActivityDB(giftEvent.User);
                foreach (var giftTarget in giftEvent.GiftedUsers)
                {
                    UpdateActivityDB(giftTarget);
                    using (var activity = UserActivity.ForUser(giftTarget.Id))
                    {
                        activity.IsSubscriber = true;
                        activity.SubscriberSince = activity.SubscriberSince ?? DateTime.Now;
                    }
                }

                if (giftEvent.GiftedUsers == null || giftEvent.GiftedUsers.Length == 0)
                {
                    // Aucune info sur le gift
                    return;
                }

                var isBomb = giftEvent.GiftedUsers.Length > 1;

                // S'agit-il d'un gift sub, ou d'une gift bomb ?
                if (isBomb)
                {
                    // Gift Bomb - Subs offerts à la communauté
                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.SubGifts,
                        Arguments = new Dictionary<string, object>() {
                            { "user", giftEvent.User.Username },
                            { "userName", giftEvent.User.Slug },
                            { "userId", giftEvent.User.Id },
                            { "userType", "kick" },
                            { "tier", "Tier1" },
                            { "gifts", giftEvent.GiftedUsers.Length },
                            { "totalGifts", giftEvent.GiftedUsers.Length },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
                else
                {
                    // Gift Sub - Sub offert à un viewer en particulier
                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.SubGift,
                        Arguments = new Dictionary<string, object>() {
                            { "user", giftEvent.User.Username },
                            { "userName", giftEvent.User.Slug },
                            { "userId", giftEvent.User.Id },
                            { "userType", "kick" },
                            { "recipientUser", giftEvent.GiftedUsers[0].Username },
                            { "recipientUserName", giftEvent.GiftedUsers[0].Slug },
                            { "recipientUserId", giftEvent.GiftedUsers[0].Id },
                            { "tier", "Tier1" },
                            { "totalSubsGifted", 1 },
                            { "monthsGifted", 1 },
                            { "rawInput", $"{giftEvent.User.Username} gifted a sub to {giftEvent.GiftedUsers[0].Username}." },
                            { "rawInputEscaped", $"{giftEvent.User.Username} gifted a sub to {giftEvent.GiftedUsers[0].Username}." },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when a viewer gifted subs : {ex}");
            }
        }

        private void Kick_OnSubscription(SubscriptionEvent subEvent)
        {
            try
            {
                if (subEvent.Channel.Id != Channel.Id)
                    return;

                UpdateActivityDB(subEvent.User);
                using (var activity = UserActivity.ForUser(subEvent.User.Id))
                {
                    activity.IsSubscriber = true;
                    activity.SubscriberSince = activity.SubscriberSince ?? DateTime.Now;
                }

                if (subEvent.IsNewSubscriber)
                {
                    CPH.LogDebug($"[Kick] New subscriber : {subEvent.User.Username}");

                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.Subscription,
                        Arguments = new Dictionary<string, object>() {
                            { "user", subEvent.User.Username },
                            { "userName", subEvent.User.Slug },
                            { "userId", subEvent.User.Id },
                            { "userType", "kick" },
                            { "tier", "Tier1" },
                            { "rawInput", $"{subEvent.User.Username} just subscribed for the first time!" },
                            { "rawInputEscaped", $"{subEvent.User.Username} just subscribed for the first time!" },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
                else
                {
                    CPH.LogDebug($"[Kick] Subscription renewed : {subEvent.User.Username} ({subEvent.Subscription.Total} mois)");

                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.Subscription,
                        Arguments = new Dictionary<string, object>() {
                            { "user", subEvent.User.Username },
                            { "userName", subEvent.User.Slug },
                            { "userId", subEvent.User.Id },
                            { "userType", "kick" },
                            { "tier", "Tier1" },
                            { "monthStreak", subEvent.Subscription.Total },
                            { "cumulative", subEvent.Subscription.Total },
                            { "rawInput", $"{subEvent.User.Username} subscribed ({subEvent.Subscription.Total} months)." },
                            { "rawInputEscaped", $"{subEvent.User.Username} subscribed ({subEvent.Subscription.Total} months)." },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when a viewer subscribed : {ex.Message}");
            }
        }

        private void Kick_OnChatUpdated(ChatUpdatedEvent chatUpdateEvent)
        {
            try
            {
                if (chatUpdateEvent.Id != Channel.Id)
                    return;

                CPH.LogDebug($"[Kick] Chat mode changed");

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.ChatUpdated,
                    Arguments = new Dictionary<string, object>() {
                        { "emotesOnly", chatUpdateEvent.EmotesMode.Enabled },
                        { "subsOnly", chatUpdateEvent.SubscribersMode.Enabled },
                        { "followersOnly", chatUpdateEvent.FollowersMode.Enabled },
                        { "followersOnlyMinDuration", chatUpdateEvent.FollowersMode.MinDuration },
                        { "slowMode", chatUpdateEvent.SlowMode.Enabled },
                        { "slowModeInterval", chatUpdateEvent.SlowMode.MessageInterval },
                        { "botProtection", chatUpdateEvent.AdvancedBotProtection.Enabled },
                        { "botProtectionRemaining", chatUpdateEvent.AdvancedBotProtection.RemainingTime },
                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred while handling chat mode change : {ex.Message}");
            }
        }

        private void Kick_OnPollCancelled(PollUpdateEvent pollUpdateEvent)
        {
            try
            {
                if (pollUpdateEvent.Channel.Id != Channel.Id)
                    return;

                CPH.LogDebug($"[Kick] Poll cancelled");

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.PollCancelled,
                    Arguments = new Dictionary<string, object>() {
                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when the current poll got cancelled : {ex.Message}");
            }
        }

        private void Kick_OnPollCreated(PollUpdateEvent pollUpdateEvent)
        {
            try
            {
                if (pollUpdateEvent.Channel.Id != Channel.Id)
                    return;

                CPH.LogDebug($"[Kick] Poll created");

                var args = new Dictionary<string, object>() {
                    { "poll.StartedAt", pollUpdateEvent.Date },
                    { "poll.Title", pollUpdateEvent.Poll.Title },
                    { "poll.Duration", pollUpdateEvent.Poll.Duration },
                    { "poll.DurationRemaining", pollUpdateEvent.Poll.Remaining },
                    { "poll.choices.count", pollUpdateEvent.Poll.Options.Length },

                    { "eventSource", "kick" },
                    { "fromKick", true }
                };

                var i = 0;
                var totalVotes = 0;
                foreach(var option in pollUpdateEvent.Poll.Options)
                {
                    args[$"poll.choice{i}.title"] = option.Label;
                    args[$"poll.choice{i}.votes"] = option.Votes;
                    args[$"poll.choice{i}.totalVotes"] = option.Votes;
                    totalVotes += option.Votes;
                    ++i;
                }

                args["poll.votes"] = totalVotes;
                args["poll.totalVotes"] = totalVotes;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.PollCreated,
                    Arguments = args
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when a new poll has been created : {ex.Message}");
            }
        }

        private void Kick_OnPollUpdated(PollUpdateEvent pollUpdateEvent)
        {
            try
            {
                if (pollUpdateEvent.Channel.Id != Channel.Id)
                    return;

                var args = new Dictionary<string, object>() {
                    { "poll.StartedAt", pollUpdateEvent.Date },
                    { "poll.Title", pollUpdateEvent.Poll.Title },
                    { "poll.Duration", pollUpdateEvent.Poll.Duration },
                    { "poll.DurationRemaining", pollUpdateEvent.Poll.Remaining },
                    { "poll.choices.count", pollUpdateEvent.Poll.Options.Length },

                    { "eventSource", "kick" },
                    { "fromKick", true }
                };

                var i = 0;
                var totalVotes = 0;
                foreach (var option in pollUpdateEvent.Poll.Options)
                {
                    args[$"poll.choice{i}.title"] = option.Label;
                    args[$"poll.choice{i}.votes"] = option.Votes;
                    args[$"poll.choice{i}.totalVotes"] = option.Votes;
                    totalVotes += option.Votes;
                    ++i;
                }

                args["poll.votes"] = totalVotes;
                args["poll.totalVotes"] = totalVotes;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.PollUpdated,
                    Arguments = args
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when the current poll got updated (new votes) : {ex.Message}");
            }
        }

        private void Kick_OnPollCompleted(PollUpdateEvent pollUpdateEvent)
        {
            try
            {
                if (pollUpdateEvent.Channel.Id != Channel.Id)
                    return;

                var args = new Dictionary<string, object>() {
                    { "poll.StartedAt", pollUpdateEvent.Date },
                    { "poll.Title", pollUpdateEvent.Poll.Title },
                    { "poll.Duration", pollUpdateEvent.Poll.Duration },
                    { "poll.DurationRemaining", pollUpdateEvent.Poll.Remaining },
                    { "poll.choices.count", pollUpdateEvent.Poll.Options.Length },

                    { "poll.EndedAt", DateTime.Now },

                    { "eventSource", "kick" },
                    { "fromKick", true }
                };

                var i = 0;
                var totalVotes = 0;
                PollOption bestOption = null;
                int bestIndex = 0;
                foreach (var option in pollUpdateEvent.Poll.Options)
                {
                    args[$"poll.choice{i}.title"] = option.Label;
                    args[$"poll.choice{i}.votes"] = option.Votes;
                    args[$"poll.choice{i}.totalVotes"] = option.Votes;

                    if (bestOption == null || bestOption.Votes < option.Votes)
                    {
                        bestOption = option;
                        bestIndex = i;
                    }

                    totalVotes += option.Votes;
                    ++i;
                }

                args["poll.votes"] = totalVotes;
                args["poll.totalVotes"] = totalVotes;

                args[$"poll.winningIndex"] = bestIndex;
                args[$"poll.winningChoice.id"] = bestIndex;
                args[$"poll.winningChoice.title"] = bestOption.Label;
                args[$"poll.winningChoice.votes"] = bestOption.Votes;
                args[$"poll.winningChoice.totalVotes"] = bestOption.Votes;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.PollCompleted,
                    Arguments = args
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when the current poll closed : {ex.Message}");
            }
        }

        private void Kick_OnStreamEnded(LivestreamStoppedEvent kickEvent)
        {
            try
            {
                if (kickEvent.Livestream.Channel.Id != Channel.Id)
                    return;

                Channel = _eventListener.Client.GetChannelInfos(Channel.Slug).Result;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.StreamEnded,
                    Arguments = new Dictionary<string, object>() {
                        { "endedAt", DateTime.Now },

                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when the stream stopped : {ex.Message}");
            }
        }

        private void Kick_OnStreamStarted(LivestreamStartedEvent livestreamEvent)
        {
            try
            {
                if (livestreamEvent.Livestream.ChannelId != Channel.Id)
                    return;

                Channel = _eventListener.Client.GetChannelInfos(Channel.Slug).Result;

                var args = new Dictionary<string, object>() {
                    { "startedAt", DateTime.Now },

                    { "game", Channel.LiveStream.Categories[0]?.Name },
                    { "gameId", Channel.LiveStream.Categories[0]?.Id },
                    { "tagCount", Channel.LiveStream.Tags.Count },
                    { "tags", Channel.LiveStream.Tags },
                    { "tagsDelimited", String.Join(",", Channel.LiveStream.Tags.ToArray()) },

                    { "eventSource", "kick" },
                    { "fromKick", true }
                };

                var i = 0;
                foreach(var tag in Channel.LiveStream.Tags)
                {
                    args[$"tag{i}"] = tag;
                    ++i;
                }

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.StreamStarted,
                    Arguments = args
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when the stream started : {ex.Message}");
            }
        }

        private void Kick_OnStreamUpdated(LivestreamUpdatedEvent livestreamEvent)
        {
            try
            {
                if (livestreamEvent.Channel.Id != Channel.Id)
                    return;

                var args = new Dictionary<string, object>() {
                    { "gameUpdate", livestreamEvent.Channel.LiveStream.Categories[0]?.Id != Channel.LiveStream.Categories[0]?.Id },
                    { "statusUpdate", livestreamEvent.SessionTitle != Channel.LiveStream?.SessionTitle },

                    { "status", livestreamEvent.SessionTitle },
                    { "oldStatus", Channel.LiveStream?.SessionTitle },

                    { "gameId", livestreamEvent.Channel.LiveStream.Categories[0]?.Id },
                    { "gameName", livestreamEvent.Channel.LiveStream.Categories[0]?.Name },
                    { "oldGameId", Channel.LiveStream.Categories[0]?.Id },
                    { "oldGameName", Channel.LiveStream.Categories[0]?.Name },

                    { "eventSource", "kick" },
                    { "fromKick", true }
                };

                Channel = _eventListener.Client.GetChannelInfos(Channel.Slug).Result;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.TitleChanged,
                    Arguments = args
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when the stream infos got updated : {ex.Message}");
            }
        }

        private void Kick_OnRaid(RaidEvent raidEvent)
        {
            try
            {
                if (raidEvent.Channel.Id != Channel.Id)
                    return;

                UpdateActivityDB(raidEvent.User);

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.Raid,
                    Arguments = new Dictionary<string, object>() {
                        { "user", raidEvent.Host.User.Username },
                        { "viewers", raidEvent.Host.ViewersCount },

                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when a raid appeared : {ex.Message}");
            }
        }

        private void Kick_OnMessagePinned(PinnedMessageEvent pinnedMessageEvent)
        {
            try
            {
                if (pinnedMessageEvent.Message.ChatroomId != Channel.Chatroom.Id)
                    return;

                var emoteRE = new Regex(@"\[emote:(?<emoteId>\d+):(?<emoteText>\w+)\]");
                var messageStripped = emoteRE.Replace(pinnedMessageEvent.Message.Content, "");
                var emotes = emoteRE.Matches(pinnedMessageEvent.Message.Content);
                List<string> emotesList = new List<string>();
                for (int i = 0; i < emotes.Count; ++i)
                {
                    emotesList.Add(emotes[i].Value);
                }

                int role = 1;
                if (pinnedMessageEvent.Message.Sender.IsVip)
                    role = 2;
                if (pinnedMessageEvent.Message.Sender.IsModerator)
                    role = 3;
                if (pinnedMessageEvent.Message.Sender.IsBroadcaster)
                    role = 4;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.MessagePinned,
                    Arguments = new Dictionary<string, object>() {
                        { "user", pinnedMessageEvent.Message.Sender.Username },
                        { "userName", pinnedMessageEvent.Message.Sender.Slug },
                        { "userId", pinnedMessageEvent.Message.Sender.Id },
                        { "userType", "kick" },
                        { "isSubscribed", pinnedMessageEvent.Message.Sender.IsSubscriber },
                        { "isModerator", pinnedMessageEvent.Message.Sender.IsModerator },
                        { "isVip", pinnedMessageEvent.Message.Sender.IsVip },
                        
                        { "msgId", pinnedMessageEvent.Message.Id },
                        { "chatroomId", pinnedMessageEvent.Message.ChatroomId },
                        { "role", role },
                        { "color", pinnedMessageEvent.Message.Sender.Identity.Color },
                        { "message", pinnedMessageEvent.Message.Content },
                        { "emoteCount", emotes.Count },
                        { "emotes", string.Join(",", emotesList) },
                        { "messageStripped", messageStripped },
                        { "messageCheermotesStripped", messageStripped },
                        { "isHighlight", false },
                        { "bits", 0 },
                        { "isReply", pinnedMessageEvent.Message.IsReply },

                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when reading pinned message data : {ex.Message}");
            }
        }

        private void Kick_OnMessageUnpinned()
        {
            try
            {
                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.MessageUnpinned,
                    Arguments = new Dictionary<string, object>() {
                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when reading unpinned message data : {ex.Message}");
            }
        }

        private void Kick_OnRewardRedeemed(RewardRedeemedEvent rewardRedeemedEvent)
        {
            try
            {
                var rewardData = new Dictionary<string, object>()
                {
                    { "user", rewardRedeemedEvent.User.Username },
                    { "userId", rewardRedeemedEvent.User.Id },

                    { "redeemId", rewardRedeemedEvent.Id },
                    { "rewardId", rewardRedeemedEvent.Reward.Id },
                    { "rewardTitle", rewardRedeemedEvent.Reward.Title },
                    { "rewardUserInput", rewardRedeemedEvent.Reward.UserInput },

                    { "eventSource", "kick" },
                    { "fromKick", true }
                };
                SendToQueue(new BotEvent
                {
                    ActionId = $"{BotEventType.RewardRedeemed}.{rewardRedeemedEvent.Reward.Id}",
                    Arguments = rewardData
                });
                SendToQueue(new BotEvent
                {
                    ActionId = BotEventType.RewardRedeemed,
                    Arguments = rewardData
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] An error occurred when reading redeemed reward data : {ex.Message}");
            }
        }

        private void UpdateActivityDB(EventUser user)
        {
            using (var activity = UserActivity.ForUser(user.Id))
            {
                activity.UserId = user.Id;
                activity.Username = user.Username;
                activity.Slug = user.Slug;
                activity.FirstActivity = activity.FirstActivity ?? DateTime.Now;
                activity.LastActivity = DateTime.Now;
            }
        }

        internal static class BotEventType
        {
            public const string Follow = "kickFollow";
            public const string Message = "kickChatMessage";
            public const string ChatCommand = "kickChatCommand";
            public const string ChatCommandCooldown = "kickChatCommandCooldown";
            public const string MessageDeleted = "kickChatMessageDeleted";
            public const string Subscription = "kickSub";
            public const string SubGift = "kickGift";
            public const string SubGifts = "kickGifts";
            public const string Timeout = "kickTO";
            public const string UserBanned = "kickBan";
            public const string UserUnbanned = "kickUnban";
            public const string PollCreated = "kickPollCreated";
            public const string PollUpdated = "kickPollUpdated";
            public const string PollCompleted = "kickPollCompleted";
            public const string PollCancelled = "kickPollCancelled";
            public const string ChatUpdated = "kickChatUpdated";
            public const string StreamStarted = "kickStreamStarted";
            public const string StreamEnded = "kickStreamEnded";
            public const string Raid = "kickIncomingRaid";
            public const string TitleChanged = "kickTitleChanged";
            public const string MessagePinned = "kickMessagePinned";
            public const string MessageUnpinned = "kickMessageUnpinned";
            public const string RewardRedeemed = "kickRewardRedeemed";
        }

        internal class BotEvent
        {
            public string ActionId { get; set; }
            public Dictionary<string, object> Arguments { get; set; }
        }
    }
}
