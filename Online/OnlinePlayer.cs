﻿using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RainMeadow
{
    public partial class OnlinePlayer : System.IEquatable<OnlinePlayer>
    {
        public CSteamID id;
        public SteamNetworkingIdentity oid;
        public Queue<PlayerEvent> OutgoingEvents = new(16);
        public List<PlayerEvent> recentlyAckedEvents = new(16);
        public Queue<ResourceState> OutgoingStates = new(128);
        private ulong nextOutgoingEvent = 1;
        public ulong lastEventFromRemote;
        public ulong lastAckFromRemote;
        internal ulong tick;
        public bool needsAck;
        public bool isMe;
        public string name;

        public OnlinePlayer(CSteamID id)
        {
            this.id = id;
            this.oid = new SteamNetworkingIdentity();
            oid.SetSteamID(id);
            isMe = this == OnlineManager.mePlayer;
            name = SteamFriends.GetFriendPersonaName(id);
        }

        internal void QueueEvent(PlayerEvent e)
        {
            RainMeadow.Debug($"Queued event {nextOutgoingEvent} to player {this}");
            e.eventId = this.nextOutgoingEvent;
            e.to = this;
            e.from = OnlineManager.mePlayer;
            nextOutgoingEvent++;
            OutgoingEvents.Enqueue(e);
        }

        internal PlayerEvent GetRecentEvent(ulong id)
        {
            return recentlyAckedEvents.First(e => e.eventId == id);
        }

        internal void AckFromRemote(ulong lastAck)
        {
            this.recentlyAckedEvents.Clear();
            this.lastAckFromRemote = lastAck;
            while (OutgoingEvents.Count > 0 && OnlineManager.IsNewerOrEqual(lastAck, OutgoingEvents.Peek().eventId)) recentlyAckedEvents.Add(OutgoingEvents.Dequeue());
        }

        internal ResourceRequest RequestResource(OnlineResource onlineResource)
        {
            RainMeadow.Debug($"Requesting player {this.name} for resource {onlineResource.Identifier()}");
            var req = new ResourceRequest(onlineResource);
            QueueEvent(req);
            return req;
        }

        internal TransferRequest TransferResource(OnlineResource onlineResource)
        {
            RainMeadow.Debug($"Requesting player {this.name} for transfer of {onlineResource.Identifier()}");
            var req = new TransferRequest(onlineResource, onlineResource.subscribers);
            QueueEvent(req);
            return req;
        }

        internal ReleaseRequest ReleaseResource(OnlineResource onlineResource)
        {
            RainMeadow.Debug($"Requesting player {this.name} for release of resource {onlineResource.Identifier()}");
            var req = new ReleaseRequest(onlineResource);
            QueueEvent(req);
            return req;
        }

        internal NewOwnerEvent NewOwnerEvent(OnlineResource onlineResource, OnlinePlayer owner)
        {
            RainMeadow.Debug($"Signaline player {this.name} of new owner for resource {onlineResource.Identifier()}");
            var req = new NewOwnerEvent(onlineResource, owner);
            QueueEvent(req);
            return req;
        }

        public override string ToString()
        {
            return $"{id} - {name}";
        }
        public override bool Equals(object obj) => this.Equals(obj as OnlinePlayer);
        public bool Equals(OnlinePlayer other)
        {
            return other != null && id == other.id;
        }
        public override int GetHashCode() => id.GetHashCode();


        public static bool operator ==(OnlinePlayer lhs, OnlinePlayer rhs)
        {
            return lhs is null ? rhs is null : lhs.Equals(rhs);
        }
        public static bool operator !=(OnlinePlayer lhs, OnlinePlayer rhs) => !(lhs == rhs);
    }
}
