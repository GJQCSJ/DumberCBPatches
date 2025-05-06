// CharacterConfigValues.cs
using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using ComplexBreeding;
using MBMScripts;

namespace DumberCBPatches.Configuration
{
    /// <summary>
    /// Configure the parameters used by each NPC character in Initialize Trap.
    /// This static instance should be read in the Harmony Patch class (such as AnnaInitializePatchMOD).
    /// </summary>
    public class CharacterConfigValues
    {
        static readonly string[] TargetCharacters = {
            "Amilia2","Anna","Aure","Barbara2","Bella",
            "Claire","Flora2","Karen","Lena2","Nero",
            "Niel2","Sena2","Sylvia","Vivi"
        };

        //Role Name ->Configuration Item
        public readonly Dictionary<string, CharacterConfigEntries> Values
            = new Dictionary<string, CharacterConfigEntries>(StringComparer.OrdinalIgnoreCase);

        public CharacterConfigValues(ConfigFile cfg)
        {
            foreach (var name in TargetCharacters)
            {
                var section = $"Character:{name}";
                var entries = new CharacterConfigEntries(name);
                Values[name] = entries;

                // Titstype
                entries.TitsType = cfg.Bind(
                    section,
                    nameof(entries.TitsType),
                    entries.DefaultTitsType,
                    new ConfigDescription($"TitsType for {name}")
                );

                // Feature List, CSV Format TraitiID: Value
                entries.Traits = cfg.Bind(
                    section,
                    nameof(entries.Traits),
                    entries.DefaultTraitPairs,
                    new ConfigDescription("Character Trait:Value list,separated by commas")
                );
            }
        }
    }

    public class CharacterConfigEntries
    {
        public string CharacterName { get; }
        public int DefaultTitsType { get; } = 1;
        public string DefaultTraitPairs => string.Join(",",
            new[] { "Trait93:15", "Trait94:15", "Trait95:15",
                    "Trait96:15", "Trait97:15", "Trait98:15" });

        // Filled by Config Manager
        public ConfigEntry<int> TitsType { get; set; }
        public ConfigEntry<string> Traits { get; set; }

        public CharacterConfigEntries(string characterName)
        {
            CharacterName = characterName;
        }

        /// <summary>
        /// Parse from CSV to (ETrait, float) pairs
        /// </summary>
        public IEnumerable<(ETrait trait, float value)> ParseTraits()
        {
            return Traits.Value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split(':'))
                .Where(parts => parts.Length == 2 &&
                                Enum.TryParse<ETrait>(parts[0], out _) &&
                                float.TryParse(parts[1], out _))
                .Select(parts => (
                    (ETrait)Enum.Parse(typeof(ETrait), parts[0]),
                    float.Parse(parts[1])
                ));
        }
    }
}
