using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using RaidBuffRecaster.Const;
using RaidBuffRecaster.DataModel;
using RaidBuffRecaster.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.AddonPartyList;
using static RaidBuffRecaster.Model.BuffActionModel;

namespace RaidBuffRecaster.Service {
    internal class MainService {
        internal static void DrawConfigWindow(ref Config config, ref bool isConfigOpen) {
            if (ImGui.Begin("RaidBuffRecaster Config", ref isConfigOpen, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize)) {
                ImGui.SetWindowSize(new Vector2(350, 500));

                var isEnabled = config.IsEnabled;
                if (ImGui.Checkbox("プラグインを有効にする(Enable Plugin)", ref isEnabled)) {
                    config.IsEnabled = isEnabled;
                }

                ImGui.Spacing();

                var isInCombatOnly = config.IsInCombatOnly;
                if (ImGui.Checkbox("戦闘時のみ有効(In Combat Only)", ref isInCombatOnly)) {
                    config.IsInCombatOnly = isInCombatOnly;
                }
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Text("アイコン X座標のオフセット(Image X Offset)");
                var offsetX = config.OffsetX;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##offsetX", ref offsetX, 0.1f)) {
                    config.OffsetX = offsetX;
                }
                ImGui.Spacing();

                ImGui.Text("アイコン  Y座標のオフセット(Image Y Offset)");
                var offsetY = config.OffsetY;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##offsetY", ref offsetY, 0.1f)) {
                    config.OffsetY = offsetY;
                }
                ImGui.Spacing();

                ImGui.Text("アイコンの拡大率(Icon Scale)");
                var size = config.Size;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##Size", ref size, 0.5f, 1, 300)) {
                    config.Size = size;
                }
                ImGui.Spacing();

                ImGui.Text("1列辺りのアイコン数(Icon Columns)");
                var columns = config.Columns;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragInt("##Columns", ref columns, 1f, 1, 20)) {
                    config.Columns = columns;
                }
                ImGui.Spacing();

                ImGui.Text("アイコンの横間隔(Icon Padding)");
                var padding = config.Padding;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##Padding", ref padding, 0.5f, 0, 100)) {
                    config.Padding = padding;
                }
                ImGui.Spacing();

                ImGui.Text("フォントの拡大率(Font Scale)");
                var fontScale = config.FontScale;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##Scale", ref fontScale, 0.5f, 1, 300)) {
                    config.FontScale = fontScale;
                }
                ImGui.Spacing();

                ImGui.Text("フォント X座標のオフセット(Font X Offset)");
                var fontOffsetX = config.FontOffsetX;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##FontOffsetX", ref fontOffsetX, 0.1f)) {
                    config.FontOffsetX = fontOffsetX;
                }
                ImGui.Spacing();

                ImGui.Text("フォント Y座標のオフセット(Font Y Offset)");
                var fontOffsetY = config.FontOffsetY;
                ImGui.SetNextItemWidth(200f);
                if (ImGui.DragFloat("##FontOffsetY", ref fontOffsetY, 0.1f)) {
                    config.FontOffsetY = fontOffsetY;
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                config.Font = GameFontFamilyAndSize.Axis36;

                if (ImGui.Button("閉じる(Close)")) {
                    isConfigOpen = false;
                    DalamudService.PluginInterface.SavePluginConfig(config);
                }
            }
            ImGui.End();
            if (!isConfigOpen) {
                DalamudService.PluginInterface.SavePluginConfig(config);
            }
        }

        internal static unsafe void DrawOverray(List<RecastTimerModel> recastTimers, Config config, IDalamudTextureWrap imageBlackOut) {
            var isBegin = false;

            try {
                var localPlayer = DalamudService.ClientState.LocalPlayer;
                var partyList = DalamudService.PartyList;

                var col = 0;
                var row = 0;

                var maxRow = Constants.maxRow / config.Columns;
                float imgWidth = Constants.ImageWidth * config.Size / 100f;
                float imgHeight = Constants.ImageHeight * config.Size / 100f;
                Vector2? pos = GetPtlistPosition();

                if (pos == null) return;

                // font
                ImGui.PushFont(DalamudService.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(config.Font.Value)).ImFont);
                ImGui.PushStyleColor(ImGuiCol.Text, Constants.White);

                // window offset
                ImGuiHelpers.ForceNextWindowMainViewport();
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Vector2(pos.Value.X + config.OffsetX, pos.Value.Y + config.OffsetY - 100f));

                if (ImGui.Begin("Overray",
                        ImGuiWindowFlags.NoInputs |
                        ImGuiWindowFlags.NoScrollbar |
                        ImGuiWindowFlags.NoBackground |
                        ImGuiWindowFlags.NoTitleBar |
                        ImGuiWindowFlags.AlwaysAutoResize)) {

                    isBegin = true;
                    ImGui.SetWindowFontScale(0.95f * config.FontScale / 100);
                    ImGui.SetWindowSize(new Vector2(Constants.maxColumn * (imgWidth + config.Padding) + config.OffsetX, maxRow * (imgHeight + config.Padding) + config.OffsetY));

                    foreach (var i in Enumerable.Range(0, recastTimers.Count)) {
                        Vector4 outlineColor;
                        var timer = recastTimers[i];

                        var partyMember = partyList.Where(p => p.ObjectId == timer.OwnerId).FirstOrDefault();
                        var status = partyMember != null ? GetStatus(partyMember, timer) : GetStatus(localPlayer, timer);
                        var dispTime = string.Empty;

                        // Draw Image
                        var offsetX = col * (imgWidth + config.Padding) + config.OffsetX;
                        var offsetY = row * imgHeight + config.OffsetY;
                        ImGui.SetCursorPos(new Vector2(offsetX, offsetY));
                        ImGui.Image(timer.Image.ImGuiHandle, new Vector2(imgWidth, imgHeight));

                        // timer
                        if (timer.StopWatch.IsRunning) {
                            if (status != null) {
                                // effect time
                                outlineColor = Constants.Red;
                                dispTime = (timer.RecastTime - (timer.StopWatch.ElapsedMilliseconds / 1000)).ToString("#");
                            } else {
                                // no effect
                                outlineColor = Constants.Black;
                                if (timer.RecastTime <= timer.StopWatch.ElapsedMilliseconds / 1000) {
                                    timer.StopWatch.Stop();
                                    timer.StopWatch.Reset();
                                } else {
                                    // recast time
                                    ImGui.SetCursorPos(new Vector2(offsetX, offsetY));
                                    ImGui.Image(imageBlackOut.ImGuiHandle, new Vector2(imgWidth, imgHeight));
                                    dispTime = (timer.RecastTime - (timer.StopWatch.ElapsedMilliseconds / 1000)).ToString("#");
                                }
                            }
                        } else {
                            if (status == null) {
                                // delay
                                outlineColor = Constants.Black;
                            } else {
                                // cast start
                                outlineColor = Constants.Red;
                                timer.StopWatch.Start();
                                dispTime = (timer.RecastTime - (timer.StopWatch.ElapsedMilliseconds / 1000)).ToString("#");
                            }
                        }

                        // Draw Time
                        if (dispTime != string.Empty) {
                            // a len text width
                            var aLenTextWidth = ((ImGui.CalcTextSize(dispTime).X / dispTime.Length) * (3 - dispTime.Length) / 2);

                            offsetX = (col * (imgWidth + config.Padding)) + aLenTextWidth + config.FontOffsetX;
                            offsetY = row * imgHeight + config.FontOffsetY;

                            ImGui.PushStyleColor(ImGuiCol.Text, outlineColor);
                            ImGui.SetCursorPos(new Vector2(offsetX + 1.5f, offsetY));
                            ImGui.Text(dispTime);

                            ImGui.SetCursorPos(new Vector2(offsetX - 1.5f, offsetY));
                            ImGui.Text(dispTime);

                            ImGui.SetCursorPos(new Vector2(offsetX, offsetY + 1.5f));
                            ImGui.Text(dispTime);

                            ImGui.SetCursorPos(new Vector2(offsetX, offsetY - 1.5f));
                            ImGui.Text(dispTime);

                            ImGui.PopStyleColor();

                            ImGui.SetCursorPos(new Vector2(offsetX, offsetY));
                            ImGui.Text(dispTime);
                        }

                        col++;
                        if (col == config.Columns) {
                            col = 0;
                            row++;
                        }
                    }

                    // AST CARD
                    var cardStatuses = localPlayer.StatusList.Where(s => s.StatusId >= 4401 && s.StatusId <= 4406).FirstOrDefault();
                    if (cardStatuses != null) {
                        // get timer
                        var timer = recastTimers.Where(timer => timer.StatusId == cardStatuses.StatusId).FirstOrDefault();

                        // Draw Image
                        var offsetX = col * (imgWidth + config.Padding) + config.OffsetX;
                        var offsetY = row * imgHeight + config.OffsetY;
                        ImGui.SetCursorPos(new Vector2(offsetX, offsetY));
                        ImGui.Image(timer.Image.ImGuiHandle, new Vector2(imgWidth, imgHeight));

                        // Draw Time
                        Vector4 outlineColor = Constants.Red;
                        var dispTime = (timer.RecastTime - cardStatuses.RemainingTime).ToString();

                        // a len text width
                        var aLenTextWidth = ((ImGui.CalcTextSize(dispTime).X / dispTime.Length) * (3 - dispTime.Length) / 2);

                        offsetX = (col * (imgWidth + config.Padding)) + aLenTextWidth + config.FontOffsetX;
                        offsetY = row * imgHeight + config.FontOffsetY;

                        ImGui.PushStyleColor(ImGuiCol.Text, outlineColor);
                        ImGui.SetCursorPos(new Vector2(offsetX + 1.5f, offsetY));
                        ImGui.Text(dispTime);

                        ImGui.SetCursorPos(new Vector2(offsetX - 1.5f, offsetY));
                        ImGui.Text(dispTime);

                        ImGui.SetCursorPos(new Vector2(offsetX, offsetY + 1.5f));
                        ImGui.Text(dispTime);

                        ImGui.SetCursorPos(new Vector2(offsetX, offsetY - 1.5f));
                        ImGui.Text(dispTime);

                        ImGui.PopStyleColor();

                        ImGui.SetCursorPos(new Vector2(offsetX, offsetY));
                        ImGui.Text(dispTime);
                    }

                    ImGui.End();
                }
            } catch (Exception e) {
                PluginLog.Error(e.Message + "\n" + e.StackTrace);
            } finally {
                if (isBegin) {
                    ImGui.PopFont();
                    ImGui.End();
                }
            }
        }

        public static Dalamud.Game.ClientState.Statuses.Status GetStatus(Dalamud.Game.ClientState.Party.PartyMember p, RecastTimerModel r) {
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

        public static Dalamud.Game.ClientState.Statuses.Status GetStatus(PlayerCharacter p, RecastTimerModel r) {
            if (p.ClassJob.Id == (uint)JobIds.NIN || p.ClassJob.Id == (uint)JobIds.SCH) {
                GameObject target = DalamudService.TargetManager.Target;
                if (target is Dalamud.Game.ClientState.Objects.Types.BattleChara b) {
                    var statusList = b.StatusList.Where(s => s.StatusId != 0).ToList();
                    return statusList.Where(s => s.StatusId == r.StatusId).FirstOrDefault();
                }
            } else {
                // get target status
                return p.StatusList.Where(p => p.StatusId == r.StatusId).FirstOrDefault();
            }
            return null;
        }

        internal unsafe static void UpdateRecastTimers(ref Config config, ref List<RecastTimerModel> recastTimers, List<BuffAction> buffActions) {
            var localPlayer = DalamudService.ClientState.LocalPlayer;
            var partyList = DalamudService.PartyList;
            recastTimers = new List<RecastTimerModel>();

            if (partyList.Count == 0) {
                // solo
                var buff = buffActions.Where(b => b.JobId == localPlayer.ClassJob.Id).ToList();
                foreach (var j in Enumerable.Range(0, buff.Count)) {
                    recastTimers.Add(RecastTimerService.AddRecastTimer(localPlayer, buff[j], config));
                }
            } else {
                // party
                for (int i = 0; i < partyList.Count(); i++) {
                    var partyMember = partyList[i];
                    var buff = buffActions.Where(b => b.JobId == partyMember.ClassJob.Id).ToList();
                    foreach (var j in Enumerable.Range(0, buff.Count)) {
                        recastTimers.Add(RecastTimerService.AddRecastTimer(partyMember, buff[j], config));
                    }
                }
            }
        }
        internal unsafe static Vector2? GetPtlistPosition() {
            var ptlist = DalamudService.GameGui.GetAddonByName("_PartyList", 1);
            if (ptlist != IntPtr.Zero) {
                var ptlistAtk = (AtkUnitBase*)ptlist;
                var x = ptlistAtk->X;
                var y = ptlistAtk->Y;
                if (ptlistAtk->IsVisible) {
                    return new Vector2(x, y);
                }
            }
            return null;
        }
    }
}
