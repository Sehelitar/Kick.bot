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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Kick.Bot
{
    internal static class BotTimedActionManager
    {
        public static readonly Random Random = new Random(481516234);
        private static readonly List<BotTimedAction> actions = new List<BotTimedAction>();
        private static readonly Timer ticker;

        static BotTimedActionManager() {
            ticker = new Timer(200)
            {
                Enabled = true
            };
            ticker.Elapsed += Ticker_Elapsed;
            ticker.Start();
        }

        private static void Ticker_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var action in actions)
            {
                if(action.IsActive())
                {
                    if(action.HasTimeConstraint() && action.TimeUntil > 0d)
                        action.TimeUntil -= .2d;

                    if(action.AreConditionsMet())
                    {
                        BotClient.CPH.LogInfo($"[Kick] Timed action \"{action.TimedAction.Name}\" (ID {action.TimedAction.Id}) triggered!");
                        action.AlreadyTriggered = true;
                        action.ResetConditions();
                        BotClient.CPH.TriggerCodeEvent("kickTimedAction." + action.TimedAction.Id, false);
                    }
                }
            }
        }

        public static void ReloadTimedActions()
        {
            var rawActions = StreamerBotAppSettings.Settings.TimedActions.Timers;
            BotClient.CPH.LogInfo($"[Kick] {rawActions.Count} timed action found");

            foreach (var rawAction in rawActions)
            {
                var action = (from ex in actions where ex.TimedAction.Id == rawAction.Id select ex).FirstOrDefault() ?? new BotTimedAction();
                action.TimedAction = rawAction;

                BotClient.CPH.LogInfo($"[Kick] Resetting timed action \"{rawAction.Name}\" (ID {rawAction.Id})");
                action.ResetConditions();
                BotClient.CPH.RegisterCustomTrigger("[Kick] Timed Action / " + rawAction.Name, "kickTimedAction."+rawAction.Id, new string[] { "Kick", "Timed Actions" });

                if(!actions.Contains(action))
                    actions.Add(action);
            }
        }

        public static void MessageReceived()
        {
            foreach (var action in actions) {
                if(action.IsActive() && action.HasLinesConstraint() && action.MessagesUntil > 0)
                    --action.MessagesUntil;
            }
        }
    }

    internal class BotTimedAction
    {
        public TimedAction TimedAction { get; set; }
        public double TimeUntil { get; set; }
        public int MessagesUntil { get; set; }
        public bool AlreadyTriggered { get; set; } = false;

        public bool IsActive() => TimedAction.Enabled && (!AlreadyTriggered || TimedAction.Repeat);
        public bool HasTimeConstraint() => TimedAction.Interval > 0 || (TimedAction.RandomInterval && TimedAction.UpperInterval > 0);
        public bool HasLinesConstraint() => TimedAction.Lines > 0;

        public bool AreConditionsMet()
        {
            if(!IsActive())
                return false;

            var timeConstraintOk = false;
            var linesConstraintOk = false;

            if (!HasTimeConstraint() || TimeUntil <= 0d)
                timeConstraintOk = true;

            if(!HasLinesConstraint() || MessagesUntil <= 0d)
                linesConstraintOk = true;

            return timeConstraintOk && linesConstraintOk;
        }

        public void ResetConditions()
        {
            TimeUntil = TimedAction.Interval;
            if(TimedAction.RandomInterval)
                TimeUntil = BotTimedActionManager.Random.Next(TimedAction.UpperInterval - TimedAction.Interval) + TimedAction.Interval;
            MessagesUntil = TimedAction.Lines;
        }
    }
}
