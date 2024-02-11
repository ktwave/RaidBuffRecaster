using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Model {
    internal class ParryMemberModel {
        public int Index { get; set; }
        public uint ObjectId { get; set; }
        public string MemberName { get; set; }
        public uint JobId { get; set; }
        void SetIndex(int Index) {
            this.Index = Index;
        }
        void SetObjectId(uint ObjectId) {
            this.ObjectId = ObjectId;
        }
        void SetMemberName(string MemberName) {
            this.MemberName = MemberName;
        }
        void SetJobId(uint JobId) {
            this.JobId = JobId;
        }
    }
}
