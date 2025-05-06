using System.Linq;
using ComplexBreeding;
using ComplexBreeding.Essences;
using MBMScripts;
using UnityEngine;

namespace CBPatches.Species
{
    public static class SpeciesPatch
    {
        public static void ApplyAll()
        {
            // Travers all classes ends with 'SpeciesData'
            var all = GameData.SpeciesDataList.
                Where(p => p.GetType().Name.EndsWith("SpeciesData"));
            foreach (var sp in all)
            {
                Debug.Log($"[SpeciesPatch] Patching species {sp.Name}");

                if (sp.ForbiddenEssences != null)
                {
                    sp.ForbiddenEssences.Clear();
                }
                var fathersProp = sp.GetType().GetProperty("PossiblyFathers");
                if (fathersProp != null)
                {
                    var currentList = (ERace[]?)fathersProp.GetValue(sp) ?? Array.Empty<ERace>();
                    var newList = new List<ERace>(currentList) { ERace.Human };
                    fathersProp.SetValue(sp, newList.ToArray());
                }
                // For example：for AngelSpeciesData, do it like：
                if (sp.GetType().Name == "AngelSpeciesData")
                {
                    // Assume there's a List<ETrait> ForbiddenTraits
                    // var angel = (AngelSpeciesData)sp;
                    // angel.ForbiddenTraits.Clear();
                    // … Other properties
                    
                }
            }
        }
    }
}
