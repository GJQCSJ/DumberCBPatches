using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexBreeding.Mechanics;
using ComplexBreeding.SpeciesCore.Data;
using ComplexBreeding.SpeciesCore;
using HarmonyLib;
using MBMScripts;

namespace DumberCBPatches.Characters
{

    [HarmonyPatch(typeof(Aure), "InitializeTrait")]
    public class AureInitializePatchMOD
    {
        public static bool Prefix(Aure __instance)
        {

            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Aure";
            ISpeciesCoreData data = GnomeSpeciesCoreData.Data;
            __instance.SetSpeciesGameplayStats(data);
            __instance.SetPersonality();


            var cfg = DumberCBPatches.CBPatches.CharacterConfig
                      .Values["Aure"];

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
