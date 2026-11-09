using HarmonyLib;
using RimWorld;
using Verse;

namespace Essencebound
{
    // A marked hostile is vulnerable to attacks by the caster's faction.
    // Rank mitigation and normal armor are still handled by their own systems.
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_EssencePredatorMark
    {
        private static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (__instance == null || dinfo.Amount <= 0f ||
                !(dinfo.Instigator is Pawn attacker) || attacker.Faction == null)
                return;
            var markDef = DefDatabase<HediffDef>
                .GetNamedSilentFail("EB_Debuff_PredatorMark");
            if (markDef == null) return;
            var status = __instance.health?.hediffSet
                ?.GetFirstHediffOfDef(markDef) as Hediff_EssenceTimed;
            if (status?.CreditCaster?.Faction != attacker.Faction) return;
            dinfo.SetAmount(dinfo.Amount * 1.18f);
        }
    }
}
