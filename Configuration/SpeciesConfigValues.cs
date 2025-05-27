// SpeciesConfigValues.cs
using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using ComplexBreeding.Essences;
using ComplexBreeding;
using UnityEngine;
using MBMScripts;
using ComplexBreeding.SpeciesCore;
using ComplexBreeding.Species;
using ComplexBreeding.SpeciesCore.Data;
using System.Runtime.InteropServices.ComTypes;
using HarmonyLib;
using static System.Collections.Specialized.BitVector32;

namespace DumberCBPatches.Configuration
{
    public class SpeciesConfigValues
    {
        static readonly string[] TargetSpecies = {
            "Angel","Devil","Dracolich","Dragon","Drake","Drow","Fairy",
            "Ghoul","Goblin","Golem","Hobgoblin","Horse","Kitsune",
            "Mermaid","Minotaur","Nekomata","Orc","Redcap","Salamander",
            "Slime","Succubus","Vampire","Wererabbit","Werewolf"
        };

        static readonly string[] TargetCoreSpecies = {
            "Elf","Gnome","Human","Imp","Lizard","Rabbit","Sheep","Wolf"
        };

        public Dictionary<string, SpeciesConfigEntries> Values { get; } = new();

        // Now constructor accepts ConfigFile for both Species and CoreSpecies type
        public SpeciesConfigValues(ConfigFile cfg)
        {
            foreach (var spName in TargetSpecies)
            {
                var dataItem = GameData.SpeciesDataList.FirstOrDefault(s => s.Name.Equals(spName, StringComparison.OrdinalIgnoreCase));
                if (dataItem == null) continue;

                var section = $"Species:{spName}";
                var entry = new SpeciesConfigEntries();
                Values[spName] = entry;

                entry.BaseChance = BindEntry(cfg, section, "BaseChance", dataItem.BaseChance, v => dataItem.BaseChance = v);

                entry.TitsType = BindString(cfg, section, "TitsType",
                    dataItem.TitsType?.ToString(),
                    str => dataItem.TitsType = int.TryParse(str, out var n) ? n : null);

                BindCsv(cfg, section, "PossiblyMothers",
                    dataItem.PossiblyMothers ?? Array.Empty<string>(),
                    arr => dataItem.PossiblyMothers = arr.ToArray());

                BindCsv(cfg, section, "PossiblyFathers",
                    dataItem.PossiblyFathers?.Select(x => x.ToString()) ?? Enumerable.Empty<string>(),
                    arr => dataItem.PossiblyFathers = arr.Select(s => Enum.TryParse<ERace>(s, out var val) ? val : ERace.None).ToArray());

                BindCsv(cfg, section, "RequiredEssences",
                    dataItem.RequiredEssences?.Select(kv => $"{kv.Key}:{kv.Value}") ?? Enumerable.Empty<string>(),
                    arr => dataItem.RequiredEssences = arr
                        .Select(p => p.Split(':'))
                        .Where(parts => parts.Length == 2 && Enum.TryParse<ETrait>(parts[0], out _))
                        .ToDictionary(
                            parts => (ETrait)Enum.Parse(typeof(ETrait), parts[0]),
                            parts => int.TryParse(parts[1], out var val) ? val : 0));

                BindCsv(cfg, section, "ForbiddenEssences",
                    dataItem.ForbiddenEssences?.Select(x => x.ToString()) ?? Enumerable.Empty<string>(),
                    arr => dataItem.ForbiddenEssences = arr.Select(s => Enum.TryParse<ETrait>(s, out var val) ? val : ETrait.None).ToList());

                BindSpeciesStats(cfg, section, entry, dataItem);
            }

            foreach (var spName in TargetCoreSpecies)
            {
                var coreItem = GameData.SpeciesCoreDataList.FirstOrDefault(s => s.Name.Equals(spName, StringComparison.OrdinalIgnoreCase));
                if (coreItem == null) continue;

                var section = $"Species:{spName}";
                var entry = new SpeciesConfigEntries();
                Values[spName] = entry;

                entry.TitsTypes = BindCsv(cfg, section, "TitsTypes",
                    coreItem.TitsTypes?.Select(x => x.ToString()) ?? Enumerable.Empty<string>(),
                    arr => coreItem.TitsTypes = arr.Select(s => int.TryParse(s, out var n) ? n : 0).ToArray());

                BindCsv(cfg, section, "GuaranteedTraits",
                    coreItem.GuaranteedTraits?.Select(x => x.ToString()) ?? Enumerable.Empty<string>(),
                    arr => coreItem.GuaranteedTraits = arr.Select(s => Enum.TryParse<ETrait>(s, out var val) ? val : ETrait.None).ToList());

                BindSpeciesStats(cfg, section, entry, coreItem);
            }
        }

        private ConfigEntry<T> BindEntry<T>(ConfigFile cfg, string section, string key,
            T defaultValue, Action<T> apply)
        {
            var entry = cfg.Bind(section, key, defaultValue, new ConfigDescription(key));
            entry.SettingChanged += (_, __) => apply(entry.Value);
            apply(entry.Value);
            return entry;
        }

        private void BindSpeciesStats(ConfigFile cfg, string section, SpeciesConfigEntries entry, ISpeciesStats stats)
        {
            entry.Maintenance = BindEntry(cfg, section, "Maintenance", stats.Maintenance, v => stats.Maintenance = v);
            entry.HealthRegeneration = BindEntry(cfg, section, "HealthRegeneration", stats.HealthRegeneration, v => stats.HealthRegeneration = v);
            entry.MaxHealth = BindEntry(cfg, section, "MaxHealth", stats.MaxHealth, v => stats.MaxHealth = v);
            entry.GrowthTime = BindEntry(cfg, section, "GrowthTime", stats.GrowthTime, v => stats.GrowthTime = v);
            entry.MilkQuality = BindEntry(cfg, section, "MilkQuality", stats.MilkQuality, v => stats.MilkQuality = v);
            entry.MilkProductionRate = BindEntry(cfg, section, "MilkProductionRate", stats.MilkProductionRate, v => stats.MilkProductionRate = v);
            entry.MaxMilkSizeChange = BindEntry(cfg, section, "MaxMilkSizeChange", stats.MaxMilkSizeChange, v => stats.MaxMilkSizeChange = v);
            entry.ConceptionRate = BindEntry(cfg, section, "ConceptionRate", stats.ConceptionRate, v => stats.ConceptionRate = v);
            entry.MaxBirthCount = BindEntry(cfg, section, "MaxBirthCount", stats.MaxBirthCount, v => stats.MaxBirthCount = v);
            entry.Figure = BindEnum(cfg, section, "Figure", stats.Figure, v => stats.Figure = v);
        }

        private ConfigEntry<string> BindEnum<T>(ConfigFile cfg, string section, string key, T defaultValue, Action<T> apply) where T : struct, Enum
        {
            var entry = cfg.Bind(section, key, defaultValue.ToString());
            entry.SettingChanged += (_, __) =>
            {
                if (Enum.TryParse<T>(entry.Value, out var parsed))
                {
                    apply(parsed);
                }
                else Debug.LogWarning($"[DumberCBPatches] Invalid enum value {entry.Value} for {key}");
            };
            if (Enum.TryParse<T>(entry.Value, out var initial)) apply(initial);
            return entry;
        }

        private ConfigEntry<string> BindString(ConfigFile cfg, string section, string key, string defaultValue, Action<string> apply)
        {
            var entry = cfg.Bind(section, key, defaultValue ?? "");
            entry.SettingChanged += (_, __) => apply(entry.Value);
            apply(entry.Value);
            return entry;
        }


        private ConfigEntry<string> BindCsv(ConfigFile cfg, string section, string key, IEnumerable<string> defaultItems, Action<IEnumerable<string>> apply)
        {
            var defaultCsv = string.Join(",", defaultItems ?? Array.Empty<string>());
            var entry = cfg.Bind(
                section: section,
                key: key,
                defaultValue: defaultCsv,
                configDescription: new ConfigDescription($"Comma-separated string list. Default: {defaultCsv}")
            );

            void ParseAndApply(string value)
            {
                if (value == null)
                {
                    apply(Array.Empty<string>());
                    return;
                }

                var parsed = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());
                apply(parsed);
            }

            entry.SettingChanged += (_, __) => ParseAndApply(entry.Value);
            ParseAndApply(entry.Value);

            return entry;
        }

    }

    public class SpeciesConfigEntries
    {
        public ConfigEntry<int> BaseChance { get; set; }
        public ConfigEntry<string> PossiblyMothers { get; set; }
        public ConfigEntry<string> PossiblyFathers { get; set; }
        public ConfigEntry<string>? RequiredEssences { get; set; }
        public ConfigEntry<string>? ForbiddenEssences { get; set; }

        public ConfigEntry<int>? MaxHealth { get; set; }
        public ConfigEntry<string>? TitsTypes { get; set; }
        public ConfigEntry<string> TitsType { get; set; }
        public ConfigEntry<string>? GuaranteedTraits { get; set; }
        
        public ConfigEntry<int> Maintenance { get; set; }
        public ConfigEntry<float> HealthRegeneration { get; set; }
        public ConfigEntry<int> GrowthTime { get; set; }
        public ConfigEntry<int> MilkQuality { get; set; }
        public ConfigEntry<float> MilkProductionRate { get; set; }
        public ConfigEntry<float> MaxMilkSizeChange { get; set; }
        public ConfigEntry<int> ConceptionRate { get; set; }
        public ConfigEntry<int> MaxBirthCount { get; set; }
        public ConfigEntry<string> Figure { get; set; }
    }
}