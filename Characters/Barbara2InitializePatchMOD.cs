using ComplexBreeding.Mechanics;
using ComplexBreeding.SpeciesCore.Data;
using ComplexBreeding.SpeciesCore;
using HarmonyLib;
using MBMScripts;
using DumberCBPatches.Configuration;

namespace DumberCBPatches.Characters
{

    [HarmonyPatch(typeof(Barbara2), "InitializeTrait")]
    public class Barbara2InitializePatchMOD
    {
        public static bool Prefix(Barbara2 __instance)
        {
            
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Barbara";

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Barbara2"];

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
