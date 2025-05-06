using System.Linq;
using ComplexBreeding;
using ComplexBreeding.Professions.Data;
using MBMScripts;
using UnityEngine;

namespace CBPatches.Professions
{
    public static class ProfessionPatch
    {
        public static void ApplyAll()
        {
            var all = GameData.ProfessionDataList.
                Where(p => p.GetType().Name.EndsWith("ProfessionData"));
            // Traverse GameData.ProfessionDataList for all '.ProfessionData'
            foreach (var prof in all )
            {
                Debug.Log($"[ProfessionPatch] Patching {prof.Name}");

                if (prof is PaladinProfessionData pal && pal.Id == 10)
                {
                    pal.ForbiddenStartingSpecies.Clear();
                    pal.ForbiddenEssences.Clear();
                    if (prof.Jobs != null)
                    {
                        foreach (var job in prof.Jobs)
                        {
                            switch (job.Name)
                            {
                                case "Adventuring":
                                    job.Value = 3600f;
                                    job.ValueMax = 12000f;
                                    job.Chance = 75;
                                    break;
                                case "Soul Cleansing":
                                    job.Value = 40f;
                                    job.ValueMax = 60f;
                                    break;
                                case "Casting Healing Spells":
                                    job.Value = 75f;
                                    job.ValueMax = 120f;
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
