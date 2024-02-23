﻿using Dalamud.Configuration;
using Dalamud.Interface.GameFonts;
using Dalamud.Plugin;
using RaidBuffRecaster.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster
{
    [Serializable]
    class Config : IPluginConfiguration {
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public int Version { get; set; } = 0;
        public float OffsetX { get; set; } = 0;
        public float OffsetY { get; set; } = 0;
        public float Size { get; set; } = 100;
        public int Columns { get; set; } = 10;
        public float Padding { get; set; } = 0;
        public bool IsPreview { get; set; } = false;
        public bool IsEnabled { get; set; } = false;
        public float FontScale { get; set; } = 100;
        public float FontOffsetX { get; set; } = 0;
        public float FontOffsetY { get; set; } = 0;
        public bool IsInCombatOnly { get; set; } = false;

        public GameFontFamilyAndSize? Font = null;

        public void Save() {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
