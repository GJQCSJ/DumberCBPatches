// CharacterConfigValues.cs
using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using ComplexBreeding;
using MBMScripts;
using ComplexBreeding.Species;
using ComplexBreeding.SpeciesCore;
using ComplexBreeding.Species.Data;
using ComplexBreeding.SpeciesCore.Data;
using System.Diagnostics;

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

        // Default species map for characters
        public static readonly Dictionary<string, string> defaultSpeciesMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Amilia2", "Human" },
            { "Anna",    "Sheep"  },
            { "Aure",    "Gnome"  },
            { "Barbara2","Gnome"  },
            { "Bella",   "Drake"  },
            { "Claire",  "Elf"    },
            { "Flora2",  "Elf"    },
            { "Karen",   "Werewolf" },
            { "Lena2",   "Sheep"  },
            { "Nero",    "Nekomata" },
            { "Niel2",   "Drake"  },
            { "Sena2",   "Sheep"  },
            { "Sylvia",  "Human"  },
            { "Vivi",    "Rabbit" }
        };

        //Role Name ->Configuration Item
        public readonly Dictionary<string, CharacterConfigEntries> Values
            = new Dictionary<string, CharacterConfigEntries>(StringComparer.OrdinalIgnoreCase);

        // Static Dictionary：Name -> Data Instance
        public static readonly Dictionary<string, ISpeciesData> speciesDataMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Angel",    AngelSpeciesData.Data    },
            { "Devil",    DevilSpeciesData.Data    },
            { "Dracolich",DracolichSpeciesData.Data},
            { "Dragon",   DragonSpeciesData.Data   },
            { "Drake",    DrakeSpeciesData.Data    },
            { "Drow",     DrowSpeciesData.Data     },
            { "Fairy",    FairySpeciesData.Data    },
            { "Ghoul",    GhoulSpeciesData.Data    },
            { "Goblin",   GoblinSpeciesData.Data   },
            { "Golem",    GolemSpeciesData.Data    },
            { "Hobgoblin",HobgoblinSpeciesData.Data},
            { "Horse",    HorseSpeciesData.Data    },
            { "Kitsune",  KitsuneSpeciesData.Data  },
            { "Mermaid",  MermaidSpeciesData.Data  },
            { "Minotaur", MinotaurSpeciesData.Data },
            { "Nekomata", NekomataSpeciesData.Data },
            { "Orc",      OrcSpeciesData.Data      },
            { "Redcap",   RedcapSpeciesData.Data   },
            { "Salamander",SalamanderSpeciesData.Data},
            { "Slime",    SlimeSpeciesData.Data    },
            { "Succubus", SuccubusSpeciesData.Data },
            { "Vampire",  VampireSpeciesData.Data  },
            { "Wererabbit",WererabbitSpeciesData.Data},
            { "Werewolf", WerewolfSpeciesData.Data }
        };
        public static readonly Dictionary<string, ISpeciesCoreData> speciesCoreDataMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Elf",   ElfSpeciesCoreData.Data   },
            { "Gnome", GnomeSpeciesCoreData.Data },
            { "Human", HumanSpeciesCoreData.Data },
            { "Imp",   ImpSpeciesCoreData.Data   },
            { "Lizard",LizardSpeciesCoreData.Data},
            { "Rabbit",RabbitSpeciesCoreData.Data},
            { "Sheep", SheepSpeciesCoreData.Data },
            { "Wolf",  WolfSpeciesCoreData.Data  }
        };

        //In order to achieve race modification, it is necessary to simultaneously change the ERace attribute assigned to the character in the game's Assembly.
        //The implementation of the character race modification function is therefore temporarily suspended
        public CharacterConfigValues(ConfigFile cfg)
        {
            // Merge all available species data keys
            var allKeys = speciesDataMap.Keys.Concat(speciesCoreDataMap.Keys).OrderBy(s => s).ToArray();

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

                // Character species menu
                var def = defaultSpeciesMap.TryGetValue(name, out var d) ? d : allKeys[0];
                entries.SpeciesType = cfg.Bind(
                    section, nameof(entries.SpeciesType), def,
                    new ConfigDescription(
                        "Choose character species",
                        null,
                        new AcceptableValueList<string>(allKeys)));
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

        public ConfigEntry<string> SpeciesType { get; set; }

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
