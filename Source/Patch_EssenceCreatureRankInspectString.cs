using System.Linq;
using HarmonyLib;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// Show a creature's effective rank in the normal selection inspect pane.
    /// This uses exactly the same rank resolver as combat and loot; astral
    /// manifestations show their assigned rank, not their placeholder species.
    /// A separate harmless rank hediff exposes the same tier in the Health tab.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class Patch_EssenceCreatureRankInspectString
    {
        private static void Postfix(Pawn __instance, ref string __result)
        {
            if (__instance?.RaceProps == null ||
                __instance.RaceProps.Humanlike || __instance.Dead)
                return;

            bool manifested = __instance.health?.hediffSet?.hediffs
                ?.OfType<Hediff_AstralManifestation>().Any() == true;
            int rank = EssenceRankCombat.RankOf(__instance);
            string rankLine = (manifested ? "Astral rank: " : "Creature rank: ") +
                EssenceCreatureRanks.RankLabel(rank);

            // Append without replacing vanilla inspection lines (health,
            // mental state, faction, equipment, etc.) or other mod information.
            __result = string.IsNullOrEmpty(__result) ? rankLine :
                __result.TrimEnd('\r', '\n') + "\n" + rankLine;
        }
    }
}
