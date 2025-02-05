﻿namespace BossMod
{
    // [ConfigDisplay(Name = "[调试用的]General settings", Order = 0)]
    [ConfigDisplay(Name = "本插件免费[20230131_6.20A]", Order = 0)]
    public class GeneralConfig : ConfigNode
    {
        [PropertyDisplay("Dump world state events")]
        public bool DumpWorldStateEvents = false;

        [PropertyDisplay("Dump server packets")]
        public bool DumpServerPackets = false;

        [PropertyDisplay("Dump client packets")]
        public bool DumpClientPackets = false;
    }
}
