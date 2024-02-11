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
                if (isConfigOpen) { // config
                    if (ImGui.Begin("RaidBuffRecaster Config", ref isConfigOpen, ImGuiWindowFlags.NoResize)) {
                        ImGui.SetWindowSize(new Vector2(350, 500));

                        var isEnabled = config.IsEnabled;
                        if (ImGui.Checkbox("プラグインを有効にする(Enable Plugin)", ref isEnabled)) {
                            config.IsEnabled = isEnabled;
                        }

                        ImGui.Spacing();

                        var isPreview = config.IsPreview;
                        if (ImGui.Checkbox("プレビュー(Preview)", ref isPreview)) {
                            config.IsPreview = isPreview;
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
                        ImGui.Separator();
                        ImGui.Spacing();

                        /*
                        var isDebug = config.IsDebug;
                        if (ImGui.Checkbox("デバッグ情報(Debug Window)", ref isDebug)) {
                            config.IsDebug = isDebug;
                        }
                        */
                        config.Font = GameFontFamilyAndSize.Axis36;
                    }
                    ImGui.End();
                    if (!isConfigOpen) {
                        DalamudService.PluginInterface.SavePluginConfig(config);
                    }
                }

                if (!config.IsEnabled) return;

                if (DalamudService.ClientState.LocalPlayer == null) return;

                // get instance
                var ownerPlayer = DalamudService.ClientState.LocalPlayer;
                var actionManager = ActionManager.Instance();
                var partyList = DalamudService.PartyList;

                if (DalamudService.Condition[ConditionFlag.InCombat] && !DalamudService.ClientState.IsPvP) {
                    // in combat
                    var pos = MainService.GetPtlistPosition();
                    if (pos != null)
                        DrawOverray(RecastTimers, (Vector2)pos);
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

                    /*
                    if (config.IsDebug) {
                        DrawDebugWindow();
                    }
                    */

                    if (config.IsPreview) {
                        var pos = MainService.GetPtlistPosition();
                        if (pos != null)
                            DrawOverrayPreview((Vector2)pos);
                    }
                }
            } catch (Exception e) {
                PluginLog.Error(e.Message + "\n" + e.StackTrace);
            } finally {
            }
        }

        private void DrawOverrayPreview(Vector2 pos) {
            var maxRow = Constants.maxRow / config.Columns;
            float imgWidth = Constants.ImageWidth * config.Size / 100f;
            float imgHeight = Constants.ImageHeight * config.Size / 100f;

            // font
            ImGui.PushFont(DalamudService.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(config.Font.Value)).ImFont);
            ImGui.PushStyleColor(ImGuiCol.Text, Constants.White);

            // window offset
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(pos.X + config.OffsetX, pos.Y + config.OffsetY - 70));

            if (ImGui.Begin("Overray",
                    ImGuiWindowFlags.NoInputs |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoBackground |
                    ImGuiWindowFlags.NoTitleBar)) {

                ImGui.SetWindowSize(new Vector2(config.Columns * (config.Padding + Constants.ImageWidth), maxRow * (config.Padding + imgHeight)));
                ImGui.SetWindowFontScale(0.95f * config.Size / 100);

                var col = 0;
                var row = 0;
                var localPlayer = DalamudService.ClientState.LocalPlayer;
                var jobId = localPlayer.ClassJob.Id;
                var buffs = BuffActions;

                for (int i = 0; i < buffs.Count; i++) {
                    var textOffsetX = col * (imgWidth + config.Padding);
                    var textOffsetY = (imgHeight * row) + (imgHeight / 5);

                    // Draw Image
                    ImGui.SetCursorPos(new Vector2(col * imgWidth, row * imgHeight));
                    ImGui.Image(buffs[i].Image.ImGuiHandle, new Vector2(Constants.ImageWidth * config.Size / 100, Constants.ImageHeight * config.Size / 100));

                    // Draw Time
                    ImGui.PushStyleColor(ImGuiCol.Text, Const.Constants.Black);
                    ImGui.SetCursorPos(new Vector2(textOffsetX + 1.5f, textOffsetY));
                    ImGui.Text(buffs[i].RecastTime.ToString("#"));

                    ImGui.SetCursorPos(new Vector2(textOffsetX - 1.5f, textOffsetY));
                    ImGui.Text(buffs[i].RecastTime.ToString("#"));

                    ImGui.SetCursorPos(new Vector2(textOffsetX, textOffsetY + 1.5f));
                    ImGui.Text(buffs[i].RecastTime.ToString("#"));

                    ImGui.SetCursorPos(new Vector2(textOffsetX, textOffsetY - 1.5f));
                    ImGui.Text(buffs[i].RecastTime.ToString("#"));

                    ImGui.PopStyleColor();

                    ImGui.SetCursorPos(new Vector2(textOffsetX, textOffsetY));
                    ImGui.Text(buffs[i].RecastTime.ToString("#"));

                    col++;
                    if (col == config.Columns) {
                        col = 0;
                        row++;
                    }
                }
            }
            ImGui.PopFont();
            ImGui.End();
        }

        private void DrawDebugWindow() {

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
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
        }

        private void DrawOverray(List<RecastTimerModel> recastTimers, Vector2 pos) {
            // --General Setting--
            var localPlayer = DalamudService.ClientState.LocalPlayer;
            var partyList = DalamudService.PartyList;
            var actionManager = ActionManager.Instance();
            var maxRow = Constants.maxRow / config.Columns;
            float imgWidth = Constants.ImageWidth * config.Size / 100f;
            float imgHeight = Constants.ImageHeight * config.Size / 100f;

            try {
                // font
                ImGui.PushFont(DalamudService.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(config.Font.Value)).ImFont);
                ImGui.PushStyleColor(ImGuiCol.Text, Constants.White);

                // window offset
                ImGuiHelpers.ForceNextWindowMainViewport();
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(pos.X + config.OffsetX, pos.Y + config.OffsetY - 70));

                if (ImGui.Begin("Overray",
                        ImGuiWindowFlags.NoInputs |
                        ImGuiWindowFlags.NoMove |
                        ImGuiWindowFlags.NoScrollbar |
                        ImGuiWindowFlags.NoBackground |
                        ImGuiWindowFlags.NoTitleBar)) {

                    ImGui.SetWindowSize(new Vector2(config.Columns * (config.Padding + Constants.ImageWidth), maxRow * (config.Padding + imgHeight)));
                    ImGui.SetWindowFontScale(0.95f * config.Size / 100);

                    for (int i = 0; i < recastTimers.Count(); i++) {
                        var recastTime = recastTimers[i];

                        // init timer
                        if (recastTime.StopWatch == null) {
                            recastTime.StopWatch = new Stopwatch();
                        }

                        // select partymember
                        var partyMember = partyList.Where(p => p.ObjectId == recastTime.OwnerId).FirstOrDefault();

                        // get status
                        var status = MainService.GetStatus(partyMember, recastTime);

                        // exist status ?
                        Vector4 outlineColor;
                        if (status != null) {
                            if (!recastTime.StopWatch.IsRunning) {
                                recastTime.StopWatch.Start();
                            }
                            outlineColor = Const.Constants.Red;
                        } else {
                            outlineColor = Const.Constants.Black;
                            if (recastTime.StopWatch.IsRunning) {

                            }
                        }

                        var elapsedTime = recastTime.StopWatch.Elapsed.TotalMilliseconds / 1000;
                        var time = (int)(recastTime.RecastTime - elapsedTime);
                        var dispTime = time.ToString("#");
                        var textOffsetX = recastTime.imageOffsetX;
                        var textOffsetY = recastTime.textOffsetY;

                        if (elapsedTime >= recastTime.RecastTime) {
                            recastTime.StopWatch.Stop();
                            recastTime.StopWatch.Reset();
                            dispTime = recastTime.RecastTime.ToString("#");
                        } else {
                            if (dispTime.Length == 2) {
                                textOffsetX = textOffsetX + (ImGui.GetFontSize() / 3.6f);
                            } else if (dispTime.Length == 1) {
                                textOffsetX = textOffsetX + (ImGui.GetFontSize() / 1.8f);
                            }
                        }

                        // Draw Image
                        ImGui.SetCursorPos(recastTime.imageOffset);
                        ImGui.Image(recastTime.Image.ImGuiHandle, new Vector2(Constants.ImageWidth * config.Size / 100, Constants.ImageHeight * config.Size / 100));

                        // Draw Time
                        ImGui.PushStyleColor(ImGuiCol.Text, outlineColor);
                        ImGui.SetCursorPos(new Vector2(textOffsetX + 1.5f, textOffsetY));
                        ImGui.Text(dispTime);

                        ImGui.SetCursorPos(new Vector2(textOffsetX - 1.5f, textOffsetY));
                        ImGui.Text(dispTime);

                        ImGui.SetCursorPos(new Vector2(textOffsetX, textOffsetY + 1.5f));
                        ImGui.Text(dispTime);

                        ImGui.SetCursorPos(new Vector2(textOffsetX, textOffsetY - 1.5f));
                        ImGui.Text(dispTime);

                        ImGui.PopStyleColor();

                        ImGui.SetCursorPos(new Vector2(textOffsetX, textOffsetY));
                        ImGui.Text(dispTime);
                    }

                    ImGui.End();
                }
            } catch (Exception e) {
                PluginLog.Error(e.Message + "\n" + e.StackTrace);
            } finally {
                ImGui.PopStyleColor();
                ImGui.PopFont();
            }
        }
    }
}

