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

                // get instance
                var ownerPlayer = DalamudService.ClientState.LocalPlayer;
                var actionManager = ActionManager.Instance();
                var partyList = DalamudService.PartyList;

                if (DalamudService.Condition[ConditionFlag.InCombat] && !DalamudService.ClientState.IsPvP) {
                    // in combat
                    var pos = MainService.GetPtlistPosition();
                    if (pos != null) MainService.DrawOverray(RecastTimers, (Vector2)pos, config);
                } else {
                    // not in combat
                    bool isLocalPartyListChanged = false;

                    if (localPartyList == null || localPartyList.Count() != partyList.Count()) {
                        // Initialize or partymember count change
                        localPartyList = new List<Model.PartyMemberModel>();
                        for (int i = 0; i < partyList.Count(); i++) {
                            // create
                            localPartyList.Add(PartyMemberService.CreatePartyMember(partyList[i], i));
                        }
                        isLocalPartyListChanged = true;
                    } else {
                        for (int i = 0; i < localPartyList.Count(); i++) {
                            // compate partyList
                            if (PartyMemberService.ComparePartyMember(localPartyList[i], partyList[i], i)) {
                                // existed change -> update
                                localPartyList[i] = PartyMemberService.UpdatePartyMember(localPartyList[i], partyList[i], i);
                                isLocalPartyListChanged = true;
                            }
                        }
                    }

                    if (isLocalPartyListChanged) {
                        var lRow = 0;
                        var lCol = 0;
                        RecastTimers = new List<RecastTimerModel>();
                        for (int i = 0; i < localPartyList.Count(); i++) {
                            var p = localPartyList[i];
                            var bas = BuffActions.Where(b => b.JobId == p.JobId).ToList();
                            for (int j = 0; j < bas.Count(); j++) {
                                RecastTimers.Add(RecastTimerService.AddRecastTimer(p, bas[j], lCol, lRow, config));
                                lCol++;
                                if (lCol == config.Columns) {
                                    lCol = 0;
                                    lRow++;
                                }
                            }
                        }
                    }

                    if (config.IsPreview) {
                        var pos = MainService.GetPtlistPosition();
                        if (pos != null) MainService.DrawOverrayPreview((Vector2)pos, BuffActions, config);
                    }
                }
            } catch (Exception e) {
                PluginLog.Error(e.Message + "\n" + e.StackTrace);
            } finally {
            }
        }
    }
}

