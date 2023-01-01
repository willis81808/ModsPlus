using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using HarmonyLib;
using UnboundLib;

namespace ModsPlus.Patches
{
    [HarmonyPatch(typeof(CardBar))]
    public static class CardBarPatches
    {
        internal static Dictionary<CardInfo, string> customAbbreviations = new Dictionary<CardInfo, string>();

        [HarmonyPostfix]
        [HarmonyPatch("AddCard")]
        public static void AddCard_Postfix(CardBar __instance, CardInfo card)
        {
            if (customAbbreviations.TryGetValue(card, out string abbr))
            {
                var gameObject = __instance.transform.GetChild(__instance.transform.childCount - 1);
                gameObject.GetComponentInChildren<TextMeshProUGUI>().text = abbr;
            }
        }
    }
}
