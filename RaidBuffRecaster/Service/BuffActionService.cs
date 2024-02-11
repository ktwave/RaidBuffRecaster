using RaidBuffRecaster.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaidBuffRecaster.Model.BuffActionModel;

namespace RaidBuffRecaster.Service {
    static class BuffActionService {
        public static List<BuffAction> Initialize(Dalamud.Plugin.DalamudPluginInterface pluginInterface) {
            List<BuffAction> buffActions = new List<BuffAction>();

            // ArcaneCircle
            BuffAction b = new BuffAction();
            b.JobId = (uint)JobIds.RPR;
            b.StatusId = 2599;
            b.ActionId = 24405;
            b.ActionName = "ArcaneCircle";
            b.RecastTime = 120;
            var ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);
            b = new BuffAction();

            // "BattleLitany"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DRG;
            b.StatusId = 786;
            b.ActionId = 3557;
            b.ActionName = "BattleLitany";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "BattleVoice"
            b = new BuffAction();
            b.JobId = (uint)JobIds.BRD;
            b.StatusId = 141;
            b.ActionId = 118;
            b.ActionName = "BattleVoice";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Brotherhood"
            b = new BuffAction();
            b.JobId = (uint)JobIds.MNK;
            b.StatusId = 1182;
            b.ActionId = 7396;
            b.ActionName = "Brotherhood";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "ChainStratagem"
            b = new BuffAction();
            b.JobId = (uint)JobIds.SCH;
            b.StatusId = 1221;
            b.ActionId = 7436;
            b.ActionName = "ChainStratagem";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Devilment"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DNC;
            b.StatusId = 1825;
            b.ActionId = 16011;
            b.ActionName = "Devilment";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);

            buffActions.Add(b);

            // "Divination"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1878;
            b.ActionId = 16552;
            b.ActionName = "Divination";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "DragonSight"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DRG;
            b.StatusId = 1910;
            b.ActionId = 7398;
            b.ActionName = "DragonSight";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Embolden"
            b = new BuffAction();
            b.JobId = (uint)JobIds.RDM;
            b.StatusId = 1239;
            b.ActionId = 7520;
            b.ActionName = "Embolden";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "Mug"
            b = new BuffAction();
            b.JobId = (uint)JobIds.NIN;
            b.StatusId = 638;
            b.ActionId = 2248;
            b.ActionName = "Mug";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "QuadTechFinish"
            b = new BuffAction();
            b.JobId = (uint)JobIds.DNC;
            b.StatusId = 1822;
            b.ActionId = 16196;
            b.ActionName = "QuadTechFinish";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "RadiantFinale"
            b = new BuffAction();
            b.JobId = (uint)JobIds.BRD;
            b.StatusId = 2722;
            b.ActionId = 25785;
            b.ActionName = "RadiantFinale";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "SearingLight"
            b = new BuffAction();
            b.JobId = (uint)JobIds.SMN;
            b.StatusId = 2703;
            b.ActionId = 25801;
            b.ActionName = "SearingLight";
            b.RecastTime = 120;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "TheArrow"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1884;
            b.ActionId = 4402;
            b.ActionName = "TheArrow";
            b.RecastTime = 30;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "TheBalance"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1882;
            b.ActionId = 4401;
            b.ActionName = "TheBalance";
            b.RecastTime = 30;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "TheBole"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1883;
            b.ActionId = 4404;
            b.ActionName = "TheBole";
            b.RecastTime = 30;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "TheEwer"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1886;
            b.ActionId = 4405;
            b.ActionName = "TheEwer";
            b.RecastTime = 30;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "TheSpear"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1885;
            b.ActionId = 4403;
            b.ActionName = "TheSpear";
            b.RecastTime = 30;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // "TheSpire"
            b = new BuffAction();
            b.JobId = (uint)JobIds.AST;
            b.StatusId = 1887;
            b.ActionId = 4406;
            b.ActionName = "TheSpire";
            b.RecastTime = 30;
            ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\" + b.ActionId.ToString() + ".png");
            b.Image = pluginInterface.UiBuilder.LoadImage(ImagePath);
            buffActions.Add(b);

            // return
            return buffActions;
        }
    }
}