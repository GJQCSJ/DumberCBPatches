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

    [HarmonyPatch(typeof(Flora2), "InitializeTrait")]
    public class Flora2InitializePatchMOD
    {
        public static bool Prefix(Flora2 __instance)
        {
           
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Flora";

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Flora2"];

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