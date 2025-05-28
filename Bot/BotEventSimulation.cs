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
using System.Threading.Tasks;

namespace Kick.Bot
{
    /*public sealed partial class BotEventListener
    {
        public void SimulateEvent(Dictionary<string, dynamic> args)
        {
            try
            {
                int eventType = (int)KickEventListener.SimulateEventType.All;
                if (args.TryGetValue("testEvent", out var eventTypeId))
                    eventType = Convert.ToInt32(eventTypeId);

                EventListener.Simulate((KickEventListener.SimulateEventType)eventType, Channel);
            }
            catch (Exception ex)
            {
                CPH.LogDebug($"[Kick] Une erreur s'est produite lors de la demande de simulation : {ex}");
            }
        }
    }*/
}
