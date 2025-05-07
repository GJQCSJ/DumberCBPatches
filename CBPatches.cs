using BepInEx;
using BepInEx.Configuration;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using HarmonyLib.Tools;
using System;
using DumberCBPatches.Configuration;
using ComplexBreeding.Patches;

namespace DumberCBPatches
{
    [BepInPlugin(GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("mbm.complexbreeding", BepInDependency.DependencyFlags.HardDependency)]
    public class CBPatches : BaseUnityPlugin
    {
        public const string
        AUTHOR = "GJQCSJ",
        GUID = "com." + AUTHOR + "." + PluginInfo.PLUGIN_NAME;
        private static ConfigEntry<bool>? Enabled;
        public static ManualLogSource? Log { get; set; }

        public static ProfessionConfigValues ProfessionConfig { get; private set; }
        public static SpeciesConfigValues SpeciesConfig { get; private set; }

        public static CharacterConfigValues CharacterConfig;
        public static PlayerConfigValues PlayerConfig;

        public static ProfessionRarityConfigValues ProfessionRarityConfig;



        /// <summary>
        /// Patch and start plugin.
        /// </summary>
        private void Awake()
        {
            Enabled = Config.Bind("General", "Enable Patches", true, "Essence list:\r\ndemonic - 93\r\nelemental - 95\r\neternal - 96\r\nferal - 98\r\nmagical - 97\r\nsacred - 94");
     
            ProfessionConfig = new ProfessionConfigValues(Config);
            SpeciesConfig = new SpeciesConfigValues(Config);
            PlayerConfig = new PlayerConfigValues(Config);
            CharacterConfig = new CharacterConfigValues(Config);
            ProfessionRarityConfig = new ProfessionRarityConfigValues(Config);

            if (Enabled.Value)
            {
                var harmony = new Harmony(GUID);
                harmony.PatchAll();
                Logger.LogInfo("[DumberCBPatches] Harmony patches applied.");
            }
        }


    }
}