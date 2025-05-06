// PlayerConfigValues.cs
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
    ///Configure the parameters that players use when initializing the Trap.
    ///This static instance should be read in Initialize TraitPatch. Postfix.
    /// </summary>
    public class PlayerConfigValues
    {
        public ConfigEntry<float> SexTime { get; private set; }
        public ConfigEntry<float> ConceptionRate { get; private set; }
        public ConfigEntry<string> Traits { get; private set; }

        public PlayerConfigValues(ConfigFile cfg)
        {
            var section = "Player";

            SexTime = cfg.Bind(
                section,
                nameof(SexTime),
                120f,
                new ConfigDescription("SexTime")
            );

            ConceptionRate = cfg.Bind(
                section,
                nameof(ConceptionRate),
                0.75f,
                new ConfigDescription("ConceptionRate [0-1]")
            );

            var defaultCsv = string.Join(",",
                new[] { 93, 94, 95, 96, 97, 98 }
                    .Select(i => $"Trait{i}:2"));
            Traits = cfg.Bind(
                section,
                nameof(Traits),
                defaultCsv,
                new ConfigDescription("Player Trait:Value list,separated by commas")
            );
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
