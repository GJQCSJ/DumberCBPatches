using ComplexBreeding.Dictionaries;
using ComplexBreeding.Mechanics;
using ComplexBreeding.Reflections;
using HarmonyLib;
using MBMScripts;

[HarmonyPatch(typeof(Item), "Use")]
[HarmonyPriority(Priority.First)]
public static class MilkAsDumbUpdatePatch
{
    static bool Prefix(Item __instance, EItemType ___m_ItemType, Unit unit)
    {
        if (___m_ItemType == EItemType.Milk)
        {
            if (unit is Character chr)
            {
                bool doTrait =
                    //UnityEngine.Random.Range(0, 2) == 0 && 
                    chr is Female;
                    ;
                if (doTrait)
                {
                    if (unit is Female female && (int)female.GetWombSize() < 5)
                    {
                        int wombSize = (int)female.GetWombSize();
                        BuffHelpers.SetInt(ConfigBuffs.SlaveWombSize, wombSize + 1, female);
                    }
                }
                else
                {
                    ETrait chosen = (ETrait)UnityEngine.Random.Range(93, 99);
                    if (!chr.TraitList.Contains(chosen))
                        chr.AddTrait(chosen);
                    // +1 anyway
                    chr.AddTraitValue(chosen, 1f);
                }
                unit.PopUpMessage("Upgrade successful");
                GameManager.Instance.PlayerData.AddAchievement(EAchievement.A000030);
            }
            return false;
        }
        return true;
    }
}
