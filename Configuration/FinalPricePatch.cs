using ComplexBreeding;
using ComplexBreeding.Professions;
using ComplexBreeding.Species;
using MBMScripts;
using ComplexBreeding.Patches.Female;
using HarmonyLib;
using UnityEngine;

namespace DumberCBPatches.Configuration
{

    [HarmonyPatch(typeof(FinalPriceHelpers), nameof(FinalPriceHelpers.GetFinalPrice))]
    public static class FinalPricePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Female female, ref int __result)
        {
            if (female.IsDead)
            {
                __result = GameManager.ConfigData.CostOfDisposingCorpse;
                return false;
            }

            GameData.TryGetSpecies(female, out ISpeciesData species);
            int key = species?.Tier ?? 1;
            decimal price = GameData.TierPrice[key];

            if (GameData.TryGetProfession(female, out IProfessionData profession) && profession != null)
            {
                price += (decimal)GameData.TierPrice[profession.Tier] / 4m;
            }

            if (female.IsInfertile) price *= 0.5m;
            if (female.IsDepraved) price *= 0.8m;

            //  Apply trait-based modifiers
            foreach (var kv in CBPatches.TraitPriceModifiers)
            {
                ETrait trait = kv.Key;
                float perPointDelta = kv.Value;

                float value = female.GetTraitValue(trait);
                if (value > 0)
                {
                    price += (decimal)value * (decimal)perPointDelta;
                }
            }

            __result = Mathf.Max(0, (int)price);
            return false; 
        }
    }
}
