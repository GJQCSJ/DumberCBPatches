using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Unity.Mono;
using DumberCBPatches.Configuration;

namespace DumberCBPatches
{
    [BepInPlugin(GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("mbm.complexbreeding", BepInDependency.DependencyFlags.HardDependency)]
    public class CBPatches : BaseUnityPlugin
    {
        public const string
            AUTHOR = "GJQCSJ",
            GUID = "com." + AUTHOR + "." + PluginInfo.PLUGIN_NAME;

        public static ManualLogSource Log { get; private set; }

        // your various per-area config holders
        public static ProfessionConfigValues ProfessionConfig { get; private set; }
        public static SpeciesConfigValues SpeciesConfig { get; private set; }
        public static CharacterConfigValues CharacterConfig { get; private set; }
        public static PlayerConfigValues PlayerConfig { get; private set; }
        public static ProfessionRarityConfigValues ProfessionRarityConfig { get; private set; }

        // toggle for whether to apply all patches
        private static ConfigEntry<bool> EnablePatches;

        private void Awake()
        {
            // assign our logger so everyone can call CBPatches.Log
            Log = Logger;

            // bind your "enable/disable" toggle
            EnablePatches = Config.Bind(
                "General",
                "Enable Patches",
                true,
                "Essence list:\r\ndemonic - 93\r\nelemental - 95\r\neternal - 96\r\nferal - 98\r\nmagical - 97\r\nsacred - 94"
            );

            // instantiate your config-holders
            ProfessionConfig = new ProfessionConfigValues(Config);
            SpeciesConfig = new SpeciesConfigValues(Config);
            CharacterConfig = new CharacterConfigValues(Config);
            PlayerConfig = new PlayerConfigValues(Config);
            ProfessionRarityConfig = new ProfessionRarityConfigValues(Config);

            if (EnablePatches.Value)
            {
                var harmony = new Harmony(GUID);
                harmony.PatchAll();
                Log.LogInfo("[DumberCBPatches] Harmony patches applied.");
            }
            else
            {
                Log.LogInfo("[DumberCBPatches] Patches are disabled in config.");
            }
        }
    }
}
