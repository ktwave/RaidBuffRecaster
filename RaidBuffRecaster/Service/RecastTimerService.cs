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
        internal static RecastTimerModel AddRecastTimer(PartyMemberModel p, BuffActionModel.BuffAction b, int lCol, int lRow, Config config) {
            RecastTimerModel r = new RecastTimerModel();

            r.OwnerId = p.ObjectId;
            r.ActionId = b.ActionId;
            r.StatusId = b.StatusId;
            r.RecastTime = b.RecastTime;
            r.Image = b.Image;

            r.row = lRow;
            r.col = lCol;

            return r;
        }
    }
}
