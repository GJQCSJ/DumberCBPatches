// ProfessionConfigValues.cs
using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using ComplexBreeding.Professions.Data;
using MBMScripts;
using UnityEngine;
using ComplexBreeding.Professions;
using ComplexBreeding;

namespace DumberCBPatches.Configuration
{
    public class ProfessionConfigValues
    {
        static readonly string[] TargetProfessions = {
            "Artificer","Blacksmith","Cultist","Farmhand","Hunter",
            "Knight","Maid","Paladin","Princess","Warrior",
            "Witch","Wizard"
        };

        public readonly Dictionary<string, Dictionary<string, JobConfigEntries>> Values
            = new Dictionary<string, Dictionary<string, JobConfigEntries>>(StringComparer.OrdinalIgnoreCase);

        public ProfessionConfigValues(ConfigFile cfg)
        {
            foreach (var profName in TargetProfessions)
            {
                var profData = GameData.ProfessionDataList
                    .FirstOrDefault(p => p.Name.Equals(profName, StringComparison.OrdinalIgnoreCase));
                if (profData == null) continue;

                var section = $"Profession:{profName}";
                var jobDict = new Dictionary<string, JobConfigEntries>(StringComparer.OrdinalIgnoreCase);
                Values[profName] = jobDict;

                // Bind simple properties
                BindEntry(cfg, section, "Teachable", profData.Teachable,
                    v => profData.Teachable = v);

                BindEntry(cfg, section, "BaseChance", profData.BaseChance,
                    v => profData.BaseChance = v);

                BindEntry(cfg, section, "OnlyWhenSumEssencesIsLessThen", profData.OnlyWhenSumEssencesIsLessThen ?? -1,
                    v => profData.OnlyWhenSumEssencesIsLessThen = v < 0 ? (int?)null : v);

                BindEntry(cfg, section, "OnlyWhenSumEssencesIsGraterThen", profData.OnlyWhenSumEssencesIsGraterThen ?? -1,
                    v => profData.OnlyWhenSumEssencesIsGraterThen = v < 0 ? (int?)null : v);

                // Bind CSV list of enum values
                BindCsv(cfg, section, "ForbiddenStartingSpecies",
                    profData.ForbiddenStartingSpecies?.Select(x => x.ToString()),
                    arr => profData.ForbiddenStartingSpecies = arr
                        .Select(s => (ERace)Enum.Parse(typeof(ERace), s))
                        .ToList());

                BindCsv(cfg, section, "ForbiddenEssences",
                    profData.ForbiddenEssences?.Select(x => x.ToString()),
                    arr => profData.ForbiddenEssences = arr
                        .Select(s => (ETrait)Enum.Parse(typeof(ETrait), s))
                        .ToList());

                BindCsv(cfg, section, "RequiredEssences",
                    profData.RequiredEssences?.Select(kv => $"{kv.Key}:{kv.Value}"),
                    arr => profData.RequiredEssences = arr
                        .Select(p => p.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(
                            parts => (ETrait)Enum.Parse(typeof(ETrait), parts[0]),
                            parts => int.Parse(parts[1])
                        ));

                BindCsv(cfg, section, "StartingEssences",
                    profData.StartingEssences?.Select(kv => $"{kv.Key}:{kv.Value}"),
                    arr => profData.StartingEssences = arr
                        .Select(p => p.Split(':'))
                        .Where(parts => parts.Length == 2)
                        .ToDictionary(
                            parts => (ETrait)Enum.Parse(typeof(ETrait), parts[0]),
                            parts => int.Parse(parts[1])
                        ));

                // Bind each JobDescription
                foreach (var job in profData.Jobs ?? Enumerable.Empty<JobDescription>())
                {
                    var keyBase = job.Name.Replace(" ", "_");
                    var val = BindEntry(cfg, section, keyBase + "_Value", job.Value,
                        v => job.Value = v);
                    var max = BindEntry(cfg, section, keyBase + "_ValueMax", job.ValueMax,
                        v => job.ValueMax = v);
                    var chance = BindEntry(cfg, section, keyBase + "_Chance", job.Chance,
                        v => job.Chance = v, new AcceptableValueRange<int>(0, 100));

                    jobDict[job.Name] = new JobConfigEntries(val, max, chance);
                }
            }
        }

        // Generic bind
        private ConfigEntry<T> BindEntry<T>(ConfigFile cfg, string section, string key,
            T defaultValue, Action<T> apply,
            AcceptableValueBase range = null)
        {
            var desc = range == null
                ? new ConfigDescription(key)
                : new ConfigDescription(key, null, range);
            var entry = cfg.Bind(section, key, defaultValue, desc);
            entry.SettingChanged += (_, __) => apply(entry.Value);
            apply(entry.Value);
            return entry;
        }

        // CSV bind (list of strings)
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

    public class JobConfigEntries
    {
        public ConfigEntry<float> Value { get; }
        public ConfigEntry<float> ValueMax { get; }
        public ConfigEntry<int> Chance { get; }

        public JobConfigEntries(
            ConfigEntry<float> value,
            ConfigEntry<float> valueMax,
            ConfigEntry<int> chance)
        {
            Value = value;
            ValueMax = valueMax;
            Chance = chance;
        }
    }
}
