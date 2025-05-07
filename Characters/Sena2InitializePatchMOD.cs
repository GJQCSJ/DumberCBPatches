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
    [HarmonyPatch(typeof(Sena2), "InitializeTrait")]
    public class Sena2InitializePatchMOD
    {
        public static bool Prefix(Sena2 __instance)
        {

            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Sena";

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Sena2"];

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


