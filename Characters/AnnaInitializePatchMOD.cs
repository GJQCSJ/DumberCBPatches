using ComplexBreeding.Mechanics;
using ComplexBreeding.SpeciesCore;
using ComplexBreeding.SpeciesCore.Data;
using HarmonyLib;
using MBMScripts;

namespace DumberCBPatches.Characters
{

    [HarmonyPatch(typeof(Anna), "InitializeTrait")]
    public class AnnaInitializePatchMOD
    {
        public static bool Prefix(Anna __instance)
        {

            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Anna";
            ISpeciesCoreData data = SheepSpeciesCoreData.Data;
            __instance.SetSpeciesGameplayStats(data);
            __instance.SetPersonality(); 

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Anna"];

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