using ComplexBreeding.Mechanics;
using ComplexBreeding.SpeciesCore;
using ComplexBreeding.SpeciesCore.Data;
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
            ISpeciesCoreData data = HumanSpeciesCoreData.Data;
            __instance.SetSpeciesGameplayStats(data);
            __instance.SetPersonality();

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Amilia2"];

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