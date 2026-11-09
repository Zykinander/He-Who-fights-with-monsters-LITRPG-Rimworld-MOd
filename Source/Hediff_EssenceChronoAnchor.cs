using System;
using Verse;

namespace Essencebound
{
    // The anchor persists with the pawn and expires without a second cast.
    public sealed class Hediff_EssenceChronoAnchor : HediffWithComps
    {
        public IntVec3 anchor = IntVec3.Invalid;
        private int remainingTicks;

        public void SetAnchor(IntVec3 cell, int duration)
        {
            anchor = cell;
            remainingTicks = Math.Max(1, duration);
        }

        public bool HasAnchor => remainingTicks > 0 && anchor.IsValid;

        public override void Tick()
        {
            base.Tick();
            if (remainingTicks > 0 && --remainingTicks == 0 && pawn?.health != null)
                pawn.health.RemoveHediff(this);
        }

        public override string LabelInBrackets => HasAnchor ?
            "anchored (" + Math.Max(0, remainingTicks / 60) + "s)" : "expended";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref anchor, "ebChronoAnchorCell", IntVec3.Invalid);
            Scribe_Values.Look(ref remainingTicks, "ebChronoAnchorTicks", 0);
        }
    }
}
