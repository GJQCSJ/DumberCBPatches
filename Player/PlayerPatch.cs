using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DumberCBPatches;

namespace DumberCBPatches.Player
{
    using System;
    using HarmonyLib;
    using MBMScripts;

    [HarmonyPatch(typeof(Player), "InitializeTrait", new Type[] { })]
    public class InitializeTraitPatch
    {
        public static void Postfix(Player __instance)
        {
            __instance.OnEnableTrait();
            __instance.ClearRaceTrait();
            __instance.ClearTrait();

            var pCfg = DumberCBPatches.CBPatches.PlayerConfig;
            __instance.SexTime = pCfg.SexTime.Value;
            __instance.ConceptionRate = pCfg.ConceptionRate.Value;
            foreach (var (trait, val) in pCfg.ParseTraits())
            {
                __instance.AddTrait(trait);
                __instance.AddTraitValue(trait, val);
            }
        }
    }

}
