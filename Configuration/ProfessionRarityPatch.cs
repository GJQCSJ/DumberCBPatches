using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MBMScripts;
using DumberCBPatches;
using DumberCBPatches.Configuration;
using UnityEngine;    // ← for Debug.Log

namespace DumberCBPatches.Configuration
{
    [HarmonyPatch(typeof(ComplexBreeding.Patches.Character.InitializeTraitPatch2), "SetProfession")]
    public static class ProfessionRarityPatch
    {
        static readonly MethodInfo CalcTier;
        static readonly PropertyInfo CfgProp;
        static readonly FieldInfo TierField;

        private static FieldInfo InitializeTierField()
        {
            // Hard coded nested class names
            var nestedType = typeof(ComplexBreeding.Patches.Character.InitializeTraitPatch2)
                .GetNestedType("<>c__DisplayClass1_0", BindingFlags.NonPublic);

            if (nestedType == null)
            {
                Debug.LogError("[DumberCBPatches] ❌ nested type <>c__DisplayClass1_0 not exist！");
                return null;
            }

            // Output all field information
            Debug.Log($"[DumberCBPatches] Find nested type: {nestedType.Name}");
            foreach (var field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Debug.Log($"[DumberCBPatches] field: {field.Name} (type: {field.FieldType}, attribute: {field.Attributes})");
            }

            // Strictly match public fields
            var tierField = nestedType.GetField("tier", BindingFlags.Public | BindingFlags.Instance);
            if (tierField == null)
            {
                Debug.LogError("[DumberCBPatches] ❌ tier Field does not exist！");
                return null;
            }

            Debug.Log($"[DumberCBPatches] ✔ Successfully obtained the tier field");
            return tierField;
        }

        static ProfessionRarityPatch()
        {
            // Initialization method reflection
            CalcTier = typeof(ProfessionRarityConfigValues)
                .GetMethod(nameof(ProfessionRarityConfigValues.CalculateTier),
                BindingFlags.Public | BindingFlags.Instance);

            // Initialize attribute reflection
            CfgProp = typeof(CBPatches)
                .GetProperty(nameof(CBPatches.ProfessionRarityConfig),
                BindingFlags.Public | BindingFlags.Static);


            var nestedType = typeof(ComplexBreeding.Patches.Character.InitializeTraitPatch2)
                .GetNestedType("<>c__DisplayClass1_0", BindingFlags.NonPublic);

            if (nestedType == null)
            {
                Debug.LogError("[DumberCBPatches] ❌ nested type <>c__DisplayClass1_0 does not exist！");
                return;
            }

            // Output information for all fields in nested classes
            Debug.Log($"[DumberCBPatches] Find nested type: {nestedType.Name}");
            foreach (var field in nestedType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Debug.Log($"[DumberCBPatches] field: {field.Name} (Type: {field.FieldType})");
            }

            TierField = InitializeTierField();
        }


        [HarmonyPrefix]
        static void Prefix_Debug()
        {
            Debug.Log("[DumberCBPatches] ✔ SetProfession patch applied");
        }

        [HarmonyTranspiler]   // ← This was missing
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instrs)
        {
            Debug.Log("[DumberCBPatches] Transpiler Start Execution");
            var matcher = new CodeMatcher(instrs)
        .MatchForward(false,
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Ldc_I4_S, (CodeInstruction code) => (sbyte)code.operand == 20),
            new CodeMatch(OpCodes.Bgt_S)
        );
            // When matching fails, return the original instruction directly
            if (matcher.IsInvalid)
            {
                Debug.LogError("[DumberCBPatches] ❌ Target instruction sequence not found");
                return instrs;
            }
            Debug.Log("[DumberCBPatches] ✔ Match to the target instruction sequence");

            matcher.RemoveInstructions(3);

            matcher
        .Advance(3)  // Skip the 3 matched instructions
        .InsertAndAdvance(
            // Log output raw roll value
            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Box, typeof(int)),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Debug), nameof(Debug.Log))),

            // Load configuration and calculate new tier
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Ldsfld, CfgProp.GetGetMethod()),
            new CodeInstruction(OpCodes.Ldloc_1),
            new CodeInstruction(OpCodes.Callvirt, CalcTier),
            new CodeInstruction(OpCodes.Stfld, TierField)
        );  // Delete the original 3 instructions

            Debug.Log("[DumberCBPatches] ✔ Instruction modification completed");
            return matcher.InstructionEnumeration();
            //    var codes = new List<CodeInstruction>(instrs);
            //    Debug.Log($"[DumberCBPatches] Original instruction count: {codes.Count}");
            //    int removeStart = -1, removeEnd = -1;


            //    if (CalcTier == null || CfgProp == null || TierField == null)
            //    {
            //        Debug.LogError($"[DumberCBPatches] ProfessionRarityPatch reflection FAILED: " +
            //                       $"CalcTier={CalcTier}, Cfg={CfgProp}, TierField={TierField}");
            //        return instrs.ToList();
            //    }

            //    // locate the very first ldloc.1; ldc.i4.s 20; bgt.s
            //    for (int i = 0; i < codes.Count - 2; i++)
            //    {
            //        var code1 = codes[i];
            //        var code2 = codes[i + 1];
            //        var code3 = codes[i + 2];

            //        Debug.Log($"[DumberCBPatches] Check instructions {i}: {code1.opcode} | {code2.opcode} ({code2.operand}) | {code3.opcode}");

            //        bool isLdloc1 = code1.opcode == OpCodes.Ldloc_1;
            //        bool isLdcI4 = (code2.opcode == OpCodes.Ldc_I4_S || code2.opcode == OpCodes.Ldc_I4);
            //        bool operandIs20 = Convert.ToInt32(code2.operand) == 20;
            //        bool isBgt = code3.opcode == OpCodes.Bgt_S;

            //        if (isLdloc1 && isLdcI4 && operandIs20 && isBgt)
            //        {
            //            removeStart = i;
            //            Debug.Log($"[DumberCBPatches] Match to instruction sequence, location: {i}");
            //            break;
            //        }


            //        if (codes[i].opcode == OpCodes.Ldloc_1
            //            && codes[i + 1].opcode == OpCodes.Ldc_I4_S 
            //            //&& (sbyte)codes[i + 1].operand == 20
            //            && Convert.ToInt32(codes[i + 1].operand) == 20
            //            && codes[i + 2].opcode == OpCodes.Bgt_S)
            //        {
            //            removeStart = i;
            //            Debug.Log($"[DumberCBPatches] Match to instruction sequence type 2, position: {i}");
            //            break;
            //        }
            //    }
            //    if (removeStart < 0)
            //    {
            //        Debug.LogError("[DumberCBPatches] ❌ Target instruction sequence not found");
            //        return codes;
            //    }

            //    // find the *last* stfld into displayClass.tier (that writes tier=2)
            //    for (int i = codes.Count - 1; i > removeStart; i--)
            //    {
            //        if (codes[i].opcode == OpCodes.Stfld && codes[i].operand == TierField)
            //        {
            //            removeEnd = i;
            //            Debug.Log($"[DumberCBPatches] Find the end position: {i}");
            //            break;
            //        }
            //    }
            //    if (removeEnd <= removeStart) 
            //    {
            //        Debug.LogError("[DumberCBPatches] ❌ Invalid deletion range); 
            //        return codes; 
            //    }

            //    // remove the entire old 5→4→3→2 block
            //    codes.RemoveRange(removeStart, removeEnd - removeStart + 1);
            //    Debug.Log($"[DumberCBPatches] Delete instruction count: {removeEnd - removeStart + 1}");

            //    // inject our config‐driven logic + debug log of the raw roll
            //    var patch = new List<CodeInstruction>
            //    {
            //        // log the raw roll so you know it’s firing
            //        new CodeInstruction(OpCodes.Ldloc_1),
            //        new CodeInstruction(OpCodes.Box,   typeof(int)),
            //        new CodeInstruction(OpCodes.Call,
            //AccessTools.Method(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.Log), new[]{ typeof(object) })),


            //        // now rewrite tier:
            //        new CodeInstruction(OpCodes.Ldloc_0),     // displayClass instance
            //        new CodeInstruction(OpCodes.Ldsfld, CfgProp),  // config singleton
            //        new CodeInstruction(OpCodes.Ldloc_1),        // the roll
            //        new CodeInstruction(OpCodes.Callvirt, CalcTier), // returns new tier
            //        new CodeInstruction(OpCodes.Stfld,    TierField),     // displayClass.tier = new tier
            //    };

            //    codes.InsertRange(removeStart, patch);
            //    Debug.Log($"[DumberCBPatches] Insert new instruction completed");
            //    return codes;
        }
    }

}
