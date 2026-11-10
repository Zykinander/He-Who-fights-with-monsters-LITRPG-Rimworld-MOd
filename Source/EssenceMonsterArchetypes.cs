using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// The five astral fighters share the game's ordinary melee/manhunter AI,
    /// but each charges and resolves a separate magical combat tactic. Spells
    /// target a fixed, warned map cell so moving away can avoid an attack.
    /// </summary>
    public sealed class EssenceMonsterArchetype
    {
        public string id;
        public string kindDef;
        public string name;
        public string feature;
        public string combatStyle;
        public int rank;
        public int pulseInterval;
        public int windupTicks;
        public float range;
        public float radius;
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
                name = "Glassmote Scarab", feature = "Dazzling static burst",
                combatStyle = "Disrupts the nearest visible colonist and others nearby.",
                rank = 0, pulseInterval = 480, windupTicks = 75,
                range = 5f, radius = 2f, debuff = "EB_Debuff_Static",
                impact = 0f,
                favoredEssences = new[] { "EB_Glass", "EB_Crystal", "EB_Lightning" }
            },
            new EssenceMonsterArchetype
            {
                id = "emberfang", kindDef = "EB_Monster_Emberfang",
                name = "Emberfang Stalker", feature = "Hunting ember blast",
                combatStyle = "Pursues injured colonists with a compact heat blast.",
                rank = 1, pulseInterval = 390, windupTicks = 65,
                range = 6f, radius = 1.6f, impact = 4f, damageDef = "Flame",
                favoredEssences = new[] { "EB_Fire", "EB_Wolf", "EB_Smoke" }
            },
            new EssenceMonsterArchetype
            {
                id = "duskweaver", kindDef = "EB_Monster_Duskweaver",
                name = "Duskweaver", feature = "Binding astral web",
                combatStyle = "Targets the farthest visible colonist with a broad slowing web.",
                rank = 2, pulseInterval = 360, windupTicks = 110,
                range = 7f, radius = 2.3f, impact = 0f,
                debuff = "EB_Debuff_Webbed",
                favoredEssences = new[] { "EB_Spider", "EB_Thread", "EB_Dark" }
            },
            new EssenceMonsterArchetype
            {
                id = "stormhide", kindDef = "EB_Monster_Stormhide",
                name = "Stormhide Colossus", feature = "Point-blank thunder shockwave",
                combatStyle = "Unleashes a large disruption wave around its position at wind-up.",
                rank = 3, pulseInterval = 330, windupTicks = 105,
                range = 5f, radius = 4f, impact = 6f,
                damageDef = "Blunt", debuff = "EB_Debuff_Static",
                favoredEssences = new[] { "EB_Lightning", "EB_Earth", "EB_Might" }
            },
            new EssenceMonsterArchetype
            {
                id = "sovereign", kindDef = "EB_Monster_PrismaticSovereign",
                name = "Prismatic Sovereign", feature = "Prismatic ward and renewal",
                combatStyle = "Wards and heals nearby astral allies while striking a target area.",
                rank = 4, pulseInterval = 300, windupTicks = 135,
                range = 7f, radius = 2f, impact = 7f, damageDef = "Blunt",
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
            return marker == null ? null : ForId(marker.archetypeId);
        }

        public static IEnumerable<string> FavoredEssences(string id) =>
            ForId(id)?.favoredEssences ?? Array.Empty<string>();

        // The selection strategy is deliberately different for each archetype:
        // Glassmote interrupts the nearest colonist; Emberfang hunts an injured
        // colonist; Duskweaver pins down the furthest visible colonist; Stormhide
        // charges a centered close-range shockwave; Sovereign supports allies.
        public static bool TryChoosePulseCenter(Hediff_AstralManifestation marker,
            out IntVec3 center)
        {
            center = IntVec3.Invalid;
            Pawn caster = marker?.pawn;
            var type = ForId(marker?.archetypeId);
            if (caster == null || type == null || !caster.Spawned ||
                caster.Dead || caster.Downed || caster.Map == null)
                return false;

            var options = caster.Map.mapPawns.FreeColonistsSpawned
                .Where(p => p != null && p.Spawned && !p.Dead && !p.Downed &&
                    p.Position.DistanceTo(caster.Position) <= type.range &&
                    GenSight.LineOfSight(caster.Position, p.Position, caster.Map)).ToList();

            if (type.id == "sovereign" && options.Count == 0)
            {
                if (!HasInjuredAstralAlly(caster, 7f)) return false;
                center = caster.Position; // A support-only pulse.
                return true;
            }
            if (options.Count == 0) return false;

            Pawn target;
            switch (type.id)
            {
                case "emberfang":
                    target = options.OrderByDescending(p =>
                        p.health?.hediffSet?.hediffs.OfType<Hediff_Injury>()
                            .Sum(h => h.Severity) ?? 0f)
                        .ThenBy(p => p.Position.DistanceTo(caster.Position))
                        .First();
                    break;
                case "duskweaver":
                    target = options.OrderByDescending(p =>
                        p.Position.DistanceTo(caster.Position)).First();
                    break;
                default:
                    target = options.OrderBy(p =>
                        p.Position.DistanceTo(caster.Position)).First();
                    break;
            }

            center = type.id == "stormhide" ? caster.Position : target.Position;
            return true;
        }

        // Executed only AFTER the charge has resolved. Impact remains at the
        // telegraphed position, rather than magically tracking a fleeing target.
        public static void TriggerPulse(Hediff_AstralManifestation manifestation)
        {
            Pawn caster = manifestation?.pawn;
            var type = ForId(manifestation?.archetypeId);
            if (caster == null || type == null || !caster.Spawned ||
                caster.Dead || caster.Downed || caster.Map == null) return;

            IntVec3 impactCell = manifestation.ChargedCell;
            if (!impactCell.IsValid || !impactCell.InBounds(caster.Map)) return;

            if (type.id == "sovereign")
            {
                RenewNearbyAstralCreatures(caster, 4f, 7f);
                WardNearbyAstralCreatures(caster, 330, 7f);
            }

            // Player colonists only. The spell does not injure normal wildlife,
            // companion animals, or other astral monsters caught in its radius.
            var affected = caster.Map.mapPawns.FreeColonistsSpawned
                .Where(p => p != null && p.Spawned && !p.Dead && !p.Downed &&
                    p.Position.DistanceTo(impactCell) <= type.radius &&
                    GenSight.LineOfSight(impactCell, p.Position, caster.Map))
                .ToList();

            foreach (Pawn target in affected)
            {
                if (!string.IsNullOrEmpty(type.debuff))
                    ApplyDebuff(target, type.debuff, type.id == "duskweaver" ? 600 : 330);

                if (type.impact > 0f)
                {
                    DamageDef damage = DefDatabase<DamageDef>
                        .GetNamedSilentFail(type.damageDef);
                    if (damage != null)
                        target.TakeDamage(new DamageInfo(damage, type.impact,
                            0f, -1f, caster));
                }
            }

            DrawSpellEffect(caster, impactCell, type, true);
        }

        private static void ApplyDebuff(Pawn target, string name, int duration)
        {
            if (target?.health?.hediffSet == null) return;
            HediffDef def = DefDatabase<HediffDef>.GetNamedSilentFail(name);
            if (def == null) return;
            var status = target.health.hediffSet.GetFirstHediffOfDef(def)
                as Hediff_EssenceTimed;
            if (status == null)
            {
                status = HediffMaker.MakeHediff(def, target) as Hediff_EssenceTimed;
                if (status != null) target.health.AddHediff(status);
            }
            // Null caster: monsters cannot earn the colony's ability-training XP.
            status?.Refresh(duration, null, null);
        }

        private static bool HasInjuredAstralAlly(Pawn source, float radius)
        {
            return source.Map.mapPawns.AllPawnsSpawned.Any(p =>
                p != null && p.Spawned && !p.Dead &&
                p.Position.DistanceTo(source.Position) <= radius &&
                p.health?.hediffSet?.hediffs.OfType<Hediff_AstralManifestation>()
                    .Any() == true &&
                p.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                    .Any(w => !w.IsPermanent()));
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

        private static void WardNearbyAstralCreatures(Pawn source,
            int duration, float radius)
        {
            HediffDef ward = DefDatabase<HediffDef>.GetNamedSilentFail(
                "EB_Buff_GuardianWard");
            if (ward == null) return;

            foreach (Pawn ally in source.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != null && p.Spawned && !p.Dead &&
                    p.Position.DistanceTo(source.Position) <= radius &&
                    p.health?.hediffSet?.hediffs
                        ?.OfType<Hediff_AstralManifestation>().Any() == true)
                .Take(3))
            {
                var existing = ally.health.hediffSet
                    .GetFirstHediffOfDef(ward) as Hediff_EssenceTimed;
                if (existing == null)
                {
                    existing = HediffMaker.MakeHediff(ward, ally)
                        as Hediff_EssenceTimed;
                    if (existing != null) ally.health.AddHediff(existing);
                }
                existing?.Refresh(duration, null, null);
            }
        }

        // Colored dust puffs preview the strike position, radius, and elemental
        // identity. The same position is used on impact for fair dodging.
        // Visuals do not deal damage or create fires on their own.
        public static void DrawSpellEffect(Pawn caster, IntVec3 center,
            EssenceMonsterArchetype type, bool impact)
        {
            if (caster?.Map == null || type == null ||
                !center.IsValid || !center.InBounds(caster.Map)) return;

            Color color;
            switch (type.id)
            {
                case "glassmote": color = new Color(0.28f, 0.92f, 1f); break;
                case "emberfang": color = new Color(1f, 0.35f, 0.15f); break;
                case "duskweaver": color = new Color(0.62f, 0.37f, 0.98f); break;
                case "stormhide": color = new Color(0.36f, 0.68f, 1f); break;
                default: color = new Color(0.92f, 0.73f, 1f); break;
            }
            Vector3 midpoint = center.ToVector3Shifted();
            int count = impact ? 10 : 5;
            for (int i = 0; i < count; i++)
            {
                float angle = Mathf.PI * 2f * i / count;
                float radius = type.radius * (impact ? 0.7f : 0.95f);
                Vector3 point = midpoint + new Vector3(Mathf.Cos(angle) * radius,
                    0f, Mathf.Sin(angle) * radius);
                FleckMaker.ThrowDustPuffThick(point, caster.Map,
                    impact ? 0.85f : 0.45f, color);
            }
        }
    }
}
