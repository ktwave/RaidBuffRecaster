using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Utility;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using RaidBuffRecaster.Const;
using RaidBuffRecaster.DataModel;
using RaidBuffRecaster.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static RaidBuffRecaster.Model.BuffActionModel;

namespace RaidBuffRecaster.Service {
    internal class MainService {
        public static Dalamud.Game.ClientState.Statuses.Status GetStatus(PartyMember p, RecastTimerModel r) {
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

        internal static void DrawConfigWindow(ref Config config, ref bool isConfigOpen) {
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

                config.Font = GameFontFamilyAndSize.Axis36;
            }
            ImGui.End();
            if (!isConfigOpen) {
                DalamudService.PluginInterface.SavePluginConfig(config);
            }
        }

        internal static unsafe void DrawOverray(List<RecastTimerModel> recastTimers, Vector2 pos, Config config) {
            // --General Setting--
            var localPlayer = DalamudService.ClientState.LocalPlayer;
            var partyList = DalamudService.PartyList;
            var actionManager = ActionManager.Instance();
            var col = 0;
            var row = 0;
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
                        var textOffsetX = col * (imgWidth + config.Padding);
                        var textOffsetY = (imgHeight * row) + (imgHeight / 5);

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
                        ImGui.SetCursorPos(new Vector2(col * (imgWidth + config.Padding), row * imgHeight));
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

                        col++;
                        if (col == config.Columns) {
                            col = 0;
                            row++;
                        }
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

        internal static void DrawOverrayPreview(Vector2 pos, List<BuffAction> BuffActions, Config config) {
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
                    ImGui.SetCursorPos(new Vector2(col * (imgWidth + config.Padding), row * imgHeight));
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
