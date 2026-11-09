using System;
using System.Linq;
using Verse;

namespace Essencebound
{
    // Real gradual healing, not an instant XP reward for casting a status.
    // Duration, caster and slot identity survive save/reload.
    public sealed class Hediff_EssenceRegeneration : HediffWithComps
    {
        private int ticksRemaining;
        private int ticksSinceHeal;
        private float healingPerPulse;
        private Pawn creditCaster;
        private string creditEssence = "";
        private int creditSlot = -1;

        public void Refresh(int duration, float amountPerPulse, Pawn caster,
            EssenceAbilitySlot slot)
        {
            ticksRemaining = Math.Max(1, duration);
            ticksSinceHeal = 0;
            healingPerPulse = Math.Max(0f, amountPerPulse);
            creditCaster = caster;
            creditEssence = caster != null && slot != null ? slot.essenceDefName : "";
            creditSlot = caster != null && slot != null ? slot.index : -1;
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksRemaining <= 0 || pawn?.health == null) return;
            if (++ticksSinceHeal >= 120 && !pawn.Dead)
            {
                ticksSinceHeal = 0;
                var wound = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                    .Where(i => !i.IsPermanent()).OrderByDescending(i => i.Severity).FirstOrDefault();
                if (wound != null)
                {
                    float before = wound.Severity;
                    wound.Heal(healingPerPulse);
                    float actual = Math.Max(0f, before - wound.Severity);
                    if (actual > 0f && creditCaster != null && !creditCaster.Dead &&
                        !string.IsNullOrEmpty(creditEssence))
                    {
                        Hediff_EssenceUser.ForPawn(creditCaster)?.AddAbilityWork(
                            creditEssence, creditSlot, actual);
                    }
                }
            }
            if (--ticksRemaining == 0)
                pawn.health.RemoveHediff(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ebRegenTicksRemaining", 0);
            Scribe_Values.Look(ref ticksSinceHeal, "ebRegenPulseTick", 0);
            Scribe_Values.Look(ref healingPerPulse, "ebRegenAmount", 0f);
            Scribe_References.Look(ref creditCaster, "ebRegenCaster");
            Scribe_Values.Look(ref creditEssence, "ebRegenEssence", "");
            Scribe_Values.Look(ref creditSlot, "ebRegenSlot", -1);
        }
    }
}
