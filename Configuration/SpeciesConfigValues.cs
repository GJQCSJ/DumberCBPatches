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

        static readonly string[] TargetCoreSpecies = {
            "Elf","Gnome","Human","Imp","Lizard","Rabbit","Sheep","Wolf"
        };

        public readonly Dictionary<string, SpeciesConfigEntries> Values
            = new Dictionary<string, SpeciesConfigEntries>(StringComparer.OrdinalIgnoreCase);

        public SpeciesConfigValues(ConfigFile cfg)
        {
            foreach (var spName in TargetSpecies.Concat(TargetCoreSpecies))
            {
                bool isCore = TargetCoreSpecies.Contains(spName, StringComparer.OrdinalIgnoreCase);

                var dataItem = GameData.SpeciesDataList
                    .FirstOrDefault(s => s.Name.Equals(spName, StringComparison.OrdinalIgnoreCase));

                if (dataItem == null) continue;

                var section = $"Species:{spName}";
                var entry = new SpeciesConfigEntries();
                Values[spName] = entry;

                entry.BaseChance = BindEntry(cfg, section, "BaseChance", dataItem.BaseChance,
                    v => dataItem.BaseChance = v);

                // Bind PossiblyMothers as CSV of strings (string[])
                BindCsv(cfg, section, "PossiblyMothers",
                    dataItem.PossiblyMothers ?? Array.Empty<string>(),
                    arr => dataItem.PossiblyMothers = arr.ToArray());

                BindCsv(cfg, section, "PossiblyFathers",
                    dataItem.PossiblyFathers?.Select(x => x.ToString()),
                    arr => dataItem.PossiblyFathers = arr
                        .Select(s => (ERace)Enum.Parse(typeof(ERace), s))
                        .ToArray());

                BindCsv(cfg, section, "RequiredEssences",
                    dataItem.RequiredEssences?.Select(kv => $"{kv.Key}:{kv.Value}"),
                    arr => dataItem.RequiredEssences = arr
                        .Select(p => p.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(
                            parts => (ETrait)Enum.Parse(typeof(ETrait), parts[0]),
                            parts => int.Parse(parts[1])
                        ));

                BindCsv(cfg, section, "ForbiddenEssences",
                    dataItem.ForbiddenEssences?.Select(x => x.ToString()),
                    arr => dataItem.ForbiddenEssences = arr
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