﻿using System;
using System.Collections.Generic;

namespace RainMeadow
{
    public partial class WorldSession
    {
        private List<AbstractPhysicalObject> earlyApos = new(); // stuff that gets added during world loading

        // Something entered this resource, check if it needs registering
        public void ApoEnteringWorld(AbstractPhysicalObject apo)
        {
            if (!isAvailable) throw new InvalidOperationException("not available");
            if (!OnlinePhysicalObject.map.TryGetValue(apo, out var oe)) // New to me
            {
                if(OnlineManager.lobby.gameMode.avatar != null && OnlineManager.lobby.gameMode.avatar.apo == apo)
                {
                    oe = OnlineManager.lobby.gameMode.avatar; // reuse this
                    OnlinePhysicalObject.map.Add(apo, oe);
                    OnlineManager.recentEntities.Add(oe.id, oe);
                }
                if (isActive)
                {
                    RainMeadow.Debug($"{this} - registering {apo}");
                    oe = OnlinePhysicalObject.RegisterPhysicalObject(apo);
                }
                else // world population generates before this can be activated // can't we simply mark it as active earlier?
                {
                    RainMeadow.Debug($"{this} - queuing up for later {apo}");
                    this.earlyApos.Add(apo);
                    return;
                }
            }
            oe.EnterResource(this);
        }

        public void ApoLeavingWorld(AbstractPhysicalObject apo)
        {
            RainMeadow.Debug(this);
            if (OnlinePhysicalObject.map.TryGetValue(apo, out var oe))
            {
                oe.LeaveResource(this);
            }
            else
            {
                RainMeadow.Error("Unregistered entity leaving");
            }
        }
    }
}
