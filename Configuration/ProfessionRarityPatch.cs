using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MBMScripts;
using DumberCBPatches;
using DumberCBPatches.Configuration;

namespace DumberCBPatches.Configuration
{
    [HarmonyPatch(typeof(ComplexBreeding.Patches.Character.InitializeTraitPatch2), "SetProfession")]
    public static class ProfessionRarityPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int startIndex = -1, endIndex = -1;
            // 找到旧 tier 分支开始: ldloc.1, ldc.i4.s 20, bgt.s
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_1
                 && codes[i + 1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i + 1].operand == 20
                 && codes[i + 2].opcode == OpCodes.Bgt_S)
                {
                    startIndex = i;
                    break;
                }
            }
            // 找到旧 tier 分支结束: ldloc.1, ldc.i4.s 40, bgt.s
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_1
                 && codes[i + 1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i + 1].operand == 40
                 && codes[i + 2].opcode == OpCodes.Bgt_S)
                {
                    endIndex = i + 3;
                    break;
                }
            }
            if (startIndex >= 0 && endIndex > startIndex)
            {
                // 删除原有逻辑
                codes.RemoveRange(startIndex, endIndex - startIndex);
                // 反射获取配置 & 方法 & 字段
                var configField = AccessTools.Field(typeof(CBPatches), "ProfessionRarityConfig");
                var calcMethod = AccessTools.Method(typeof(ProfessionRarityConfigValues), "CalculateTier");
                // 获取嵌套DisplayClass类型
                var displayType = typeof(ComplexBreeding.Patches.Character.InitializeTraitPatch2)
                    .GetNestedType("<>c__DisplayClass1_0", BindingFlags.NonPublic | BindingFlags.Instance);
                var tierField = AccessTools.Field(displayType, "tier");

                // 新注入指令: displayClass -> config -> roll -> CalculateTier -> set tier
                var newInstr = new List<CodeInstruction>
                {
                    // 加载 displayClass 引用 (local 0)
                    new CodeInstruction(OpCodes.Ldloc_0),
                    // 加载配置单例
                    new CodeInstruction(OpCodes.Ldsfld, configField),
                    // 加载 roll (local 1)
                    new CodeInstruction(OpCodes.Ldloc_1),
                    // 调用 CalculateTier
                    new CodeInstruction(OpCodes.Callvirt, calcMethod),
                    // 存回 displayClass.tier
                    new CodeInstruction(OpCodes.Stfld, tierField),
                };
                codes.InsertRange(startIndex, newInstr);
            }
            return codes;
        }
    }
}
