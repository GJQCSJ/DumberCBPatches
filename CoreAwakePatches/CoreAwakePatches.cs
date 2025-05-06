using CBPatches.Professions;
using CBPatches.Species;
using HarmonyLib;
using UnityEngine;

namespace CBPatches.CoreAwakePatches
{
    [HarmonyPatch(typeof(ComplexBreeding.Core), nameof(ComplexBreeding.Core.Awake))]
    public static class CoreAwakePatch
    {
        static void Postfix()
        {
            Debug.Log("[CoreAwakePatch] Core.Awake Completed，start batch patching");
            ProfessionPatch.ApplyAll();
            SpeciesPatch.ApplyAll();
            Debug.Log("[CoreAwakePatch] all patches installed");
        }
    }
}
