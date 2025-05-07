using HarmonyLib;
using MBMScripts;

[HarmonyPatch(typeof(Item), "Use")]
[HarmonyPriority(Priority.LowerThanNormal)] 
public static class CosmeticPillOverridePatch
{
    [HarmonyPrefix]
    public static void PostPrefix(Item __instance, EItemType ___m_ItemType, Unit unit, ref bool __runOriginal)
    {
        if (___m_ItemType != EItemType.Item_CosmeticPill)
            return;

        // Override UsePatch results
        __runOriginal = true; // Frocibly run original use method
    }
}