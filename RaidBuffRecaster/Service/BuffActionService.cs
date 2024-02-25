using RaidBuffRecaster.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaidBuffRecaster.Model.BuffActionModel;

namespace RaidBuffRecaster.Service {
    static class BuffActionService {
        public enum ActionIds {
            ArcaneCircle = 24405,
            BattleLitany = 3557,
            BattleVoice = 118,
            Brotherhood = 7396,
            ChainStratagem = 7436,
            Devilment = 16011,
            Divination = 16552,
            DragonSight = 7398,
            Embolden = 7520,
            Mug = 2248,
            QuadTechFinish = 16196,
            RadiantFinale = 25785,
            SearingLight = 25801,
        }

        public enum StatusIds {
            ArcaneCircle = 2599,
            BattleLitany = 786,
            BattleVoice = 141,
            Brotherhood = 1182,
            ChainStratagem = 1221,
            Devilment = 1825,
            Divination = 1878,
            DragonSight = 1910,
            Embolden = 1239,
            Mug = 638,
            QuadTechFinish = 1822,
            RadiantFinale = 2722,
            SearingLight = 2703,
        }

        public static List<BuffAction> Initialize(Dalamud.Plugin.DalamudPluginInterface pluginInterface, Config config) {
            List<BuffAction> buffActions = new List<BuffAction>();

            // ArcaneCircle
            BuffAction b = new BuffAction();
            b.JobId = (uint)JobIds.RPR;
            b.StatusId = (uint)StatusIds.ArcaneCircle;
            b.ActionId = (uint)ActionIds.ArcaneCircle;
            b.ActionName = ActionIds.ArcaneCircle.ToString();
            b.RecastTime = 120;
            var ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);
            b = new BuffAction();

            // "BattleLitany"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DRG;
            b.StatusId = (uint)StatusIds.BattleLitany;
            b.ActionId = (uint)ActionIds.BattleLitany;
            b.ActionName = ActionIds.BattleLitany.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "BattleVoice"
            b = new BuffAction();
            b.JobId = (uint)JobIds.BRD;
            b.StatusId = (uint)StatusIds.BattleVoice;
            b.ActionId = (uint)ActionIds.BattleVoice;
            b.ActionName = ActionIds.BattleVoice.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Brotherhood"
            b = new BuffAction();
            b.JobId = (uint)JobIds.MNK;
            b.StatusId = (uint)StatusIds.Brotherhood;
            b.ActionId = (uint)ActionIds.Brotherhood;
            b.ActionName = ActionIds.Brotherhood.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "ChainStratagem"
            b = new BuffAction();
            b.JobId = (uint)JobIds.SCH;
            b.StatusId = (uint)StatusIds.ChainStratagem;
            b.ActionId = (uint)ActionIds.ChainStratagem;
            b.ActionName = ActionIds.ChainStratagem.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Devilment"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DNC;
            b.StatusId = (uint)StatusIds.Devilment;
            b.ActionId = (uint)ActionIds.Devilment;
            b.ActionName = ActionIds.Devilment.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Divination"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = (uint)StatusIds.Divination;
            b.ActionId = (uint)ActionIds.Divination;
            b.ActionName = ActionIds.Divination.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "DragonSight"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DRG;
            b.StatusId = (uint)StatusIds.DragonSight;
            b.ActionId = (uint)ActionIds.DragonSight;
            b.ActionName = ActionIds.DragonSight.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Embolden"
            b = new BuffAction();
            b.JobId = (uint)JobIds.RDM;
            b.StatusId = (uint)StatusIds.Embolden;
            b.ActionId = (uint)ActionIds.Embolden;
            b.ActionName = ActionIds.Embolden.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Mug"
            b = new BuffAction();
            b.JobId = (uint)JobIds.NIN;
            b.StatusId = (uint)StatusIds.Mug;
            b.ActionId = (uint)ActionIds.Mug;
            b.ActionName = ActionIds.Mug.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "QuadTechFinish"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DNC;
            b.StatusId = (uint)StatusIds.QuadTechFinish;
            b.ActionId = (uint)ActionIds.QuadTechFinish;
            b.ActionName = ActionIds.QuadTechFinish.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "RadiantFinale"
            b = new BuffAction();
            b.JobId = (uint)JobIds.BRD;
            b.StatusId = (uint)StatusIds.RadiantFinale;
            b.ActionId = (uint)ActionIds.RadiantFinale;
            b.ActionName = ActionIds.RadiantFinale.ToString();
            b.RecastTime = 110;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "SearingLight"
            b = new BuffAction();
            b.JobId = (uint)JobIds.SMN;
            b.StatusId = (uint)StatusIds.SearingLight;
            b.ActionId = (uint)ActionIds.SearingLight;
            b.ActionName = ActionIds.SearingLight.ToString();
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // return
            return buffActions;
        }
    }
}