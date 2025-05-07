using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace DumberCBPatches.Configuration
{
    public class ProfessionRarityConfigValues
    {
        public readonly ConfigEntry<int> ThresholdTier5;
        public readonly ConfigEntry<int> ThresholdTier4;
        public readonly ConfigEntry<int> ThresholdTier3;
        public readonly ConfigEntry<int> ThresholdTier2;
        public readonly ConfigEntry<int> ThresholdTier1;

        public ProfessionRarityConfigValues(ConfigFile cfg)
        {
            // Five thresholds, default to be consistent with the original logic
            ThresholdTier5 = cfg.Bind("ProfessionRarity", "Tier5_MaxRoll", 3, "roll <= A  => Tier5");
            ThresholdTier4 = cfg.Bind("ProfessionRarity", "Tier4_MaxRoll", 10, "A < roll <= B => Tier4");
            ThresholdTier3 = cfg.Bind("ProfessionRarity", "Tier3_MaxRoll", 20, "B < roll <= C => Tier3");
            ThresholdTier2 = cfg.Bind("ProfessionRarity", "Tier2_MaxRoll", 40, "C < roll <= D => Tier2");
            ThresholdTier1 = cfg.Bind("ProfessionRarity", "Tier1_MaxRoll", 100, "roll > D        => Tier1");
        }

        public int CalculateTier(int roll)
        {
            Debug.Log($"[DumberCBPatches] Calculate tier: roll={roll}, threshold=[T5={ThresholdTier5.Value}, T4={ThresholdTier4.Value}, T3={ThresholdTier3.Value}， T2={ThresholdTier2.Value}， T1={ThresholdTier1.Value}...]");
            if (roll <= ThresholdTier5.Value) return 5;
            if (roll <= ThresholdTier4.Value) return 4;
            if (roll <= ThresholdTier3.Value) return 3;
            if (roll <= ThresholdTier2.Value) return 2;
            // Last tier
            return 1;
        }
    }
}
