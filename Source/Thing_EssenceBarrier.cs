using System;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Temporary placed obstruction. Destroying or expiring it restores access.
    public sealed class Thing_EssenceBarrier : Building
    {
        private int remainingTicks = 720;

        public void Initialize(int duration)
        {
            remainingTicks = Math.Max(60, duration);
        }

        protected override void Tick()
        {
            base.Tick();
            if (--remainingTicks <= 0 && !Destroyed)
                Destroy(DestroyMode.Vanish);
        }

        public override string GetInspectString()
        {
            return "Temporary essence barrier. Remaining: " +
                Math.Max(0, remainingTicks / 60) + " seconds.";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "ebBarrierTicks", 720);
        }
    }
}
