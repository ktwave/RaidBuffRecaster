using Dalamud.Game.ClientState.Party;
using RaidBuffRecaster.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Service
{
    public static class PartyMemberService
    {
        internal static PartyMemberModel CreatePartyMember(PartyMember? p, int i)
        {
            PartyMemberModel lp = new PartyMemberModel();
            lp.Index = i;
            lp.ObjectId = p.ObjectId;
            lp.MemberName = p.Name.ToString();
            lp.JobId = p.ClassJob.Id;
            return lp;
        }

        internal static PartyMemberModel UpdatePartyMember(PartyMemberModel lp, PartyMember? p, int i)
        {
            lp.Index = i;
            lp.ObjectId = p.ObjectId;
            lp.MemberName = p.Name.ToString();
            lp.JobId = p.ClassJob.Id;
            return lp;
        }

        internal static bool ComparePartyMember(PartyMemberModel lp, PartyMember? p, int i)
        {
            return lp.ObjectId != p.ObjectId || lp.JobId != p.ClassJob.Id || lp.Index == i;
        }
    }
}
