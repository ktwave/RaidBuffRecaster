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
    }
}
