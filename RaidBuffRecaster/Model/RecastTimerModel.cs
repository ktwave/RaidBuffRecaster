using Dalamud.Interface.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Model {
    internal class RecastTimerModel {
        internal uint OwnerId { get; set; }
        internal uint ActionId { get; set; }
        internal uint StatusId { get; set; }
        internal float RecastTime { get; set; }
        internal Stopwatch StopWatch { get; set; }
        internal IDalamudTextureWrap Image { get; set; }

        internal int col { get; set; }
        internal int row { get; set; } 

        internal float imageWidth { get; set; }
        internal float imageHeight { get; set; }
        internal Vector2 imageSize { get; set; }

        internal float imageOffsetX { get; set; }
        internal float imageOffsetY { get; set; }
        internal Vector2 imageOffset { get; set; }

        internal float textOffsetX { get; set; }
        internal float textOffsetY { get; set; }
        internal Vector2 textOffset { get; set; }
    }
}
