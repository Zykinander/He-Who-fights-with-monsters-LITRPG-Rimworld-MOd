using System;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Target-following damage over time, persisted with its pawn and credited
    // only for actual damage dealt. Rank mitigation still applies per pulse.
    public sealed class Hediff_EssenceCorruption : HediffWithComps
    {
        private int remainingTicks;
        private int pulseTick;
        private float damagePerPulse;
        private Pawn creditCaster;
        private string creditEssence = "";
        private int creditSlot = -1;
        public void Refresh(int duration, float damage, Pawn caster,
            EssenceAbilitySlot slot)
        {
            remainingTicks = Math.Max(60, duration);
            damagePerPulse = Math.Max(0f, damage);
            pulseTick = 0;
            creditCaster = caster;
            creditEssence = slot?.essenceDefName ?? "";
            creditSlot = slot?.index ?? -1;
        }
        public override void Tick()
        {
            base.Tick();
            if (remainingTicks <= 0 || pawn?.health == null) return;
            if (++pulseTick >= 180 && !pawn.Dead && pawn.Spawned &&
                creditCaster != null && creditCaster.Spawned && !creditCaster.Dead &&
                creditCaster.Map == pawn.Map && pawn.HostileTo(creditCaster))
            {
                pulseTick = 0;
                if (EssencePowerExecutor.FieldDamage(creditCaster, pawn, "Blunt",
                    damagePerPulse, out float actual) && actual > 0f &&
                    !string.IsNullOrEmpty(creditEssence) && creditSlot >= 0)
                    Hediff_EssenceUser.ForPawn(creditCaster)?.AddAbilityWork(
                        creditEssence, creditSlot, actual);
            }
            if (--remainingTicks <= 0)
                pawn.health.RemoveHediff(this);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "ebCorruptionTicks", 0);
            Scribe_Values.Look(ref pulseTick, "ebCorruptionPulse", 0);
            Scribe_Values.Look(ref damagePerPulse, "ebCorruptionDamage", 0f);
            Scribe_References.Look(ref creditCaster, "ebCorruptionCaster");
            Scribe_Values.Look(ref creditEssence, "ebCorruptionEssence", "");
            Scribe_Values.Look(ref creditSlot, "ebCorruptionSlot", -1);
        }
    }
}
