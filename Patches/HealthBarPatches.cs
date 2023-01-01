using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using UnboundLib;

namespace ModsPlus.Patches
{
    [HarmonyPatch(typeof(HealthBar))]
    public static class HealthBarPatches
    {
        public static bool CustomHealthBarExists(GameObject healthBar)
        {
            var customHealthBar = healthBar.GetComponentInParent<CustomHealthBar>();
            return customHealthBar != null;
        }

        public static float HealthBarCalculatePercentageOverride(GameObject healthBar)
        {
            var baseHealthBar = healthBar.GetComponent<HealthBar>();
            if (baseHealthBar == null) return -1;

            var customHealthBar = healthBar.GetComponentInParent<CustomHealthBar>();
            if (customHealthBar == null)
            {
                var data = (CharacterData)baseHealthBar.GetFieldValue("data");
                return data.health / data.maxHealth;
            }

            return customHealthBar.CurrentHealth / customHealthBar.MaxHealth;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("Update")]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            FieldInfo hpTargField = AccessTools.Field(typeof(HealthBar), "hpTarg");
            FieldInfo characterDataField = AccessTools.Field(typeof(HealthBar), "data");

            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < code.Count; i++)
            {
                var currentInstruction = code[i];

                // /* 0x0002444A */ IL_0002: ldfld     class CharacterData HealthBar::data	// Finds the value of a field in the object
                if (startIndex < 0 && currentInstruction.opcode == OpCodes.Ldfld && currentInstruction.LoadsField(characterDataField))
                    startIndex = i;

                // /* 0x00024460 */ IL_0018: stfld     float32 HealthBar::hpTarg	// Replaces the value stored in the field of an object
                if (endIndex < 0 && currentInstruction.opcode == OpCodes.Stfld && currentInstruction.StoresField(hpTargField))
                    endIndex = i;
            }

            if (startIndex < 0 || endIndex < 0)
            {
                ModsPlusPlugin.LOGGER.LogError($"[HealthBar] Update transpiler unable to find code block to replace");
                return code;
            }

            code.RemoveRange(startIndex, (endIndex - startIndex) + 1);
            code.InsertRange(startIndex, new List<CodeInstruction>
            {
                // this.hpTarg = HealthBarPatches.HealthBarCalculatePercentageOverride(base.gameObject);
                CodeInstruction.Call(typeof(UnityEngine.Component), "get_gameObject"),
                CodeInstruction.Call(typeof(HealthBarPatches), nameof(HealthBarPatches.HealthBarCalculatePercentageOverride), parameters: new []{ typeof(GameObject) }),
                CodeInstruction.StoreField(typeof(HealthBar), "hpTarg")
            });

            return code;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("Start")]
        static IEnumerable<CodeInstruction> StartTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var code = new List<CodeInstruction>(instructions);

            Label continueLabel = generator.DefineLabel();
            MethodInfo getCharacterStatModifiersInParentMethod = AccessTools.Method(typeof(UnityEngine.Component), "GetComponentInParent", generics: new[] { typeof(CharacterStatModifiers) });

            int insertIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                // /* 0x0000000D */ IL_000D: call      instance !!0 [UnityEngine.CoreModule]UnityEngine.Component::GetComponentInParent<class CharacterStatModifiers>()	// Calls the method indicated by the passed method descriptor.
                if (insertIndex < 0 && code[i].opcode == OpCodes.Ldarg_0 && code[i + 1].Calls(getCharacterStatModifiersInParentMethod))
                {
                    insertIndex = i;
                    code[i].WithLabels(continueLabel);
                    break;
                }
            }

            if (insertIndex == -1)
            {
                ModsPlusPlugin.LOGGER.LogError($"[HealthBar] Start transpiler unable to find call to 'GetComponentInParent<CharacterStatModifiers>'");
                return code;
            }

            code.InsertRange(insertIndex, new List<CodeInstruction>
            {
                // call `HealthBarPatches.CustomHealthBarExists(base.gameObject)
                new CodeInstruction(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(UnityEngine.Component), "get_gameObject"),
                CodeInstruction.Call(typeof(HealthBarPatches), nameof(HealthBarPatches.CustomHealthBarExists), parameters: new[] { typeof(GameObject) }),

                // continue if returned false, otherwise exit method
                new CodeInstruction(OpCodes.Brfalse_S, continueLabel),
                new CodeInstruction(OpCodes.Ret)
            });

            return code;
        }
    }
}
