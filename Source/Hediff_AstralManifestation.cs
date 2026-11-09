using Verse;

namespace Essencebound
{
    // Marker persists on the dead pawn inside its Corpse for one-time looting.
    public sealed class Hediff_AstralManifestation : HediffWithComps
    {
        public float magicDensity = 0.5f;
        // Major monster tier (Iron 0 .. Diamond 4) persists with its corpse.
        public int rankIndex;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref magicDensity, "ebMagicDensityAtBirth", 0.5f);
            Scribe_Values.Look(ref rankIndex, "ebMonsterRankIndex", 0);
            rankIndex = System.Math.Max(0, System.Math.Min(4, rankIndex));
        }
    }
}
