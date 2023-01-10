using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using ModsPlus.Patches;

namespace ModsPlus
{
    public static class ExtensionMethods
    {
        public static void RemovePlayerDiedAction(this PlayerManager pm, Action<Player, int> listener)
        {
            var action = (Action<Player, int>)pm.GetFieldValue("PlayerDiedAction");
            action -= listener;
        }

        public static Player GetPlayerWithID(this PlayerManager pm, int playerId)
        {
            return (Player)pm.InvokeMethod("GetPlayerWithID", playerId);
        }

        public static bool TryGetComponent<T>(this GameObject go, out T component) where T : Component
        {
            component = go.GetComponent<T>();
            return component != null;
        }

        public static bool VisibleFrom(this Player p, Vector3 origin)
        {
            return p.VisibleFrom(origin, LayerMask.GetMask("Default"));
        }

        public static bool VisibleFrom(this Player p, Vector3 origin, int layerMask)
        {
            var hit = Physics2D.Raycast(origin, Vector3.Normalize(p.transform.position - origin), Vector3.Distance(origin, p.transform.position));
            return hit.collider == null;
        }

        public static void AddStatusIndicator(this Player player, GameObject statusObj, float verticalPadding = 0f, bool normalizeScale = true)
        {
            player.GetComponentInChildren<PlayerWobblePosition>().gameObject
                .GetOrAddComponent<StatusManager>()
                .AddStatusObject(statusObj, verticalPadding, normalizeScale);
        }

        public static void SetAbbreviation(this CardInfo card, string abbreviation)
        {
            if (string.IsNullOrWhiteSpace(abbreviation))
            {
                throw new Exception($"[ModsPlus] - Attempted to set a card abbreviation to a null or empty string for card: {card.cardName}");
            }
            else if (abbreviation.Length > 2)
            {
                UnityEngine.Debug.LogWarning($"[ModsPlus] - Attempted to set card abbreviation for {card.cardName} to {abbreviation}, which is more than 2 characters long, will be truncated!");
            }

            var text = abbreviation.Substring(0, 2);
            string text2 = text[0].ToString().ToUpper();
            if (text.Length > 1)
            {
                string str = text[1].ToString().ToLower();
                text = text2 + str;
            }
            else
            {
                text = text2;
            }

            CardBarPatches.customAbbreviations[card] = text;
        }

        public static string GetAbbreviation(this CardInfo card)
        {
            if (CardBarPatches.customAbbreviations.TryGetValue(card, out string abbr))
            {
                return abbr;
            }
            else
            {
                string text = card.cardName;
                text = text.Substring(0, 2);
                string text2 = text[0].ToString().ToUpper();
                if (text.Length > 1)
                {
                    string str = text[1].ToString().ToLower();
                    text = text2 + str;
                }
                else
                {
                    text = text2;
                }
                return text;
            }
        }
    }
}
