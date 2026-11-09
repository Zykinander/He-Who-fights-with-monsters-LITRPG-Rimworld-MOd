using RimWorld;
using Verse;

namespace Essencebound
{
    // Duration-managed effect with optional support-training credit for its caster.
    // Source info is saved so buffs may keep training after save/load.
    public sealed class Hediff_EssenceTimed : HediffWithComps
    {
        private int ticksRemaining;
        private Pawn creditCaster;
        private string creditEssence = "";
        private int creditSlot = -1;
        private int benefitTicks;
        private bool trainOnlyAgainstHostile;

        public void Refresh(int durationTicks, Pawn caster, EssenceAbilitySlot slot,
            bool hostileOnly = false)
        {
            ticksRemaining = durationTicks > 0 ? durationTicks : 1;
            creditCaster = caster;
            creditEssence = caster != null && slot != null ? slot.essenceDefName : "";
            creditSlot = caster != null && slot != null ? slot.index : -1;
            benefitTicks = 0;
            trainOnlyAgainstHostile = hostileOnly;
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksRemaining <= 0) return;
            if (creditCaster != null && pawn != null && pawn.Spawned && !pawn.Dead &&
                !creditCaster.Dead && !string.IsNullOrEmpty(creditEssence) &&
                (!trainOnlyAgainstHostile || pawn.HostileTo(creditCaster)))
            {
                // Buffs accrue one unit per 60 ticks of real, active duration.
                // Recasting before duration passes gives no instant training.
                if (++benefitTicks >= 60)
                {
                    benefitTicks = 0;
                    Hediff_EssenceUser.ForPawn(creditCaster)?.AddAbilityWork(
                        creditEssence, creditSlot, 1f);
                }
            }
            if (--ticksRemaining == 0 && pawn?.health != null)
                pawn.health.RemoveHediff(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ebTicksRemaining", 0);
            Scribe_References.Look(ref creditCaster, "ebCreditCaster");
            Scribe_Values.Look(ref creditEssence, "ebCreditEssence", "");
            Scribe_Values.Look(ref creditSlot, "ebCreditSlot", -1);
            Scribe_Values.Look(ref benefitTicks, "ebBenefitTicks", 0);
            Scribe_Values.Look(ref trainOnlyAgainstHostile, "ebHostileTraining", false);
        }
    }
}
