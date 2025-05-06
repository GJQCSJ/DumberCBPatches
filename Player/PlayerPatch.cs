using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            __instance.SexTime = 120f;
            __instance.ConceptionRate = 0.75f;
            __instance.AddTrait(ETrait.Trait93);
            __instance.AddTraitValue(ETrait.Trait93, 2f);
            __instance.AddTrait(ETrait.Trait94);
            __instance.AddTraitValue(ETrait.Trait94, 2f);
            __instance.AddTrait(ETrait.Trait95);
            __instance.AddTraitValue(ETrait.Trait95, 2f);
            __instance.AddTrait(ETrait.Trait96);
            __instance.AddTraitValue(ETrait.Trait96, 2f);
            __instance.AddTrait(ETrait.Trait97);
            __instance.AddTraitValue(ETrait.Trait97, 2f);
            __instance.AddTrait(ETrait.Trait98);
            __instance.AddTraitValue(ETrait.Trait98, 2f);
        }
    }

}
