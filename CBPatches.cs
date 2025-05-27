using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Unity.Mono;
using DumberCBPatches.Configuration;
using MBMScripts;

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

        // various per-area config holders
        public static ProfessionConfigValues ProfessionConfig { get; private set; }
        public static SpeciesConfigValues SpeciesConfig { get; private set; }
        public static CharacterConfigValues CharacterConfig { get; private set; }
        public static PlayerConfigValues PlayerConfig { get; private set; }
        public static ProfessionRarityConfigValues ProfessionRarityConfig { get; private set; }
        public static Dictionary<ETrait, int> TraitPriceModifiers = new();


        // toggle for whether to apply all patches
        private static ConfigEntry<bool> EnablePatches;
        public static ConfigEntry<bool> EnableRarityPatch;

        private void Awake()
        {
            // assign our logger so everyone can call CBPatches.Log
            Log = Logger;

            // bind  "enable/disable" toggle
            EnablePatches = Config.Bind(
                "General",
                "Enable Patches",
                true,
                "Essence list:\r\ndemonic - 93\r\nelemental - 95\r\neternal - 96\r\nferal - 98\r\nmagical - 97\r\nsacred - 94"
            );

            EnableRarityPatch = Config.Bind("ProfessionRarity", "Enable Custom Rarity Patch", true, "Do you want to enable custom profession rarity logic?");

            // instantiate config-holders
            ProfessionConfig = new ProfessionConfigValues(Config);
            SpeciesConfig = new SpeciesConfigValues(Config);
            CharacterConfig = new CharacterConfigValues(Config);
            PlayerConfig = new PlayerConfigValues(Config);
            ProfessionRarityConfig = new ProfessionRarityConfigValues(Config);

            var traitPriceSection = "TraitPriceAdjustments";

            foreach (ETrait trait in Enum.GetValues(typeof(ETrait)))
            {
                string key = trait.ToString();
                var entry = Config.Bind(traitPriceSection, key, 0, $"Price adjustment for trait {key}");
                TraitPriceModifiers[trait] = entry.Value;
            }


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
