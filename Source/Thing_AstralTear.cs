using Verse;

namespace Essencebound
{
    // Short-lived visible rupture. Its density and remaining time persist in saves.
    public sealed class Thing_AstralTear : ThingWithComps
    {
        private int ticksUntilResolution = 900;
        private float openingDensity = 0.5f;
        public float OpeningDensity => openingDensity;

        public void Initialize(float density)
        {
            openingDensity = density;
            ticksUntilResolution = 900;
        }

        protected override void Tick()
        {
            base.Tick();
            if (--ticksUntilResolution > 0 || Map == null) return;
            Map.GetComponent<MapComponent_EssenceWorld>()?.ResolveTear(this);
            if (!Destroyed) Destroy(DestroyMode.Vanish);
        }

        public override string GetInspectString()
        {
            return "Tear in astral space. Magic density: " +
                (openingDensity * 100f).ToString("0") +
                "%. Opens in " + System.Math.Max(0, ticksUntilResolution / 60) + " seconds.";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksUntilResolution, "ebTearCountdown", 900);
            Scribe_Values.Look(ref openingDensity, "ebTearDensity", 0.5f);
        }
    }
}
