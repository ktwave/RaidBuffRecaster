using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaidBuffRecaster.Const
{
    public static class Constants
    {
        public const string CommandConfig = "/rbrcfg";
        public const string CommandToggle = "/rbrtgl";
        public const string HelpMessageConfig = "コンフィグ画面を開きます(Open Raid Buff Recaster Config Window)";
        public const string HelpMessageToggle = "オーバーレイの有効/無効を切り替えます(Switch Raid Buff Recaster Enabled/Disabled)";

        public const uint ImageWidth = 76;
        public const uint ImageHeight = 76;
        public const uint maxColumn = 20;
        public const uint maxRow = 20;

        public static Vector4 White { get; set; } = new Vector4(1, 1, 1, 1);
        public static Vector4 Red { get; set; } = new Vector4(1, 0, 0, 1);
        public static Vector4 Green { get; set; } = new Vector4(0, 1, 0, 1);
        public static Vector4 Blue { get; set; } = new Vector4(0, 0, 1, 1);
        public static Vector4 Black { get; set; } = new Vector4(0, 0, 0, 1);
    }
}
