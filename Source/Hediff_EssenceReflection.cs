using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Essencebound
{
    public sealed class Hediff_EssenceReflection : HediffWithComps
    {
        private int remainingTicks;
        public float reflectRatio = 0.25f;
        private Pawn creditCaster;
        private string creditEssence = "";
        private int creditSlot = -1;

        public void Refresh(int duration, Pawn caster, EssenceAbilitySlot slot)
        {
            remainingTicks = Math.Max(1, duration);
            creditCaster = caster;
            creditEssence = slot?.essenceDefName ?? "";
            creditSlot = slot?.index ?? -1;
        }

        public void Award(float actual)
        {
            if (actual > 0f && creditCaster != null && !creditCaster.Dead &&
                !string.IsNullOrEmpty(creditEssence) && creditSlot >= 0)
                Hediff_EssenceUser.ForPawn(creditCaster)?.AddAbilityWork(
                    creditEssence, creditSlot, actual);
        }

        public override void Tick()
        {
            base.Tick();
            if (remainingTicks > 0 && --remainingTicks == 0 && pawn?.health != null)
                pawn.health.RemoveHediff(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "ebReflectionTicks", 0);
            Scribe_Values.Look(ref reflectRatio, "ebReflectionRatio", 0.25f);
            Scribe_References.Look(ref creditCaster, "ebReflectionCaster");
            Scribe_Values.Look(ref creditEssence, "ebReflectionEssence", "");
            Scribe_Values.Look(ref creditSlot, "ebReflectionSlot", -1);
        }

        public bool Active => remainingTicks > 0;
    }

    // DamageInfo is shared with the existing rank-difference protection.
    // Guarded reflection can never recursively reflect itself.
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_EssenceReflection
    {
        [ThreadStatic] private static bool reflecting;

        private static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (reflecting || __instance == null || dinfo.Amount <= 0f ||
                !(dinfo.Instigator is Pawn attacker) || attacker.Dead ||
                attacker == __instance || !attacker.Spawned || !__instance.Spawned ||
                attacker.Map != __instance.Map ||
                __instance.Position.DistanceTo(attacker.Position) <= 2f) return;

            var shield = __instance.health?.hediffSet?.hediffs
                ?.OfType<Hediff_EssenceReflection>().FirstOrDefault(h => h.Active);
            if (shield == null) return;

            float original = dinfo.Amount;
            dinfo.SetAmount(original * (1f - shield.reflectRatio));
            try
            {
                reflecting = true;
                var result = attacker.TakeDamage(new DamageInfo(dinfo.Def,
                    original * shield.reflectRatio, 0f, -1f, __instance));
                shield.Award(Math.Max(0f, result.totalDamageDealt));
            }
            finally { reflecting = false; }
        }
    }
}
