using System;
using System.Collections.Generic;
using System.IO;
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
            Log = Logger;

            // general patch toggle remains in default config file
            EnablePatches = Config.Bind(
                "General",
                "Enable Patches",
                true,
                "Whether to enable all Harmony patches." +
                "\r\nEssence list:\r\ndemonic - 93\r\nelemental - 95\r\neternal - 96\r\nferal - 98\r\nmagical - 97\r\nsacred - 94"
            );

            // Define config root and instantiate separate config files
            string configRoot = Paths.ConfigPath;

            var speciesCfg = new ConfigFile(Path.Combine(configRoot, "DumberCBPatches.Species.cfg"), true);
            var characterCfg = new ConfigFile(Path.Combine(configRoot, "DumberCBPatches.Characters.cfg"), true);
            var professionCfg = new ConfigFile(Path.Combine(configRoot, "DumberCBPatches.Professions.cfg"), true);
            var playerCfg = new ConfigFile(Path.Combine(configRoot, "DumberCBPatches.Player.cfg"), true);
            var rarityCfg = new ConfigFile(Path.Combine(configRoot, "DumberCBPatches.Rarity.cfg"), true);

            // Bind profession rarity toggle in its own file
            EnableRarityPatch = rarityCfg.Bind("ProfessionRarity", "Enable Custom Rarity Patch", true, "Do you want to enable custom profession rarity logic?");

            // Create per-module configuration instances
            ProfessionConfig = new ProfessionConfigValues(professionCfg);
            SpeciesConfig = new SpeciesConfigValues(speciesCfg);
            CharacterConfig = new CharacterConfigValues(characterCfg);
            PlayerConfig = new PlayerConfigValues(playerCfg);
            ProfessionRarityConfig = new ProfessionRarityConfigValues(rarityCfg);

            // Trait-based pricing (keep in default config file)
            string traitPriceSection = "TraitPriceAdjustments";
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
