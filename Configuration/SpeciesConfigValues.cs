// SpeciesConfigValues.cs
using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using ComplexBreeding.Essences;
using ComplexBreeding;
using UnityEngine;
using MBMScripts;

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

        public readonly Dictionary<string, SpeciesConfigEntries> Values
            = new Dictionary<string, SpeciesConfigEntries>(StringComparer.OrdinalIgnoreCase);

        public SpeciesConfigValues(ConfigFile cfg)
        {
            foreach (var spName in TargetSpecies)
            {
                var spData = GameData.SpeciesDataList
                    .FirstOrDefault(s => s.Name.Equals(spName, StringComparison.OrdinalIgnoreCase));
                if (spData == null) continue;

                var section = $"Species:{spName}";
                var entry = new SpeciesConfigEntries();
                Values[spName] = entry;

                entry.BaseChance = BindEntry(cfg, section, "BaseChance", spData.BaseChance,
                    v => spData.BaseChance = v);

                // Bind PossiblyMothers as CSV of strings (string[])
                BindCsv(cfg, section, "PossiblyMothers",
                    spData.PossiblyMothers ?? Array.Empty<string>(),
                    arr => spData.PossiblyMothers = arr.ToArray());

                BindCsv(cfg, section, "PossiblyFathers",
                    spData.PossiblyFathers?.Select(x => x.ToString()),
                    arr => spData.PossiblyFathers = arr
                        .Select(s => (ERace)Enum.Parse(typeof(ERace), s))
                        .ToArray());

                BindCsv(cfg, section, "RequiredEssences",
                    spData.RequiredEssences?.Select(kv => $"{kv.Key}:{kv.Value}"),
                    arr => spData.RequiredEssences = arr
                        .Select(p => p.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(
                            parts => (ETrait)Enum.Parse(typeof(ETrait), parts[0]),
                            parts => int.Parse(parts[1])
                        ));

                BindCsv(cfg, section, "ForbiddenEssences",
                    spData.ForbiddenEssences?.Select(x => x.ToString()),
                    arr => spData.ForbiddenEssences = arr
                        .Select(s => (ETrait)Enum.Parse(typeof(ETrait), s))
                        .ToList());
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

        private void BindCsv(ConfigFile cfg, string section, string key,
            IEnumerable<string> defaultItems,
            Action<IEnumerable<string>> apply)
        {
            var defaultCsv = defaultItems != null
                ? string.Join(",", defaultItems)
                : string.Empty;
            var entry = cfg.Bind(section, key, defaultCsv,
                new ConfigDescription(key));
            entry.SettingChanged += (_, __) =>
                apply(entry.Value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            apply(entry.Value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    public class SpeciesConfigEntries
    {
        public ConfigEntry<int> BaseChance { get; set; }
        public ConfigEntry<string> PossiblyMothers { get; set; }
        public ConfigEntry<string> PossiblyFathers { get; set; }
        public ConfigEntry<string> RequiredEssences { get; set; }
        public ConfigEntry<string> ForbiddenEssences { get; set; }
    }
}