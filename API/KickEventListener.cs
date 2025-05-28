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

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PusherClient;
using Kick.API.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kick.API.Models;

namespace Kick.API
{
    public sealed class KickEventListener
    {
        private const string KickAppId = "32cbd69e4b950bf97679";
        private const string KickCluster = "us2";

        // [CHAT] Nouveau Message
        public delegate void OnChatMessageHandler(ChatMessageEvent message);
        public event OnChatMessageHandler OnChatMessage;


        // [Chat] Message supprimé
        public delegate void OnChatMessageDeletedHandler(ChatMessageDeletedEvent message);
        public event OnChatMessageDeletedHandler OnChatMessageDeleted;

        // [PUB] Stream Started
        public delegate void OnStreamStartedHandler(LivestreamStartedEvent kickEvent);
        public event OnStreamStartedHandler OnStreamStarted;

        // [PUB] Stream Ended
        public delegate void OnStreamEndedHandler(LivestreamStoppedEvent kickEvent);
        public event OnStreamEndedHandler OnStreamEnded;

        // [PUB] Nouveau Follower
        public delegate void OnNewFollowerHandler(ChannelFollowEvent followEvent);
        public event OnNewFollowerHandler OnViewerFollow;

        // [PUB] Nouveau Sub
        public delegate void OnNewSubscriberHandler(SubscriptionEvent subEvent);
        public event OnNewSubscriberHandler OnSubscription;

        // [PUB] Nouveau.x Gift.s
        public delegate void OnSubGiftedHandler(GiftedSubscriptionEvent giftEvent);
        public event OnSubGiftedHandler OnSubGift;

        // [PUB] Mode de chat modifié
        public delegate void OnChatUpdatedHandler(ChatUpdatedEvent chatUpdateEvent);
        public event OnChatUpdatedHandler OnChatUpdated;

        // [PUB] Sondage créé
        public delegate void OnPollCreatedHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollCreatedHandler OnPollCreated;

        // [PUB] Sondage mis à jour
        public delegate void OnPollUpdatedHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollUpdatedHandler OnPollUpdated;

        // [PUB] Sondage terminé
        public delegate void OnPollCompletedHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollCompletedHandler OnPollCompleted;

        // [PUB] Sondage annulé
        public delegate void OnPollCancelledHandler(PollUpdateEvent pollUpdateEvent);
        public event OnPollCancelledHandler OnPollCancelled;

        // [PUB] Message épinglé
        public delegate void OnMessagePinnedHandler(PinnedMessageEvent pinnedMessageEvent);
        public event OnMessagePinnedHandler OnMessagePinned;

        // [PUB] Message détaché
        public delegate void OnMessageUnpinnedHandler();
        public event OnMessageUnpinnedHandler OnMessageUnpinned;
        
        // [PUB] Récompense achetée
        public delegate void OnRewardRedeemedHandler(RewardRedeemedEvent rewardRedeemedEvent);
        public event OnRewardRedeemedHandler OnRewardRedeemed;

        // [MOD] Termes bloqués
        /*public delegate void OnBannedWordChangedHandler(BannedWordEvent bannedWordEvent);
        public event OnBannedWordChangedHandler OnBannedWordChanged;*/

        // [MOD] Bannissement
        public delegate void OnUserBannedHandler(BannedUserEvent bannedUserEvent);
        public event OnUserBannedHandler OnUserBanned;

        // [MOD] Changement de mode du chat
        public delegate void OnChatModeChangedHandler(ChatModeChangedEvent chatModeEvent);
        public event OnChatModeChangedHandler OnChatModeChanged;
        
        // [MOD] Modification de la liste de termes bannis
        public delegate void OnWordBannedHandler(BannedWordEvent bannedWordEvent);
        public event OnWordBannedHandler OnWordBanned;
        
        // [MOD] Raid entrant
        public delegate void OnRaidHandler(RaidEvent raidEvent);
        public event OnRaidHandler OnRaid;
        
        // [MOD] Stream Infos Updated
        public delegate void OnStreamUpdatedHandler(LivestreamUpdatedEvent livestreamEvent);
        public event OnStreamUpdatedHandler OnStreamUpdated;
        
        public delegate void OnPredictionCreatedHandler(Prediction prediction);
        public event OnPredictionCreatedHandler OnPredictionCreated;
        
        public delegate void OnPredictionUpdatedHandler(Prediction prediction);
        public event OnPredictionUpdatedHandler OnPredictionUpdated;
        
        private readonly Pusher _pusherClient;
        //private delegate void PusherEventHandler(string eventType, PusherEvent eventData);
        
        private readonly Dictionary<long, Models.Channel>
            _channels = new Dictionary<long, Models.Channel>();
        
        private readonly Dictionary<Models.Channel, List<string>> _registrations = new Dictionary<Models.Channel, List<string>>();
        
        public KickClient Client { get; }
        public bool IsConnected => _pusherClient.State == ConnectionState.Connected;
        
        private readonly Dictionary<long, PollUpdateEvent> _currentPolls = new Dictionary<long, PollUpdateEvent>();
        
        internal KickEventListener(KickClient client, IAuthorizer authorizer)
        {
            Client = client;
            Console.WriteLine($@"Pusher init, AppID: {KickAppId} (Kick.com), Cluster: {KickCluster}");
            _pusherClient = new Pusher(KickAppId, new PusherOptions() { Cluster = KickCluster, Authorizer = authorizer });
            _pusherClient.BindAll(HandleEvent);
        }
        
        public async Task JoinAsync(Models.Channel channel)
        {
            if (_channels.ContainsKey(channel.Id))
                return;
            Console.WriteLine($@"Connecting to chatroom {channel.Slug} ... (Chatroom: {channel.Chatroom.Id}, Channel: {channel.Id})");
            await RegisterChannel(channel);
        }

        private async Task RegisterChannel(Models.Channel channel)
        {
            var channels = new List<string> {
                $"chatrooms.{channel.Chatroom.Id}.v2",
                $"predictions-channel-{channel.Id}"
            };

            if (channel.Role == "Channel Host" || channel.Role == "Moderator")
            {
                channels.AddRange(new[] {
                    $"private-chatroom_{channel.Chatroom.Id}",
                    $"private-channel_{channel.Id}"
                });

                if (channel.LiveStream != null)
                {
                    channels.AddRange(new[] {
                        $"private-livestream_{channel.LiveStream.Id}",
                        $"private-livestream-updated.{channel.LiveStream.Id}"
                    });
                }
            }
            
            if (_pusherClient.State != ConnectionState.Connected)
            {
                await _pusherClient.ConnectAsync();
            }
            
            _channels[channel.Id] = channel;
            if (!_registrations.ContainsKey(channel))
            {
                _registrations[channel] = new List<string>();
            }

            foreach (var c in channels)
            {
                try
                {
                    // Already connected to this channel, ignore it
                    if(_registrations[channel].Contains(c))
                        continue;
                    
                    // Subscribe
                    await _pusherClient.SubscribeAsync(c);
                    
                    // Keep track of subscribed channels
                    _registrations[channel].Add(c);
                    Console.WriteLine($@"Connected to {c}");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        
        private void HandleEvent(string eventType, PusherEvent eventData)
        {
            var pChannel = eventData.ChannelName;
            var channel = _registrations
                .Where(x => x.Value.Contains(eventData.ChannelName))
                .Select(x => x.Key).FirstOrDefault();

            if (channel == null)
            {
                Console.WriteLine($@"Event received for an unknown channel {pChannel}");
                return;
            }
            
            ChatMode? modeChanged = null;
            
            switch (eventType)
            {
                // chatrooms.<id>.v2
                case "App\\Events\\ChatMessageEvent":
                    var chatMessageEvent = JsonConvert.DeserializeObject<ChatMessageEvent>(eventData.Data);
                    OnChatMessage?.Invoke(chatMessageEvent);
                    return;
                case "App\\Events\\MessageDeletedEvent":
                case "App\\Events\\ChatroomClearEvent":
                    var deletedMessage = JsonConvert.DeserializeObject<ChatMessageDeletedEvent>(eventData.Data);
                    OnChatMessageDeleted?.Invoke(deletedMessage);
                    return;
                case "App\\Events\\ChatroomUpdatedEvent":
                    var updatedChat = JsonConvert.DeserializeObject<ChatUpdatedEvent>(eventData.Data);
                    OnChatUpdated?.Invoke(updatedChat);
                    return;
                case "App\\Events\\SubscriptionEvent":
                    // TODO
                    // "{\"chatroom_id\":<id>,\"username\":\"<name>\",\"months\":<duration>}"
                    return;
                case "App\\Events\\PollUpdateEvent":
                    var pollUpdate = JsonConvert.DeserializeObject<PollUpdateEvent>(eventData.Data);
                    pollUpdate.Channel = channel;
                    PollUpdate(channel, pollUpdate);
                    return;
                case "App\\Events\\PollDeleteEvent":
                    PollUpdate(channel, null);
                    return;
                case "App\\Events\\PinnedMessageCreatedEvent":
                    var pinnedMessage = JsonConvert.DeserializeObject<PinnedMessageEvent>(eventData.Data);
                    OnMessagePinned?.Invoke(pinnedMessage);
                    return;
                case "App\\Events\\PinnedMessageDeletedEvent":
                    OnMessageUnpinned?.Invoke();
                    return;
                case "App\\Events\\UserBannedEvent":
                case "App\\Events\\UserUnbannedEvent":
                    // ignored
                    return;
                
                // channel.<id>
                case "App\\Events\\ChannelSubscriptionEvent":
                    // TODO
                    // "{\"user_ids\":[<id>],\"username\":\"<user>\",\"channel_id\":<channelId>}"
                    return;
                case "App\\Events\\StreamerIsLive":
                    var startEvent = JsonConvert.DeserializeObject<LivestreamStartedEvent>(eventData.Data);
                    OnStreamStarted?.Invoke(startEvent);
                    channel = Client.GetChannelInfos(channel.Slug).Result;
                    RegisterChannel(channel).Wait();
                    return;
                case "App\\Events\\StopStreamBroadcast":
                    var endEvent = JsonConvert.DeserializeObject<LivestreamStoppedEvent>(eventData.Data);
                    OnStreamEnded?.Invoke(endEvent);
                    return;
                
                // private-chatroom_<id>
                case "BannedWordAdded":
                    var bwAdd = JsonConvert.DeserializeObject<BannedWordEvent>(eventData.Data);
                    OnWordBanned?.Invoke(bwAdd);
                    return;
                case "BannedWordDeleted":
                    var bwDel = JsonConvert.DeserializeObject<BannedWordEvent>(eventData.Data);
                    bwDel.IsWordBanned = false;
                    OnWordBanned?.Invoke(bwDel);
                    return;
                case "BannedUserAdded":
                case "BannedUserDeleted":
                case "UserTimeouted":
                    var message = JsonConvert.DeserializeObject<BannedUserEvent>(eventData.Data);
                    OnUserBanned?.Invoke(message);
                    return;
                case "MessageDeleted":
                    // TODO - Utilité ? Remonte déjà dans le flux public
                    break;
                case "SlowModeActivated":
                    modeChanged = ChatMode.SlowModeEnabled;
                    break;
                case "SlowModeDeactivated":
                    modeChanged = ChatMode.SlowModeDisabled;
                    break;
                case "EmotesModeActivated":
                    modeChanged = ChatMode.EmotesOnlyEnabled;
                    break;
                case "EmotesModeDeactivated":
                    modeChanged = ChatMode.EmotesOnlyDisabled;
                    break;
                case "FollowersModeActivated":
                    modeChanged = ChatMode.FollowersOnlyEnabled;
                    break;
                case "FollowersModeDeactivated":
                    modeChanged = ChatMode.FollowersOnlyDisabled;
                    break;
                case "SubscribersModeActivated":
                    modeChanged = ChatMode.SubsOnlyEnabled;
                    break;
                case "SubscribersModeDeactivated":
                    modeChanged = ChatMode.SubsOnlyDisabled;
                    break;
                case "AllowLinksActivated":
                    modeChanged = ChatMode.AllowLinksActivated;
                    break;
                case "AllowLinksDeactivated":
                    modeChanged = ChatMode.AllowLinksDeactivated;
                    break;
                case "MessagePinned":
                case "MessageUnpinned":
                case "PollCreated":
                case "PollDeleted":
                    // ignored, duplicate
                    break;
                
                // private-channel_<id>
                case "FollowerAdded":
                    var followAdded = JsonConvert.DeserializeObject<ChannelFollowEvent>(eventData.Data);
                    OnViewerFollow?.Invoke(followAdded);
                    return;
                case "FollowerDeleted":
                    var followDeleted = JsonConvert.DeserializeObject<ChannelFollowEvent>(eventData.Data);
                    followDeleted.IsFollowing = false;
                    OnViewerFollow?.Invoke(followDeleted);
                    return;
                case "SubscriptionCreated":
                case "SubscriptionRenewed":
                    var newSub = JsonConvert.DeserializeObject<SubscriptionEvent>(eventData.Data);
                    OnSubscription?.Invoke(newSub);
                    return;
                case "SubscriptionGifted":
                    var newGift = JsonConvert.DeserializeObject<GiftedSubscriptionEvent>(eventData.Data);
                    OnSubGift?.Invoke(newGift);
                    return;
                case "RedeemedReward":
                    var newRedeem = JsonConvert.DeserializeObject<RewardRedeemedEvent>(eventData.Data);
                    OnRewardRedeemed?.Invoke(newRedeem);
                    return;
                
                // private-livestream.<id>
                case "HostReceived":
                    var raid = JsonConvert.DeserializeObject<RaidEvent>(eventData.Data);
                    OnRaid?.Invoke(raid);
                    return;
                case "TitleChanged":
                case "CategoryChanged":
                case "MatureModeActivated":
                case "MatureModeDeactivated":
                    return;
                
                // private-livestream-updated.<id>
                case "App\\Events\\LiveStream\\UpdatedLiveStreamEvent":
                    var liveUpdate =
                        JsonConvert.DeserializeObject<LivestreamUpdatedEvent>(eventData.Data);
                    channel = Client.GetChannelInfos(channel.Slug).Result;
                    liveUpdate.Channel = channel;
                    OnStreamUpdated?.Invoke(liveUpdate);
                    return;
                
                // predictions-channel-<id>
                case "PredictionCreated":
                    var predCreated = JsonConvert.DeserializeObject<PredictionResponse>(eventData.Data);
                    OnPredictionCreated?.Invoke(predCreated.Prediction);
                    return;
                case "PredictionUpdated":
                    var predUpdated = JsonConvert.DeserializeObject<PredictionResponse>(eventData.Data);
                    OnPredictionUpdated?.Invoke(predUpdated.Prediction);
                    return;
                
                default:
                    Console.WriteLine(
                        $@"{DateTime.Now.ToShortTimeString()} [Pusher] Unknown event triggered => {eventType} : {eventData}");
                    break;
            }
            
            if (modeChanged.HasValue)
            {
                var message = JsonConvert.DeserializeObject<ChatModeChangedEvent>(eventData.Data);
                message.ChatMode = modeChanged.Value;
                OnChatModeChanged?.Invoke(message);
            }
        }

        public async Task LeaveAsync(Models.Channel channel)
        {
            if (!_channels.ContainsKey(channel.Id))
                return;
            
            var registrations = _registrations[channel];
            _registrations.Remove(channel);
            _channels.Remove(channel.Id);
            
            Console.WriteLine($@"Stopped listening for events from {channel.Slug}");

            foreach (var registration in registrations)
            {
                await _pusherClient.UnsubscribeAsync(registration);                
            }
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine($@"Connecting...");
            await _pusherClient.ConnectAsync();
            _registrations.Clear();
            if (_channels.Any())
            {
                var channels = new Dictionary<long, Models.Channel>(_channels);
                _channels.Clear();
                foreach (var channel in channels)
                {
                    await JoinAsync(channel.Value);
                }
            }
            Console.WriteLine($@"Connected.");
        }
        
        public async Task DisconnectAsync()
        {
            Console.WriteLine($@"Disconnecting...");
            await _pusherClient.DisconnectAsync();
            Console.WriteLine($@"Disconnected.");
        }

        private void PollUpdate(Models.Channel channel, PollUpdateEvent pollUpdateEvent)
        {
            if(!_currentPolls.ContainsKey(channel.Id))
            {
                _currentPolls[channel.Id] = pollUpdateEvent;
                var pollUpdater = new Thread(() =>
                {
                    // Poll Created
                    OnPollCreated?.Invoke(pollUpdateEvent);

                    // Wait for poll to end
                    var waitHandle = new AutoResetEvent(false);
                    var remaining = pollUpdateEvent.Poll.Remaining;
                    var timer = new Timer(state => {
                        if(!_currentPolls.TryGetValue(channel.Id, out var currentPoll))
                        {
                            // Not supposed to happen, thread will be stopped
                            currentPoll = pollUpdateEvent; // Restore context
                            pollUpdateEvent.State = PollState.Cancelled;
                        } else
                        {
                            pollUpdateEvent = currentPoll; // Save context
                        }

                        if(pollUpdateEvent.State == PollState.Cancelled)
                        {
                            OnPollCancelled?.Invoke(pollUpdateEvent);
                            waitHandle.Set();
                            return;
                        }

                        currentPoll.Poll.Remaining = --remaining;
                        if (currentPoll.Poll.Remaining >= 0)
                        {
                            OnPollUpdated?.Invoke(currentPoll);
                        }
                        else
                        {
                            currentPoll.Poll.Remaining = 0;
                            currentPoll.State = PollState.Completed;
                            OnPollCompleted?.Invoke(currentPoll);
                            waitHandle.Set();
                        }
                    }, null, 1000, 1000);
                    waitHandle.WaitOne();

                    // Stop Timer
                    timer.Dispose();

                    // Poll completed
                    _currentPolls.Remove(channel.Id);
                });
                pollUpdater.Start();
            }
            else {
                if (pollUpdateEvent != null)
                {
                    _currentPolls[channel.Id].Poll = pollUpdateEvent.Poll;
                }
                else
                {
                    _currentPolls[channel.Id].State = PollState.Cancelled;
                }
            }
        }
    }
}
