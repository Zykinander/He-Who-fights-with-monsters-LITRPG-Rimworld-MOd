using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// First wave of original Essencebound monsters. Vanilla animal races
    /// supply safe body and corpse behavior; each custom PawnKind supplies a
    /// name and tinted silhouette. Powers live on the astral marker, not on
    /// every ordinary animal of that race.
    /// </summary>
    public sealed class EssenceMonsterArchetype
    {
        public string id;
        public string kindDef;
        public string name;
        public string feature;
        public int rank;
        public int pulseInterval;
        public float range;
        public float impact;
        public string debuff;
        public string damageDef;
        public string[] favoredEssences;
    }

    public static class EssenceMonsterArchetypes
    {
        private static readonly EssenceMonsterArchetype[] Kinds =
        {
            new EssenceMonsterArchetype
            {
                id = "glassmote", kindDef = "EB_Monster_Glassmote",
                name = "Glassmote Scarab", feature = "Dazzling static",
                rank = 0, pulseInterval = 480, range = 4f,
                debuff = "EB_Debuff_Static", impact = 0f,
                favoredEssences = new[] { "EB_Glass", "EB_Crystal", "EB_Lightning" }
            },
            new EssenceMonsterArchetype
            {
                id = "emberfang", kindDef = "EB_Monster_Emberfang",
                name = "Emberfang Stalker", feature = "Ember pulse",
                rank = 1, pulseInterval = 390, range = 5f, impact = 4f,
                damageDef = "Flame",
                favoredEssences = new[] { "EB_Fire", "EB_Wolf", "EB_Smoke" }
            },
            new EssenceMonsterArchetype
            {
                id = "duskweaver", kindDef = "EB_Monster_Duskweaver",
                name = "Duskweaver", feature = "Astral binding web",
                rank = 2, pulseInterval = 360, range = 6f, impact = 0f,
                debuff = "EB_Debuff_Webbed",
                favoredEssences = new[] { "EB_Spider", "EB_Thread", "EB_Dark" }
            },
            new EssenceMonsterArchetype
            {
                id = "stormhide", kindDef = "EB_Monster_Stormhide",
                name = "Stormhide Colossus", feature = "Storm surge",
                rank = 3, pulseInterval = 330, range = 6f, impact = 6f,
                damageDef = "Blunt", debuff = "EB_Debuff_Static",
                favoredEssences = new[] { "EB_Lightning", "EB_Earth", "EB_Might" }
            },
            new EssenceMonsterArchetype
            {
                id = "sovereign", kindDef = "EB_Monster_PrismaticSovereign",
                name = "Prismatic Sovereign", feature = "Prismatic renewal and pulse",
                rank = 4, pulseInterval = 300, range = 7f, impact = 7f,
                damageDef = "Blunt",
                favoredEssences = new[] { "EB_Light", "EB_Star", "EB_Mirror" }
            }
        };

        public static IReadOnlyList<EssenceMonsterArchetype> All => Kinds;

        public static EssenceMonsterArchetype ForId(string id) =>
            Kinds.FirstOrDefault(kind => string.Equals(kind.id, id, StringComparison.Ordinal));

        public static EssenceMonsterArchetype ForRank(int rank) =>
            Kinds.FirstOrDefault(kind => kind.rank == rank);

        public static EssenceMonsterArchetype ForPawn(Pawn pawn)
        {
            if (pawn == null) return null;
            var marker = pawn.health?.hediffSet?.hediffs
                ?.OfType<Hediff_AstralManifestation>().FirstOrDefault();
            if (marker != null && !string.IsNullOrEmpty(marker.archetypeId))
                return ForId(marker.archetypeId);
            return null;
        }

        public static IEnumerable<string> FavoredEssences(string id) =>
            ForId(id)?.favoredEssences ?? Array.Empty<string>();

        public static void TriggerPulse(Hediff_AstralManifestation manifestation)
        {
            Pawn caster = manifestation?.pawn;
            EssenceMonsterArchetype type = ForId(manifestation?.archetypeId);
            if (caster == null || type == null || !caster.Spawned ||
                caster.Dead || caster.Downed || caster.Map == null) return;

            Map map = caster.Map;
            // A magical monster spawned by a tear seeks an active threat.
            // Restrict the prototype pulse to player-owned living colonists
            // in actual sight, not downed pawns or the colony's pets.
            Pawn target = map.mapPawns.FreeColonistsSpawned
                .Where(p => p != null && p.Spawned && !p.Dead && !p.Downed &&
                    p.Position.DistanceTo(caster.Position) <= type.range &&
                    GenSight.LineOfSight(caster.Position, p.Position, map))
                .OrderBy(p => p.Position.DistanceTo(caster.Position))
                .FirstOrDefault();

            if (type.id == "sovereign")
                RenewNearbyAstralCreatures(caster, 4f, 7f);

            if (target == null || target.health == null) return;

            if (!string.IsNullOrEmpty(type.debuff))
            {
                var def = DefDatabase<HediffDef>.GetNamedSilentFail(type.debuff);
                if (def != null)
                {
                    var active = target.health.hediffSet.GetFirstHediffOfDef(def)
                        as Hediff_EssenceTimed;
                    if (active == null)
                    {
                        active = HediffMaker.MakeHediff(def, target) as Hediff_EssenceTimed;
                        if (active != null) target.health.AddHediff(active);
                    }
                    active?.Refresh(420, null, null);
                }
            }

            if (type.impact > 0f)
            {
                var damage = DefDatabase<DamageDef>.GetNamedSilentFail(type.damageDef);
                if (damage != null)
                    target.TakeDamage(new DamageInfo(damage, type.impact, 0f, -1f, caster));
            }
        }

        private static void RenewNearbyAstralCreatures(Pawn source, float heal, float radius)
        {
            var ally = source.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != null && p.Spawned && !p.Dead &&
                    p.Position.DistanceTo(source.Position) <= radius &&
                    p.health?.hediffSet?.hediffs
                        ?.OfType<Hediff_AstralManifestation>().Any() == true)
                .Select(p => new
                {
                    pawn = p,
                    wound = p.health.hediffSet.hediffs
                        .OfType<Hediff_Injury>()
                        .Where(w => !w.IsPermanent())
                        .OrderByDescending(w => w.Severity).FirstOrDefault()
                })
                .Where(x => x.wound != null)
                .OrderByDescending(x => x.wound.Severity)
                .FirstOrDefault();

            ally?.wound.Heal(heal);
        }
    }
}
