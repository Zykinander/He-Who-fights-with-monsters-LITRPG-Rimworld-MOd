using Verse;

namespace Essencebound
{
    // Marker persists on the dead pawn inside its Corpse for one-time looting.
    public sealed class Hediff_AstralManifestation : HediffWithComps
    {
        public float magicDensity = 0.5f;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref magicDensity, "ebMagicDensityAtBirth", 0.5f);
        }
    }
}
