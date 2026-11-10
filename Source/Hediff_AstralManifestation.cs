using Verse;

namespace Essencebound
{
    // Marker persists on the dead pawn inside its Corpse for one-time looting.
    // Existing manifestations from older saves have a blank archetype and
    // retain their old behavior. New custom kinds carry their own spell timer.
    public sealed class Hediff_AstralManifestation : HediffWithComps
    {
        public float magicDensity = 0.5f;
        // Major monster tier (Iron 0 .. Diamond 4) persists with its corpse.
        public int rankIndex;
        public string archetypeId = "";
        private int nextPulseTick;

        public void AssignArchetype(string id)
        {
            archetypeId = id ?? "";
            var kind = EssenceMonsterArchetypes.ForId(archetypeId);
            nextPulseTick = Find.TickManager.TicksGame +
                (kind?.pulseInterval ?? 600);
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn == null || pawn.Dead || !pawn.Spawned ||
                string.IsNullOrEmpty(archetypeId) || Find.TickManager == null)
                return;
            var type = EssenceMonsterArchetypes.ForId(archetypeId);
            if (type == null) return;
            int now = Find.TickManager.TicksGame;
            // A saved default of zero (from earlier builds) simply starts a
            // fresh cooldown. Avoid burst loops on loading or long tick skips.
            if (nextPulseTick <= 0)
                nextPulseTick = now + type.pulseInterval;
            if (now < nextPulseTick) return;
            nextPulseTick = now + type.pulseInterval;
            EssenceMonsterArchetypes.TriggerPulse(this);
        }

        public override string TipStringExtra
        {
            get
            {
                var type = EssenceMonsterArchetypes.ForId(archetypeId);
                return type == null
                    ? "Magical creature released by an astral tear."
                    : "Astral monster: " + type.name +
                      "\nSignature power: " + type.feature +
                      "\nCreature rank: " + EssenceProgression.RankLabel(rankIndex) +
                      "\nEssence crystal and awakening-stone drop chances still follow rank.";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref magicDensity, "ebMagicDensityAtBirth", 0.5f);
            Scribe_Values.Look(ref rankIndex, "ebMonsterRankIndex", 0);
            Scribe_Values.Look(ref archetypeId, "ebMonsterArchetype", "");
            Scribe_Values.Look(ref nextPulseTick, "ebMonsterNextPulse", 0);
            archetypeId ??= "";
            rankIndex = System.Math.Max(0, System.Math.Min(4, rankIndex));
        }
    }
}
