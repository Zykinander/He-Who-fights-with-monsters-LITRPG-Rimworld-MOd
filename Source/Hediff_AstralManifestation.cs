using Verse;

namespace Essencebound
{
    // The marker persists on the dead pawn inside its Corpse for one-time
    // looting. Older manifestations without an archetype keep their old rank
    // and do not receive new spells. A charged attack's CELL and deadline are
    // saved so loading mid-cast cannot re-target or instantly double-cast.
    public sealed class Hediff_AstralManifestation : HediffWithComps
    {
        public float magicDensity = 0.5f;
        public int rankIndex;
        public string archetypeId = "";

        private int nextPulseTick;
        private int chargeEndTick;
        private IntVec3 chargedCell = IntVec3.Invalid;

        public IntVec3 ChargedCell => chargedCell;

        public void AssignArchetype(string id)
        {
            archetypeId = id ?? "";
            var kind = EssenceMonsterArchetypes.ForId(archetypeId);
            nextPulseTick = Find.TickManager.TicksGame +
                (kind?.pulseInterval ?? 600);
            chargeEndTick = 0;
            chargedCell = IntVec3.Invalid;
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn == null || pawn.Dead || !pawn.Spawned ||
                pawn.Downed || string.IsNullOrEmpty(archetypeId) ||
                Find.TickManager == null)
                return;

            var type = EssenceMonsterArchetypes.ForId(archetypeId);
            if (type == null) return;
            int now = Find.TickManager.TicksGame;

            if (chargeEndTick > 0)
            {
                // A fixed warning at the selected cell gives defenders an
                // opportunity to escape. Once charged, never re-target.
                if (now < chargeEndTick)
                {
                    if (now % 30 == 0)
                        EssenceMonsterArchetypes.DrawSpellEffect(pawn,
                            chargedCell, type, false);
                    return;
                }
                if (chargedCell.IsValid)
                    EssenceMonsterArchetypes.TriggerPulse(this);
                chargeEndTick = 0;
                chargedCell = IntVec3.Invalid;
                return;
            }

            if (nextPulseTick <= 0)
                nextPulseTick = now + type.pulseInterval;
            if (now < nextPulseTick) return;
            nextPulseTick = now + type.pulseInterval;

            if (EssenceMonsterArchetypes.TryChoosePulseCenter(this,
                out IntVec3 center))
            {
                chargedCell = center;
                chargeEndTick = now + type.windupTicks;
                EssenceMonsterArchetypes.DrawSpellEffect(pawn,
                    chargedCell, type, false);
            }
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
                      "\nSpell warning: " + type.windupTicks + " ticks before impact" +
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
            Scribe_Values.Look(ref chargeEndTick, "ebMonsterChargeEndTick", 0);
            Scribe_Values.Look(ref chargedCell, "ebMonsterChargedCell", IntVec3.Invalid);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                archetypeId ??= "";
                rankIndex = System.Math.Max(0, System.Math.Min(4, rankIndex));
                if (chargeEndTick <= 0 || !chargedCell.IsValid)
                {
                    chargeEndTick = 0;
                    chargedCell = IntVec3.Invalid;
                }
            }
        }
    }
}
