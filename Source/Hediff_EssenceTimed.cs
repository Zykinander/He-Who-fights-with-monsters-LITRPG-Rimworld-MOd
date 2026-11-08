using Verse;

namespace Essencebound
{
    // Simple duration-managed status effect; durations persist across save/load.
    public sealed class Hediff_EssenceTimed : HediffWithComps
    {
        private int ticksRemaining;

        public void Refresh(int durationTicks)
        {
            ticksRemaining = durationTicks > 0 ? durationTicks : 1;
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksRemaining > 0 && --ticksRemaining == 0 && pawn?.health != null)
                pawn.health.RemoveHediff(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ebTicksRemaining", 0);
        }
    }
}
