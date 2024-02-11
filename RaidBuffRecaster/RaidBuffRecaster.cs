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

namespace RaidBuffRecaster
{
    unsafe class RaidBuffRecaster : IDalamudPlugin {
        public string Name => "RaidBuffRecaster";
        bool isConfigOpen = false;

        List<Model.ParryMemberModel> localPartyList;
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
            if (isConfigOpen) { // config
                if (ImGui.Begin("RaidBuffRecaster Config", ref isConfigOpen, ImGuiWindowFlags.NoResize)) {
                    ImGui.SetWindowSize(new Vector2(350, 550));

                    var isEnabled = config.IsEnabled;
                    if(ImGui.Checkbox("プラグインを有効にする(Enable Plugin)", ref isEnabled)) {
                        config.IsEnabled = isEnabled;
                    }

                    ImGui.Spacing();

                    var isPreview = config.IsPreview;
                    if (ImGui.Checkbox("プレビュー(Preview)", ref isPreview)) {
                        config.IsPreview = isPreview;
                        // DrawPreview();
                    }
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.Text("X座標のオフセット(X Offset)");
                    var offsetX = (int)config.OffsetX;
                    ImGui.SetNextItemWidth(200f);
                    if (ImGui.DragInt(" ", ref offsetX, 0.5f)) {
                        config.OffsetX = offsetX;
                    }
                    ImGui.Spacing();

                    ImGui.Text("Y座標のオフセット(Y Offset)");
                    var offsetY = (int)config.OffsetY;
                    ImGui.SetNextItemWidth(200f);
                    if (ImGui.DragInt("  ", ref offsetY, 0.5f)) {
                        config.OffsetY = offsetY;
                    }
                    ImGui.Spacing();

                    ImGui.Text("アイコンの拡大率(Icon Scale)");
                    var size = (int)config.Size;
                    ImGui.SetNextItemWidth(200f);
                    if (ImGui.DragInt("   ", ref size, 1, 1, 300)) {
                        config.Size = size;
                    }
                    ImGui.Spacing();

                    ImGui.Text("1列辺りのアイコン数(Icon Columns)");
                    var columns = config.Columns;
                    ImGui.SetNextItemWidth(200f);
                    if (ImGui.DragInt("    ", ref columns, 1f, 1, 20)) {
                        config.Columns = columns;
                    }
                    ImGui.Spacing();

                    ImGui.Text("アイコンの間隔(Icon Padding)");
                    var padding = config.Padding;
                    ImGui.SetNextItemWidth(200f);
                    if (ImGui.DragInt("     ", ref padding, 1f, 0, 100)) {
                        config.Padding = padding;
                    }
                    ImGui.Spacing();

                    ImGui.Text("テキストカラー(Text Color)");
                    var textColor = config.Color;
                    ImGui.BeginChild("ragio");
                    {
                        if (ImGui.RadioButton("白(White)", textColor == Constants.White)) {
                            config.Color = Constants.White;
                        }
                        if (ImGui.RadioButton("赤(Red)", textColor == Constants.Red)) {
                            config.Color = Constants.Red;
                        }
                        if (ImGui.RadioButton("緑(Green)", textColor == Constants.Green)) {
                            config.Color = Constants.Green;
                        }
                        if (ImGui.RadioButton("青(Blue)", textColor == Constants.Blue)) {
                            config.Color = Constants.Blue;
                        }
                        if (ImGui.RadioButton("黒(Black)", textColor == Constants.Black)) {
                            config.Color = Constants.Black;
                        }
                    }
                    ImGui.EndChild();

                    config.Font = GameFontFamilyAndSize.Axis36;
                }
                ImGui.End();
                if (!isConfigOpen) {
                    DalamudService.PluginInterface.SavePluginConfig(config);
                }
            }

            if (!config.IsEnabled) return;

            // get instance
            var ownerPlayer = DalamudService.ClientState.LocalPlayer;
            var actionManager = ActionManager.Instance();
            var partyList = DalamudService.PartyList;

            if (DalamudService.Condition[ConditionFlag.InCombat] && !DalamudService.ClientState.IsPvP) {
                // in combat
                try {
                    var ptlist = DalamudService.GameGui.GetAddonByName("_PartyList", 1);
                    if (ptlist != IntPtr.Zero) {
                        var ptlistAtk = (AtkUnitBase*)ptlist;
                        var x = ptlistAtk->X;
                        var y = ptlistAtk->Y;
                        if (ptlistAtk->IsVisible) {
                            DrawOverray(RecastTimers, x, y);
                        }
                    }
                } catch (Exception e) {
                    PluginLog.Error(e.Message + "\n" + e.StackTrace);
                } finally {
                    ImGui.PopStyleColor();
                    ImGui.PopFont();
                }
            } else {
                // not in combat
                bool isLocalPartyListChanged = false;

                if (localPartyList == null || localPartyList.Count() != partyList.Count()) {
                    // Initialize or partymember count change
                    localPartyList = new List<Model.ParryMemberModel>();
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
                    if (ImGui.Begin("Debug Window", ImGuiWindowFlags.AlwaysAutoResize)) {
                        ImGui.Text("[Debug Window 1]");
                        ImGui.Text("");

                        localPartyList.ForEach(p => {
                            ImGui.Text("Index");
                            ImGui.SameLine();
                            ImGui.Text(p.Index.ToString());

                            ImGui.Text("ObjectId");
                            ImGui.SameLine();
                            ImGui.Text(p.ObjectId.ToString());

                            ImGui.Text("JobId");
                            ImGui.SameLine();
                            ImGui.Text(p.JobId.ToString());

                            ImGui.Text("MemberName");
                            ImGui.SameLine();
                            ImGui.Text(p.MemberName);

                            ImGui.Text("");
                        });
                        ImGui.End();
                    }

                    if (ImGui.Begin("Debug Window2", ImGuiWindowFlags.AlwaysAutoResize)) {
                        ImGui.Text("[Debug Window 2]");
                        ImGui.Text("");

                        RecastTimers.ForEach(r => {
                            ImGui.Image(r.Image.ImGuiHandle, r.imageSize);
                            ImGui.SameLine();
                            ImGui.Text("row: " + r.row);
                            ImGui.SameLine();
                            ImGui.Text("col: " + r.col);
                            ImGui.SameLine();
                            ImGui.Text("text offset x: " + r.textOffsetX.ToString());
                            ImGui.SameLine();
                            ImGui.Text("text offset y: " + r.textOffsetY.ToString());
                        });
                        ImGui.End();
                    }

                    if (ImGui.Begin("Debug Window3", ImGuiWindowFlags.AlwaysAutoResize)) {
                        ImGui.Text("[Debug Window 3]");
                        ImGui.Text("");

                        GameObject target = DalamudService.TargetManager.Target;

                        if (target is Dalamud.Game.ClientState.Objects.Types.BattleChara b) {
                            var ss = b.StatusList.Where(s => s.StatusId != 0).ToList();
                            for (int i = 0; i < ss.Count(); i++) {
                                var s = ss[i];

                                ImGui.Text("ObjectId: " + target.ObjectId.ToString());
                                ImGui.Text("StatusId[" + i + "]: " + s.StatusId.ToString());
                                ImGui.Text("SourceId[" + i + "]: " + s.SourceId.ToString());
                            }
                        }

                        ImGui.End();
                    }
                }
            }
        }
        public static AtkUnitBase* GetUnitBase(string name, int index = 1) {
            return (AtkUnitBase*)DalamudService.GameGui.GetAddonByName(name, index);
        }

        private void DrawOverray(List<RecastTimerModel> recastTimers, short x, short y) {
            // --General Setting--
            var localPlayer = DalamudService.ClientState.LocalPlayer;
            var partyList = DalamudService.PartyList;
            var actionManager = ActionManager.Instance();
            var maxRow = Constants.maxRow / config.Columns;
            float imgWidth = Constants.ImageWidth * config.Size / 100f;
            float imgHeight = Constants.ImageHeight * config.Size / 100f;

            // font
            ImGui.PushFont(DalamudService.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(config.Font.Value)).ImFont);
            ImGui.PushStyleColor(ImGuiCol.Text, config.Color);

            // window offset
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(x + config.OffsetX, y + config.OffsetY));

            if (ImGui.Begin("Overray",
                    // ImGuiWindowFlags.NoInputs |
                    // ImGuiWindowFlags.NoMove |
                    // ImGuiWindowFlags.NoBackground | 
                    ImGuiWindowFlags.NoTitleBar)) {

                // font scale
                ImGui.SetWindowFontScale(0.95f * config.Size / 100);

                // Window Size Set
                ImGui.SetWindowSize(new Vector2(config.Columns * (config.Padding + Constants.ImageWidth), maxRow * (config.Padding + imgHeight)));

                for(int i = 0; i < recastTimers.Count(); i++) {
                    var recastTime = recastTimers[i];

                    // init timer
                    if (recastTime.StopWatch == null) {
                        recastTime.StopWatch = new Stopwatch();
                    }

                    // select partymember
                    var partyMember = partyList.Where(p => p.ObjectId == recastTime.OwnerId).FirstOrDefault();

                    // get status
                    var status = getStatus(partyMember, recastTime);
                    
                    // exist status ?
                    if (status != null) { 
                        if (!recastTime.StopWatch.IsRunning) {
                            recastTime.StopWatch.Start();
                        }
                    } else {
                        if(recastTime.StopWatch.IsRunning) {
                            
                        }
                    }

                    var elapsedTime = recastTime.StopWatch.Elapsed.TotalMilliseconds / 1000;
                    var time = recastTime.RecastTime - elapsedTime;
                    var dispTime = time.ToString("#");
                    if (elapsedTime >= recastTime.RecastTime) {
                        recastTime.StopWatch.Stop();
                        recastTime.StopWatch.Reset();
                        dispTime = recastTime.RecastTime.ToString("#");
                    } else {
                        if (dispTime.Length == 2) {
                            dispTime = " " + dispTime + " ";
                        } else if (dispTime.Length == 1) {
                            dispTime = "  " + dispTime + "  ";
                        }
                    }

                    // Draw Image
                    ImGui.SetCursorPos(recastTime.imageOffset);
                    ImGui.Image(recastTime.Image.ImGuiHandle, recastTime.imageSize);

                    // Draw Time
                    ImGui.SetCursorPos(recastTime.imageOffset);
                    ImGui.Text(dispTime);
                }

                ImGui.End();
            }
        }

        private Dalamud.Game.ClientState.Statuses.Status getStatus(PartyMember p, RecastTimerModel r) {
            if (p.ClassJob.Id == (uint)JobIds.NIN || p.ClassJob.Id == (uint)JobIds.SCH) {
                GameObject target = DalamudService.TargetManager.Target;
                if (target is Dalamud.Game.ClientState.Objects.Types.BattleChara b) {
                    var statusList = b.StatusList.Where(s => s.StatusId != 0).ToList();
                    return statusList.Where(s => s.StatusId == r.StatusId).FirstOrDefault();
                }
            } else {
                // get target status
                return p.Statuses.Where(p => p.StatusId == r.StatusId).FirstOrDefault();
            }
            return null;
        }
    }
}

