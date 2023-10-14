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
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Kick.Bot.BotClient;

namespace Kick.Bot
{
    internal static class BotChatCommander
    {
        private static List<BotChatCommand> commands = new List<BotChatCommand>();

        public static void ReloadCommands()
        {
            CPH.LogVerbose($"[Kick] Purge des commandes existantes, puis rechargement de la config.");
            /*commands.ForEach(botCommand =>
            {
                CPH.UnregisterCustomTrigger($"kickChatCommand.{botCommand.CommandInfo.Id}", new string[] { "Kick Chat" });
                CPH.UnregisterCustomTrigger($"kickChatCommandCooldown.{botCommand.CommandInfo.Id}", new string[] { "Kick Chat" });
            });*/
            commands.Clear();
            foreach(StreamerBotCommand botCommand in StreamerBotAppSettings.Commands)
            {
                var bcc = new BotChatCommand() {
                    CommandInfo = botCommand,
                    Counter = CPH.GetGlobalVar<long>("KickCommand.Counters." + botCommand.Id, botCommand.PersistCounter),
                    UsersCounters = CPH.GetGlobalVar<Dictionary<long, long>>("KickCommand.UsersCounters." + botCommand.Id, botCommand.PersistUserCounter)
                };
                if(bcc.UsersCounters == null)
                    bcc.UsersCounters = new Dictionary<long, long>();
                
                commands.Add(bcc);
                CPH.RegisterCustomTrigger($"[Kick] {botCommand.Name} ({botCommand.Command.Replace("\r\n", ", ")})", $"kickChatCommand.{botCommand.Id}", new string[] { "Kick", "Commands" });
                CPH.RegisterCustomTrigger($"[Kick] {botCommand.Name} [Cooldown] ({botCommand.Command.Replace("\r\n", ", ")})", $"kickChatCommandCooldown.{botCommand.Id}", new string[] { "Kick", "Commands" });
                
                CPH.LogVerbose($"[Kick] Commande ajoutée : {botCommand.Command} (Id={botCommand.Id})");
            }
            CPH.LogVerbose($"[Kick] {StreamerBotAppSettings.Commands.Count} commandes chargées");
        }


        public static bool Evaluate(ChatMessageEvent chatMessageEvent)
        {
            var isCommand = false;

            foreach (BotChatCommand botCommand in commands)
            {
                if (!botCommand.CommandInfo.Enabled)
                    continue;

                /* Vérification de la commande saisie */

                bool textCommandMatch = false;
                string inputCommand = null;
                string[] inputStrings = null;

                switch (botCommand.CommandInfo.Mode)
                {
                    // Matching Basic
                    case 0:
                        var textCommands = botCommand.CommandInfo.Command.Replace("\r\n", "\n").Split('\n');
                        foreach (var textCommand in textCommands)
                        {
                            var command = textCommand.Trim();
                            // Si le texte de la commande est vide, on ignore
                            if (command.Length < 2)
                                continue;

                            switch (botCommand.CommandInfo.Location)
                            {
                                // Début de phrase
                                case 0:
                                    if (chatMessageEvent.Content.StartsWith(command, botCommand.CommandInfo.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                                    {
                                        textCommandMatch = true;
                                        inputCommand = command;
                                        inputStrings = chatMessageEvent.Content.Substring(command.Length).Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    }
                                    break;

                                // Correspondance exacte
                                case 1:
                                    if (String.Compare(chatMessageEvent.Content, command, botCommand.CommandInfo.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        textCommandMatch = true;
                                        inputCommand = chatMessageEvent.Content.Trim();
                                        inputStrings = chatMessageEvent.Content.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    }
                                    break;

                                // N'importe où dans la chaine
                                case 2:
                                    if (botCommand.CommandInfo.CaseSensitive ?
                                        chatMessageEvent.Content.Contains(command) :
                                        chatMessageEvent.Content.ToLower().Contains(command.ToLower()))
                                    {
                                        textCommandMatch = true;
                                        inputCommand = command.Trim();
                                        inputStrings = chatMessageEvent.Content.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    }
                                    break;
                            }
                            // Si ça a matché, pas besoin de tester les autres lignes de texte
                            if (textCommandMatch)
                                break;
                        }
                        break;

                    // Matching Regex
                    case 1:
                        var regex = new Regex(botCommand.CommandInfo.Command);
                        textCommandMatch = regex.IsMatch(chatMessageEvent.Content);
                        inputCommand = botCommand.CommandInfo.Command;
                        inputStrings = chatMessageEvent.Content.Split(' ');
                        break;

                    // Matching Inconnu, on passe à la commande suivante
                    default: continue;
                }

                // La commande ne correspond pas, on passe à la suivante
                if (!textCommandMatch)
                    continue;

                var rawInput = String.Join(" ", inputStrings);

                /* Vérification de la liste des accès */

                bool deniedCheck = botCommand.CommandInfo.GrantType == 1;
                bool userPermitted = botCommand.CommandInfo.PermittedGroups.Count == 0 ^ deniedCheck;
                if (botCommand.CommandInfo.PermittedGroups.Count > 0)
                {
                    var groupMatch = (botCommand.CommandInfo.PermittedGroups.Contains("Moderators") && chatMessageEvent.Sender.IsModerator) ||
                        (botCommand.CommandInfo.PermittedGroups.Contains("VIPs") && chatMessageEvent.Sender.IsVIP) ||
                        (botCommand.CommandInfo.PermittedGroups.Contains("Subscribers") && chatMessageEvent.Sender.IsSubscriber);

                    userPermitted = groupMatch ^ deniedCheck;
                }
                // Le streamer a tous les droits :)
                if (chatMessageEvent.Sender.IsBroadcaster)
                    userPermitted = true;

                // Si l'utilisateur n'est pas autorisé à utiliser cette commande, on passe à la suivante
                if (!userPermitted)
                {
                    CPH.LogDebug($"[Kick] Commande rejetée, droits insuffisants. Caster={chatMessageEvent.Sender.IsBroadcaster} Mod={chatMessageEvent.Sender.IsModerator} VIP={chatMessageEvent.Sender.IsVIP} OG={chatMessageEvent.Sender.IsOG} Sub={chatMessageEvent.Sender.IsSubscriber}");
                    continue;
                }

                CPH.LogVerbose($"[Kick] Commande détectée ! {botCommand.CommandInfo.Command}");

                /* Vérification des cooldowns */
                bool onCooldown = false;
                double globalRem = 0;
                double userRem = 0;

                if (botCommand.CommandInfo.UserCooldown > 0)
                {
                    if (botCommand.UsersLastExec.TryGetValue(chatMessageEvent.Sender.Id, out var userLastExec) && (userRem = DateTime.Now.Subtract(userLastExec).TotalSeconds) < botCommand.CommandInfo.UserCooldown)
                    {
                        onCooldown = true;
                        userRem = botCommand.CommandInfo.UserCooldown - userRem;
                    }
                    else
                    {
                        botCommand.LastExec = DateTime.Now;
                    }
                }
                if (botCommand.CommandInfo.GlobalCooldown > 0)
                {
                    if (botCommand.LastExec.HasValue && (globalRem = DateTime.Now.Subtract(botCommand.LastExec.Value).TotalSeconds) < botCommand.CommandInfo.GlobalCooldown)
                    {
                        onCooldown = true;
                        globalRem = botCommand.CommandInfo.GlobalCooldown - globalRem;
                    }
                    else
                    {
                        botCommand.LastExec = DateTime.Now;
                    }
                }

                int role = 1;
                if (chatMessageEvent.Sender.IsVIP)
                    role = 2;
                if (chatMessageEvent.Sender.IsModerator)
                    role = 3;
                if (chatMessageEvent.Sender.IsBroadcaster)
                    role = 4;

                if (onCooldown)
                {
                    // Cooldown actif
                    var cdArguments = new Dictionary<string, object>() {
                        { "command", inputCommand },
                        { "commandId", botCommand.CommandInfo.Id },
                        { "commandSource", "kick" },
                        { "commandType", "message" },

                        { "user", chatMessageEvent.Sender.Username },
                        { "userName", chatMessageEvent.Sender.Slug },
                        { "userId", chatMessageEvent.Sender.Id },
                        { "userType", "kick" },
                        { "isSubscribed", chatMessageEvent.Sender.IsSubscriber },
                        { "isModerator", chatMessageEvent.Sender.IsModerator },
                        { "isVip", chatMessageEvent.Sender.IsVIP },
                        { "eventSource", "kick" },

                        { "cooldownLeft", Convert.ToInt64(Math.Max(globalRem, userRem)) },
                        { "globalCooldownLeft", Convert.ToInt64(globalRem) },
                        { "userCooldownLeft", Convert.ToInt64(userRem) },
                        { "fromKick", true }
                    };

                    CPH.TriggerCodeEvent($"kickChatCommandCooldown.{botCommand.CommandInfo.Id}", cdArguments);
                    CPH.TriggerCodeEvent(BotEventListener.BotEventType.ChatCommandCooldown, cdArguments);

                    isCommand = true;
                    continue;
                }

                /* Incrémentation des compteurs */
                ++botCommand.Counter;
                CPH.SetGlobalVar("KickCommand.Counters." + botCommand.CommandInfo.Id, botCommand.Counter, botCommand.CommandInfo.PersistCounter);

                if (!botCommand.UsersCounters.TryGetValue(chatMessageEvent.Sender.Id, out var userCurrentCounter))
                    userCurrentCounter = 0;
                ++userCurrentCounter;
                botCommand.UsersCounters.Remove(chatMessageEvent.Sender.Id);
                botCommand.UsersCounters.Add(chatMessageEvent.Sender.Id, userCurrentCounter);
                CPH.SetGlobalVar("KickCommand.UsersCounters." + botCommand.CommandInfo.Id, botCommand.UsersCounters, botCommand.CommandInfo.PersistUserCounter);

                // Fini ! Si on arrive là, c'est que la commande est valide, on peut la lancer
                var arguments = new Dictionary<string, object>() {
                    { "command", inputCommand },
                    { "commandId", botCommand.CommandInfo.Id },
                    { "commandSource", "kick" },
                    { "commandType", "message" },

                    { "rawInput", rawInput },
                    { "rawInputEscaped", rawInput },
                    { "rawInputUrlEncoded", System.Net.WebUtility.UrlEncode(rawInput) },

                    { "user", chatMessageEvent.Sender.Username },
                    { "userName", chatMessageEvent.Sender.Slug },
                    { "userId", chatMessageEvent.Sender.Id },
                    { "userType", "kick" },
                    { "isSubscribed", chatMessageEvent.Sender.IsSubscriber },
                    { "isModerator", chatMessageEvent.Sender.IsModerator },
                    { "isVip", chatMessageEvent.Sender.IsVIP },
                    { "eventSource", "command" },

                    { "msgId", chatMessageEvent.Id },
                    { "chatroomId", chatMessageEvent.ChatroomId },
                    { "role", role },
                    { "counter", botCommand.Counter },
                    { "userCounter", userCurrentCounter },
                    { "fromKick", true }
                };
                
                for(int i = 0; i < inputStrings.Length; ++i)
                {
                    arguments.Add($"input{i}", inputStrings[i]);
                    arguments.Add($"inputEscaped{i}", inputStrings[i]);
                    arguments.Add($"inputUrlEncoded{i}", System.Net.WebUtility.UrlEncode(inputStrings[i]));
                }

                CPH.TriggerCodeEvent($"kickChatCommand.{botCommand.CommandInfo.Id}", arguments);
                CPH.TriggerCodeEvent(BotEventListener.BotEventType.ChatCommand, arguments);

                isCommand = true;
            }

            return isCommand;
        }
    }

    internal class BotChatCommand
    {
        public StreamerBotCommand CommandInfo;
        public DateTime? LastExec = null;
        public Dictionary<long, DateTime> UsersLastExec = new Dictionary<long, DateTime>();
        public long Counter = 0;
        public Dictionary<long, long> UsersCounters = new Dictionary<long, long>();
    }
}
