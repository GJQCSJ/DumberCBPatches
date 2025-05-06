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
using ComplexBreeding.Species.Data;
using ComplexBreeding.Species;

namespace DumberCBPatches.Characters {

    [HarmonyPatch(typeof(Sylvia), "InitializeTrait")]
    public class SylviaInitializePatchMOD
    {
        public static bool Prefix(Sylvia __instance)
        {
            
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();
            __instance.DisplayName = "Sylvia";
            ISpeciesCoreData data = HumanSpeciesCoreData.Data;
            __instance.SetSpeciesGameplayStats(data);
            __instance.SetPersonality();

            var cfg = DumberCBPatches.CBPatches.CharacterConfig
          .Values["Sylvia"];

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

