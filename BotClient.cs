using Kick;
using Kick.Models.Events;
using Kick.Models.API;
using Newtonsoft.Json;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.CodeDom;
using static Kick.Bot.BotClient;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Reflection;

namespace Kick.Bot
{
    public sealed class BotClient
    {
        public static IInlineInvokeProxy CPH;
        
        public KickClient Client { get; private set; }

        public User AuthenticatedUser { get; private set; }

        public BotClient() {
            CPH.RegisterCustomTrigger("[Kick] Chat Message", BotEventListener.BotEventType.Message, new string[] { "Kick", "Chat" });
            CPH.RegisterCustomTrigger("[Kick] Chat Message Deleted", BotEventListener.BotEventType.MessageDeleted, new string[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] Chat Command (Any)", BotEventListener.BotEventType.ChatCommand, new string[] { "Kick", "Commands" });
            CPH.RegisterCustomTrigger("[Kick] Chat Command Cooldown (Any)", BotEventListener.BotEventType.ChatCommand, new string[] { "Kick", "Commands" });
            CPH.RegisterCustomTrigger("[Kick] Follow", BotEventListener.BotEventType.Follow, new string[] { "Kick", "Channel" });
            CPH.RegisterCustomTrigger("[Kick] Subscription", BotEventListener.BotEventType.Subscription, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gift (x1)", BotEventListener.BotEventType.SubGift, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Sub Gifts (multiple)", BotEventListener.BotEventType.SubGifts, new string[] { "Kick", "Subscriptions" });
            CPH.RegisterCustomTrigger("[Kick] Timeout", BotEventListener.BotEventType.Timeout, new string[] { "Kick", "Moderation" });
            CPH.RegisterCustomTrigger("[Kick] User Ban", BotEventListener.BotEventType.UserBanned, new string[] { "Kick", "Moderation" });

            CPH.LogDebug("[Kick] Démarrage du bot Kick");
            Client = new KickClient();
        }

        ~BotClient()
        {
            CPH.LogDebug("[Kick] Arrêt du bot Kick");
        }

        public async Task Authenticate()
        {
            CPH.LogDebug("[Kick] Authentification...");
            AuthenticatedUser = await Client.Authenticate();
            CPH.LogDebug($"[Kick] Connecté en tant que {AuthenticatedUser.UserName}");

            var target = Assembly.GetExecutingAssembly().Location;
            target = target.Substring(0, target.LastIndexOf('\\')+1) + "kick.png";

            new ToastContentBuilder()
                .AddText("Kick")
                .AddText($"Successfuly connected as {AuthenticatedUser.UserName}")
                .AddAppLogoOverride(new Uri(target), ToastGenericAppLogoCrop.None)
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }

        public async Task<BotEventListener> StartListeningTo(string channelName)
        {
            if (AuthenticatedUser == null)
                throw new Exception("Authentification requise");

            CPH.LogDebug($"[Kick] Mise en place de l'écoute des évènements pour la chaine de {channelName}");
            var channel = await Client.GetChannelInfos(channelName);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public async Task<BotEventListener> StartListeningToSelf()
        {
            if (AuthenticatedUser == null)
                throw new Exception("Authentification requise");

            CPH.LogDebug($"[Kick] Mise en place de l'écoute des évènements pour la chaine de {AuthenticatedUser.StreamerChannel.Slug}");
            var channel = await Client.GetChannelInfos(AuthenticatedUser.StreamerChannel.Slug);
            return new BotEventListener(Client.GetEventListener(), channel);
        }

        public void SendMessage(Dictionary<string, dynamic> args)
        {
            try {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (args.TryGetValue("chatroomId", out var chatroomId))
                {
                    chatroomId = Convert.ToInt64(chatroomId);
                    Client.SendMessageToChatroom(chatroomId, Convert.ToString(args["message"])).Wait();
                } else
                {
                    throw new Exception("argument chatroomId manquant.");
                }
            }
            catch(Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi du message : {ex}");
            }
        }

        public void SendReply(Dictionary<string, dynamic> args)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                if (args.TryGetValue("chatroomId", out var chatroomId))
                    chatroomId = Convert.ToInt64(chatroomId);
                else
                    throw new Exception("argument chatroomId manquant.");
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
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        public void GetUserInfos(Dictionary<string, dynamic> args, Channel channel)
        {
            try
            {
                if(!args.TryGetValue("userName", out var target))
                    target = channel.Slug;

                GetKickChannelInfos(args, channel, Convert.ToString(target));
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        public void GetBroadcasterInfos(Dictionary<string, dynamic> args, Channel channel)
        {
            try
            {
                GetKickChannelInfos(args, channel, channel.Slug);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }

        protected void GetKickChannelInfos(Dictionary<string, dynamic> args, Channel channel, string username)
        {
            try
            {
                if (AuthenticatedUser == null)
                    throw new Exception("authentification requise");

                var channelInfos = Client.GetChannelInfos(username).Result;
                var userInfos = Client.GetChannelUserInfos(channel.Slug, username).Result;

                CPH.SetArgument("targetUser", channelInfos.User.UserName);
                CPH.SetArgument("targetUserName", channelInfos.Slug);
                CPH.SetArgument("targetUserId", channelInfos.User.Id);
                CPH.SetArgument("targetDescription", channelInfos.User.Bio);
                CPH.SetArgument("targetDescriptionEscaped", WebUtility.UrlEncode(channelInfos.User.Bio));
                CPH.SetArgument("targetUserProfileImageUrl", channelInfos.User.ProfilePic);
                CPH.SetArgument("targetUserProfileImageUrlEscaped", WebUtility.UrlEncode(channelInfos.User.ProfilePic));
                CPH.SetArgument("targetUserProfileImageEscaped", WebUtility.UrlEncode(channelInfos.User.ProfilePic)); // Bugfix Streamer.bot qui se sont planté ! La doc est contradictoire.
                CPH.SetArgument("targetUserType", channelInfos.IsVerified ? "partner" : (channelInfos.IsAffiliate ? "affiliate" : String.Empty));
                CPH.SetArgument("targetIsAffiliate", channelInfos.IsAffiliate);
                CPH.SetArgument("targetIsPartner", channelInfos.IsVerified);
                CPH.SetArgument("targetLastActive", DateTime.Now);
                CPH.SetArgument("targetPreviousActive", DateTime.Now);
                CPH.SetArgument("targetIsSubscribed", userInfos.IsSubscriber);
                CPH.SetArgument("targetSubscriptionTier", (userInfos.SubscribedFor > 0 ? 1000 : 0));
                CPH.SetArgument("targetIsModerator", userInfos.IsModerator);
                CPH.SetArgument("targetIsVip", userInfos.IsVIP);
                CPH.SetArgument("targetIsFollowing", userInfos.IsFollowing);
                CPH.SetArgument("targetChannelTitle", String.Empty);
                CPH.SetArgument("game", null);
                CPH.SetArgument("gameId", null);
                CPH.SetArgument("createdAt", DateTime.Now);
                CPH.SetArgument("accountAge", 0);
                CPH.SetArgument("tagCount", 0);
                CPH.SetArgument("tags", new List<string>());
                CPH.SetArgument("tagsDelimited", String.Empty);

                if (channelInfos.LiveStream != null)
                {
                    CPH.SetArgument("targetChannelTitle", channelInfos.LiveStream.SessionTitle);
                    CPH.SetArgument("game", channelInfos.LiveStream.Categories?[0]?.Name);
                    CPH.SetArgument("gameId", channelInfos.LiveStream.Categories?[0]?.Id);

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
                        var latestCategory = categories[categories.Count - 1];
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
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de l'envoi de la réponse : {ex}");
            }
        }
    }
}
