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

            r.imageWidth = Constants.ImageWidth;
            r.imageHeight = Constants.ImageWidth;
            r.imageSize = new Vector2(r.imageWidth, r.imageHeight);

            r.imageOffsetX = lCol * (r.imageWidth + config.Padding);
            r.imageOffsetY = lRow * r.imageHeight;
            r.imageOffset = new Vector2(r.imageOffsetX, r.imageOffsetY);

            r.textOffsetX = lCol * (r.imageWidth + config.Padding);
            r.textOffsetY = (r.imageHeight * lRow) + r.imageHeight / 5;
            r.textOffset = new Vector2(0, 0);

            return r;
        }
    }
}
