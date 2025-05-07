using ComplexBreeding.Mechanics;
using ComplexBreeding.SpeciesCore;
using ComplexBreeding.SpeciesCore.Data;
using DumberCBPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBPatches.Characters
{

    [HarmonyPatch(typeof(Amilia2), "InitializeTrait")]
    public class Amilia2InitializePatchMOD
    {
        public static bool Prefix(Amilia2 __instance)
        {

            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Amilia";

            

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Amilia2"];

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