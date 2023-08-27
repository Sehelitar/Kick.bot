using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kick.Bot
{
    public sealed partial class BotEventListener
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
    }
}
