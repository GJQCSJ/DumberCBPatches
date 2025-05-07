using ComplexBreeding.Mechanics;
using ComplexBreeding.Species;
using ComplexBreeding.SpeciesCore;
using ComplexBreeding.SpeciesCore.Data;
using DumberCBPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBPatches.Characters
{

    [HarmonyPatch(typeof(Anna), "InitializeTrait")]
    public class AnnaInitializePatchMOD
    {
        public static bool Prefix(Anna __instance)
        {
            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Anna"];
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Anna";
            
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