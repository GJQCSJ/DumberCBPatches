using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MBMScripts;
using UnityEngine;
using ComplexBreeding.Professions;
using DumberCBPatches;
using ComplexBreeding.Professions.Data;
using ComplexBreeding;

namespace DumberCBPatches.Configuration
{
    [HarmonyPatch(typeof(ComplexBreeding.Patches.Character.InitializeTraitPatch2), "SetProfession")]
    public static class ProfessionRarityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Character character)
        {
            if (!CBPatches.EnableRarityPatch.Value)
            {
                Debug.Log("[DumberCBPatches] ⚠ Custom career assignment disabled, using original SetProfession()");
                return true; 
            }

            int roll = UnityEngine.Random.Range(1, 101);
            int tier = CBPatches.ProfessionRarityConfig.CalculateTier(roll);
            Debug.Log($"[DumberCBPatches] 🎯 Custom Set Professional takes effect：roll={roll}, tier={tier}");

            List<IProfessionData> list = GameData.ProfessionDataList
                .Where(x => (x.ForbiddenStartingSpecies == null || !x.ForbiddenStartingSpecies.Contains(character.Race)) && x.Tier == tier)
                .ToList();

            if (GameManager.Instance.PlayerData.GetUnitList(ESector.Female).Count == 0)
            {
                list = new List<IProfessionData> { CultistProfessionData.Data };
            }

            if (!list.Any())
            {
                Debug.LogWarning("[DumberCBPatches] ❗ No available profession, skip profession assignment");
                return false;
            }

            IProfessionData professionData = list[UnityEngine.Random.Range(0, list.Count)];

            character.AddTrait((ETrait)10);
            character.SetTraitValue((ETrait)10, professionData.Id);

            if (professionData.StartingEssences != null)
            {
                foreach (KeyValuePair<ETrait, int> kv in professionData.StartingEssences)
                {
                    if (character.GetTraitValue(kv.Key) == 0f)
                        character.AddTrait(kv.Key);

                    character.AddTraitValue(kv.Key, kv.Value);
                }
            }

            if (professionData.GuaranteedTraits != null)
            {
                foreach (ETrait trait in professionData.GuaranteedTraits)
                {
                    if (!character.TraitContains(trait))
                        character.AddRaceTrait(trait);
                }
            }

            character.SortTrait();

            return false; 
        }
    }
}
