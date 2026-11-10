using HarmonyLib;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// Health-tab display only: no stat modifiers, severity scaling or extra
    /// damage mechanics. The label always resolves through the combat rank
    /// calculator, so astral monsters show their actual assigned rank.
    /// </summary>
    public sealed class Hediff_CreatureRank : Hediff
    {
        public override string LabelBase
        {
            get
            {
                if (pawn == null) return "Creature rank";
                bool astral = pawn.health?.hediffSet?.hediffs
                    ?.Exists(h => h is Hediff_AstralManifestation) == true;
                int tier = EssenceRankCombat.RankOf(pawn);
                return (astral ? "Astral rank: " : "Creature rank: ") +
                    EssenceCreatureRanks.RankLabel(tier);
            }
        }

        public override string TipStringExtra =>
            "This is the creature's effective Essencebound rank. It governs " +
            "rank-based combat resistance and the quality and chance of " +
            "essence and awakening-stone loot.";
    }

    /// <summary>
    /// RimWorld runs SpawnSetup for newly spawned creatures and when existing
    /// map pawns are loaded. Add the hediff once; repeated loads or respawns
    /// must not create duplicate rank entries.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Patch_EssenceCreatureRankOnSpawn
    {
        private static void Postfix(Pawn __instance)
        {
            if (__instance?.RaceProps == null ||
                __instance.RaceProps.Humanlike || __instance.Dead ||
                __instance.health?.hediffSet == null)
                return;

            HediffDef rankDef = DefDatabase<HediffDef>
                .GetNamedSilentFail("EB_CreatureRank");
            if (rankDef == null ||
                __instance.health.hediffSet.GetFirstHediffOfDef(rankDef) != null)
                return;

            __instance.health.AddHediff(rankDef);
        }
    }
}
