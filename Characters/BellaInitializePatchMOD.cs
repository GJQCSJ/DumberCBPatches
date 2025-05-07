using ComplexBreeding.Mechanics;
using ComplexBreeding.SpeciesCore.Data;
using ComplexBreeding.SpeciesCore;
using HarmonyLib;
using MBMScripts;
using ComplexBreeding.Species.Data;
using ComplexBreeding.Species;
using DumberCBPatches.Configuration;

namespace DumberCBPatches.Characters
{

    [HarmonyPatch(typeof(Bella), "InitializeTrait")]
    public class BellaInitializePatchMOD
    {
        public static bool Prefix(Bella __instance)
        {

            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Bella";

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Bella"];

            var choice = cfg.SpeciesType.Value;
            if (CharacterConfigValues.speciesDataMap.TryGetValue(choice, out var sd))
            {
                __instance.SetSpeciesGameplayStats(sd);
            }
            else if (CharacterConfigValues.speciesCoreDataMap.TryGetValue(choice, out var scd))
            {
                __instance.SetSpeciesGameplayStats(scd);
            }

            __instance.SetPersonality();
            __instance.TitsType = cfg.TitsType.Value;


            foreach (var (trait, val) in cfg.ParseTraits())
            {
                __instance.AddTrait(trait);
                __instance.AddTraitValue(trait, val);
            }

            return false; 
        }
    }
}

