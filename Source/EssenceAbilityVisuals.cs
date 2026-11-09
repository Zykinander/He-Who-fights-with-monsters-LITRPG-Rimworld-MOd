using RimWorld;
using Verse;

namespace Essencebound
{
    // First-pass runtime VFX. Each power uses its own bright, transparent
    // 128-pixel sigil which fades and grows as a cast and result cue.
    // No combat mechanics, damage or XP are modified by these visuals.
    public static class EssenceAbilityVisuals
    {
        public static void OnSuccessfulCast(Pawn caster, EssencePowerDef power,
            LocalTargetInfo target)
        {
            if (caster == null || !caster.Spawned || caster.Map == null || power == null)
                return;
            FleckDef fleck = FindSigil(power.defName);
            if (fleck == null) return;
            Map map = caster.Map;
            Emit(fleck, caster.Position, map, 0.60f);
            IntVec3 destination = target.Cell;
            if (target.Thing is Pawn pawn) destination = pawn.Position;
            if (destination.IsValid && destination.InBounds(map) &&
                destination != caster.Position)
                Emit(fleck, destination, map, 0.95f);
        }

        // Delayed fields, summons and channeled spells keep their per-power
        // identity rather than using the same generic smoke effect.
        public static void OnFieldPulse(EssenceWorldEffect effect, Map map)
        {
            if (effect == null || map == null || string.IsNullOrEmpty(effect.powerDefName))
                return;
            FleckDef fleck = FindSigil(effect.powerDefName);
            if (fleck == null) return;
            if (effect.kind == "summon")
            {
                if (effect.summonedPawn != null &&
                    effect.summonedPawn.Spawned &&
                    effect.summonedPawn.Map == map)
                    Emit(fleck, effect.summonedPawn.Position, map, 0.48f);
            }
            else if (effect.center.IsValid && effect.center.InBounds(map))
                Emit(fleck, effect.center, map,
                    effect.kind == "delayedBlast" || effect.kind == "hazardZone" ?
                        1.18f : 0.82f);
        }

        private static FleckDef FindSigil(string powerName)
        {
            if (string.IsNullOrEmpty(powerName) ||
                !powerName.StartsWith("EB_Power_")) return null;
            return DefDatabase<FleckDef>.GetNamedSilentFail(
                "EB_AbilitySigil_" + powerName.Substring("EB_Power_".Length));
        }

        private static void Emit(FleckDef def, IntVec3 cell, Map map, float size)
        {
            if (!cell.IsValid || map == null || !cell.InBounds(map) ||
                map.fogGrid.IsFogged(cell)) return;
            FleckMaker.Static(cell.ToVector3Shifted(), map, def, size);
        }
    }
}
