using Dalamud.Interface.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Model {
    internal class BuffActionModel {
        public class BuffAction {
            public uint JobId { get; set; }
            public uint StatusId { get; set; }
            public uint ActionId { get; set; }
            public string ActionName { get; set; }
            public float RecastTime { get; set; }
            public IDalamudTextureWrap Image { get; set; }

            void SetJobId(uint JobId) {
                this.JobId = JobId;
            }
            void SetStatusId(uint StatusId) {
                this.StatusId = StatusId;
            }
            void SetActionId(uint ActionId) {
                this.ActionId = ActionId;
            }
            void SetActionName(string ActionName) {
                this.ActionName = ActionName;
            }
            void SetRecastTime(float RecastTime) {
                this.RecastTime = RecastTime;
            }
            void SetImage(IDalamudTextureWrap Image) {
                this.Image = Image;
            }
        }
    }
}
