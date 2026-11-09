using System;
using System.Linq;
using HarmonyLib;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// Common Iron/Bronze/Silver/Gold/Diamond damage rules for all ranked
    /// pawns, including Essencebound users and astral manifestations.
    /// Unranked attackers are represented as -1.
    /// </summary>
    public static class EssenceRankCombat
    {
        public static int RankOf(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return -1;
            var manifestation = pawn.health.hediffSet.hediffs
                .OfType<Hediff_AstralManifestation>().FirstOrDefault();
            if (manifestation != null)
                return Math.Max(0, Math.Min(4, manifestation.rankIndex));

            var essenceUser = Hediff_EssenceUser.ForPawn(pawn);
            if (essenceUser == null || essenceUser.OwnedEssenceNames.Count == 0)
                return -1;

            // A user's defensive tier is the least-advanced owned essence,
            // matching the overall rank displayed in their character profile.
            return Math.Max(0, Math.Min(4, essenceUser.OwnedEssenceNames
                .Min(essenceUser.GetEssenceRankIndex)));
        }

        public static float LowerRankDamageFactor(int targetRank, int attackerRank)
        {
            if (targetRank < 0 || attackerRank >= targetRank)
                return 1f;
            int gap = Math.Min(5, targetRank - attackerRank);
            return Math.Max(0.25f, (float)Math.Pow(0.75f, gap));
        }

        // Ranked monster attacks become more dangerous as tears grow stronger.
        // Avoid double-counting the ability scaling already on Essencebound users.
        public static float MonsterOutgoingFactor(int rank)
        {
            switch (rank)
            {
                case 1: return 1.15f;
                case 2: return 1.35f;
                case 3: return 1.60f;
                case 4: return 2.00f;
                default: return 1f;
            }
        }
    }

    // Patches the engine's common pawn damage hook rather than the 12 custom
    // powers only, covering normal attacks and powers with Pawn instigators.
    // Damage from map hazards without a pawn instigator remains unchanged.
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_EssenceRankIncomingDamage
    {
        private static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (__instance == null || dinfo.Amount <= 0f ||
                !(dinfo.Instigator is Pawn attacker)) return;

            float amount = dinfo.Amount;

            var manifestedAttacker = attacker.health?.hediffSet?.hediffs
                ?.OfType<Hediff_AstralManifestation>().FirstOrDefault();
            if (manifestedAttacker != null)
                amount *= EssenceRankCombat.MonsterOutgoingFactor(manifestedAttacker.rankIndex);

            int targetRank = EssenceRankCombat.RankOf(__instance);
            int attackerRank = EssenceRankCombat.RankOf(attacker);
            amount *= EssenceRankCombat.LowerRankDamageFactor(targetRank, attackerRank);

            dinfo.SetAmount(amount);
        }
    }
}
