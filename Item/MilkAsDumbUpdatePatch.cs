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
                int type = UnityEngine.Random.Range(0, 2);
                if (type == 0 && unit is Female)
                {
                    int raw = UnityEngine.Random.Range(93, 99);
                    ETrait chosen = (ETrait)raw;
                    if (chr.TraitList.Contains(chosen))
                    {
                        chr.UpgradeTrait(chosen);
                    }
                    else
                    {
                        chr.AddTrait(chosen);
                    }
                }
                else
                {
                    if (unit is Female female)
                    {
                        int wombSize = (int)female.GetWombSize();
                        if (wombSize < 5)
                        {
                            BuffHelpers.SetInt(ConfigBuffs.SlaveWombSize, wombSize + 1, female);
                        }
                    }
                }
                unit.PopUpMessage("Upgrade successful");
                GameManager.Instance.PlayerData.AddAchievement(EAchievement.A000030);
            }
            return false;
        }
        return true;
    }
}
