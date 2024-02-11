using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Component.GUI;
using RaidBuffRecaster.DataModel;
using RaidBuffRecaster.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Service {
    internal class MainService {
        public static Dalamud.Game.ClientState.Statuses.Status GetStatus(PartyMember p, RecastTimerModel r) {
            if (p.ClassJob.Id == (uint)JobIds.NIN || p.ClassJob.Id == (uint)JobIds.SCH) {
                GameObject target = DalamudService.TargetManager.Target;
                if (target is Dalamud.Game.ClientState.Objects.Types.BattleChara b) {
                    var statusList = b.StatusList.Where(s => s.StatusId != 0).ToList();
                    return statusList.Where(s => s.StatusId == r.StatusId).FirstOrDefault();
                }
            } else {
                // get target status
                return p.Statuses.Where(p => p.StatusId == r.StatusId).FirstOrDefault();
            }
            return null;
        }

        internal unsafe static Vector2? GetPtlistPosition() {
            var ptlist = DalamudService.GameGui.GetAddonByName("_PartyList", 1);
            if (ptlist != IntPtr.Zero) {
                var ptlistAtk = (AtkUnitBase*)ptlist;
                var x = ptlistAtk->X;
                var y = ptlistAtk->Y;
                if (ptlistAtk->IsVisible) {
                    return new Vector2(x, y);
                }
            }
            return null;
        }
    }
}
