using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Party;
using RaidBuffRecaster.Const;
using RaidBuffRecaster.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Service
{
    internal class RecastTimerService {
        internal static RecastTimerModel AddRecastTimer(PartyMember p, BuffActionModel.BuffAction b, Config config) {
            RecastTimerModel r = new RecastTimerModel();

            r.OwnerId = p.ObjectId;
            r.ActionId = b.ActionId;
            r.StatusId = b.StatusId;
            r.RecastTime = b.RecastTime;
            r.Image = b.Image;
            r.StopWatch = new System.Diagnostics.Stopwatch();

            return r;
        }

        internal static RecastTimerModel AddRecastTimer(PlayerCharacter? p, BuffActionModel.BuffAction b, Config config) {
            RecastTimerModel r = new RecastTimerModel();

            r.OwnerId = p.ObjectId;
            r.ActionId = b.ActionId;
            r.StatusId = b.StatusId;
            r.RecastTime = b.RecastTime;
            r.Image = b.Image;
            r.StopWatch = new System.Diagnostics.Stopwatch();

            return r;
        }
    }
}
