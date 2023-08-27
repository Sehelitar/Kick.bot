using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kick.Models.Events;
using Kick;
using Kick.Models.API;

namespace Kick.Bot
{
    public sealed partial class BotEventListener
    {
        internal static IInlineInvokeProxy CPH { get { return BotClient.CPH; } }

        private readonly KickEventListener EventListener = null;
        public readonly Channel Channel;

        private readonly List<long> Followers = new List<long>();

        internal BotEventListener(KickEventListener listener, Channel channel) {
            EventListener = listener;
            Channel = channel;

            EventListener.OnViewerFollow += Kick_OnViewerFollow;
            EventListener.OnChatMessage += Kick_OnChatMessage;
            EventListener.OnChatMessageDeleted += Kick_OnChatMessageDeleted;
            EventListener.OnSubscription += Kick_OnSubscription;
            EventListener.OnSubGift += Kick_OnSubGift;
            EventListener.OnUserBanned += Kick_OnUserBanned;

            EventListener.JoinAsync(Channel).Wait();

            StreamerBotAppSettings.Load();
        }

        ~BotEventListener() {
            EventListener.LeaveAsync(Channel);
        }

        private void SendToQueue(BotEvent botEvent)
        {
            CPH.TriggerCodeEvent(botEvent.ActionId, botEvent.Arguments);
        }

        private void Kick_OnChatMessage(ChatMessageEvent message)
        {
            try
            {
                if (message.ChatroomId != Channel.Chatroom.Id)
                    return;

                CPH.LogVerbose($"[Kick] Chat | {message.Sender.Username} : {message.Content}");
                var isCommand = false;

                try
                {
                    // Si on trouve une commande qui correspond, on ne la traite pas comme un message
                    isCommand = BotChatCommander.Evaluate(message);
                }
                catch (Exception inEx)
                {
                    CPH.LogError($"[Kick] Une erreur s'est produite lors de la recherche de commande de chat : {inEx}");
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
                if (message.Sender.IsVIP)
                    role = 2;
                if (message.Sender.IsModerator)
                    role = 3;
                if (message.Sender.IsBroadcaster)
                    role = 4;

                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.Message,
                    Arguments = new Dictionary<string, object>() {
                        { "user", message.Sender.Username },
                        { "userName", message.Sender.Slug },
                        { "userId", message.Sender.Id },
                        { "userType", "kick" },
                        { "isSubscribed", message.Sender.IsSubscriber },
                        { "isModerator", message.Sender.IsModerator },
                        { "isVip", message.Sender.IsVIP },
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
                        { "isReply", message.IsReply },
                        { "firstMessage", false },

                        { "isCommand", isCommand },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] Une erreur s'est produite lors du déclenchement d'un évènement de réception d'un message dans le chat : {ex}");
            }
        }

        private void Kick_OnChatMessageDeleted(ChatMessageDeletedEvent message)
        {
            try
            {
                SendToQueue(new BotEvent()
                {
                    ActionId = BotEventType.MessageDeleted,
                    Arguments = new Dictionary<string, object>() {
                        { "message", message.Id },
                        { "eventSource", "kick" },
                        { "fromKick", true }
                    }
                });
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] Une erreur s'est produite lors du déclenchement d'un évènement de suppression de message : {ex}");
            }
        }

        private void Kick_OnViewerFollow(ChannelFollowEvent followEvent)
        {
            try
            {
                if (followEvent.Channel.Id != Channel.Id)
                    return;

                CPH.LogDebug($"[Kick] Nouveau follower : {followEvent.User.Username}");

                if (Followers.Contains(followEvent.User.Id))
                {
                    // On a déjà reçu un event de follow pour cet utilisateur !
                    return;
                }
                Followers.Add(followEvent.User.Id);

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
                CPH.LogError($"[Kick] Une erreur s'est produite lors du déclenchement d'un évènement de follow : {ex}");
            }
        }

        private void Kick_OnUserBanned(BannedUserEvent bannedUserEvent)
        {
            try
            {
                if (bannedUserEvent.Channel.Id != Channel.Id)
                    return;

                if (!bannedUserEvent.IsBanned)
                {
                    CPH.LogDebug($"[Kick] Unban de {bannedUserEvent.Banned.Username}");
                    return;
                }

                if (bannedUserEvent.Ban.BannedUntil != null && bannedUserEvent.Ban.BannedUntil.HasValue)
                {
                    // Timeout
                    var duration = bannedUserEvent.Ban.BannedUntil.Value.Subtract(bannedUserEvent.Ban.BannedSince).TotalSeconds;
                    CPH.LogDebug($"[Kick] Timeout de {bannedUserEvent.Banned.Username} pendans {duration} secondes.");
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
                    CPH.LogDebug($"[Kick] Ban de {bannedUserEvent.Banned.Username}");
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
                CPH.LogError($"[Kick] Une erreur s'est produite lors du déclenchement d'un évènement de ban : {ex}");
            }
        }

        private void Kick_OnSubGift(GiftedSubscriptionEvent giftEvent)
        {
            try
            {
                if (giftEvent.Channel.Id != Channel.Id)
                    return;

                // OnSubGift
                CPH.LogDebug($"[Kick] Nouveaux gifts de {giftEvent.User.Username} ({giftEvent.GiftedUsers.Length} subs offerts)");

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
                            { "rawInput", $"{giftEvent.User.Username} offre un abonnement à {giftEvent.GiftedUsers[0].Username}." },
                            { "rawInputEscaped", $"{giftEvent.User.Username} offre un abonnement à {giftEvent.GiftedUsers[0].Username}." },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] Une erreur s'est produite lors du déclenchement d'un évènement de sub gifts : {ex}");
            }
        }

        private void Kick_OnSubscription(SubscriptionEvent subEvent)
        {
            try
            {
                if (subEvent.Channel.Id != Channel.Id)
                    return;

                if (subEvent.IsNewSubscriber)
                {
                    CPH.LogDebug($"[Kick] Nouveau sub : {subEvent.User.Username}");

                    SendToQueue(new BotEvent()
                    {
                        ActionId = BotEventType.Subscription,
                        Arguments = new Dictionary<string, object>() {
                            { "user", subEvent.User.Username },
                            { "userName", subEvent.User.Slug },
                            { "userId", subEvent.User.Id },
                            { "userType", "kick" },
                            { "tier", "Tier1" },
                            { "rawInput", $"{subEvent.User.Username} vient de s'abonner, c'est son premier mois !" },
                            { "rawInputEscaped", $"{subEvent.User.Username} vient de s'abonner, c'est son premier mois !" },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
                else
                {
                    CPH.LogDebug($"[Kick] Nouveau resub : {subEvent.User.Username} ({subEvent.Subscription.Total} mois)");

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
                            { "rawInput", $"{subEvent.User.Username} se réabonne, c'est son {subEvent.Subscription.Total}ème mois." },
                            { "rawInputEscaped", $"{subEvent.User.Username} se réabonne, c'est son {subEvent.Subscription.Total}ème mois." },
                            { "eventSource", "kick" },
                            { "fromKick", true }
                        }
                    });
                }
                CPH.RunAction("KickBot", false);
            }
            catch (Exception ex)
            {
                CPH.LogError($"[Kick] Une erreur s'est produite lors du déclenchement d'un évènement d'abonnement : {ex.Message}");
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
        }

        internal class BotEvent
        {
            public string ActionId { get; set; }
            public Dictionary<string, object> Arguments { get; set; }
        }

    }
}
