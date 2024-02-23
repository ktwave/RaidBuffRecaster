using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Statuses;
using Dalamud.Game.ClientState.Structs;
using Dalamud.Game.Internal;
using Dalamud.Hooking;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Table;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Havok;
using ImGuiNET;
using Lumina.Data.Parsing;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json.Linq;
using RaidBuffRecaster.Const;
using RaidBuffRecaster.DataModel;
using RaidBuffRecaster.Model;
using RaidBuffRecaster.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentFreeCompanyProfile.FCProfile;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureMacroModule;
using static RaidBuffRecaster.Model.BuffActionModel;
using static System.Net.Mime.MediaTypeNames;

namespace RaidBuffRecaster {
    unsafe class RaidBuffRecaster : IDalamudPlugin {
        public string Name => "RaidBuffRecaster";
        bool isConfigOpen = false;

        List<Model.PartyMemberModel> localPartyList;
        List<BuffAction> BuffActions;
        List<RecastTimerModel> RecastTimers;
        public IDalamudTextureWrap imageBlackOut;

        internal Config config;
        internal static RaidBuffRecaster R;

        private DalamudPluginInterface PluginInterface { get; init; }

        public void Dispose() {
            DalamudService.PluginInterface.UiBuilder.Draw -= Draw;
            R = null;
        }

        public RaidBuffRecaster([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface) {
            PluginInterface = pluginInterface;

            // Initialize setting for buffs
            BuffActions = BuffActionService.Initialize(PluginInterface);

            R = this;
            PluginInterface.Create<DalamudService>();
            DalamudService.PluginInterface.UiBuilder.Draw += Draw;
            config = DalamudService.PluginInterface.GetPluginConfig() as Config ?? new Config();

            var ImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "images\\blackout.png");
            imageBlackOut = pluginInterface.UiBuilder.LoadImage(ImagePath);

            DalamudService.PluginInterface.UiBuilder.OpenConfigUi += delegate { isConfigOpen = true; };
            DalamudService.Framework.RunOnFrameworkThread(() => {
                if (config.Font != null) {
                    _ = DalamudService.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(config.Font.Value));
                }
            });
        }

        private void Draw() {
            try {
                if (isConfigOpen) MainService.DrawConfigWindow(ref config, ref isConfigOpen);

                if (!config.IsEnabled) return;

                if (DalamudService.ClientState.LocalPlayer == null) return;

                if (DalamudService.Condition[ConditionFlag.InCombat] && !DalamudService.ClientState.IsPvP) {
                    // in combat
                    MainService.DrawOverray(RecastTimers, config, imageBlackOut);
                } else {
                    // not in combat
                    MainService.UpdateRecastTimers(ref config, ref RecastTimers, BuffActions);
                    if(!config.IsInCombatOnly) MainService.DrawOverray(RecastTimers, config, imageBlackOut);
                }
            } catch (Exception e) {
                PluginLog.Error(e.Message + "\n" + e.StackTrace);
            } finally {
            }
        }
    }
}

