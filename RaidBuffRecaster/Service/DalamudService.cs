using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;

namespace RaidBuffRecaster.Service
{
    class DalamudService
    {
        [PluginService] static internal DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] static internal IClientState ClientState { get; private set; }
        [PluginService] static internal ICondition Condition { get; private set; }
        [PluginService] public static IChatGui ChatGui { get; private set; }
        [PluginService] static internal IFramework Framework { get; private set; }
        [PluginService] static internal IGameGui GameGui { get; private set; }
        [PluginService] static internal IPartyList PartyList { get; private set; }
        [PluginService] public static ICommandManager CommandManager { get; private set; }
        [PluginService] public static IObjectTable Objects { get; private set; }
        [PluginService] public static ISigScanner SigScanner { get; private set; }
        [PluginService] public static IDataManager DataManager { get; private set; }
        [PluginService] public static ITextureProvider TextureProvider { get; private set; }
        [PluginService] public static ITargetManager TargetManager { get; private set; }
        [PluginService] public static IJobGauges JobGauges { get; private set; }
        [PluginService] public static IPluginLog PluginLog { get; private set; }
        [PluginService] public static IGameInteropProvider Hooks { get; private set; }
        public static void Error(Exception e, string message) => PluginLog.Error(e, message);

        public static void Error(string message) => PluginLog.Error(message);

        public static void Log(string messages) => PluginLog.Info(messages);

        // UserMethod

    }
}
