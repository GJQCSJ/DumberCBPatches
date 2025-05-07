using HarmonyLib;
using MBMScripts;
using ComplexBreeding.Patches;
using ComplexBreeding.Reflections;

namespace DumberCBPatches.Item
{

    [HarmonyPatch(typeof(MBMScripts.Item), "Use")]
    static class CosmeticPillOverridePatch
    {  
        static void Postfix(ref bool __result, EItemType ___m_ItemType, Unit unit)
        {
            if (___m_ItemType == EItemType.Item_CosmeticPill)
            {

                __result = true;
            }
        }
    }
}
