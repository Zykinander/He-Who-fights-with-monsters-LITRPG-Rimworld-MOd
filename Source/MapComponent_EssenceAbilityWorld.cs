using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Persisted map-wide spell effects: telegraphed attacks, healing fields,
    // defensive auras, hazard zones, and temporary manifested companions.
    public sealed class EssenceWorldEffect : IExposable
    {
        public string kind = "";
        public IntVec3 center = IntVec3.Invalid;
        public float radius;
        public float magnitude;
        public string damageDef = "";
        public string buffDef = "";
        public int expireTick;
        public int nextPulseTick;
        public int pulseTicks = 120;
        public Pawn caster;
        public string essenceName = "";
        public int slotIndex = -1;
        public Pawn summonedPawn;

        public void ExposeData()
        {
            Scribe_Values.Look(ref kind, "ebFieldKind", "");
            Scribe_Values.Look(ref center, "ebFieldCenter", IntVec3.Invalid);
            Scribe_Values.Look(ref radius, "ebFieldRadius", 0f);
            Scribe_Values.Look(ref magnitude, "ebFieldMagnitude", 0f);
            Scribe_Values.Look(ref damageDef, "ebFieldDamageDef", "");
            Scribe_Values.Look(ref buffDef, "ebFieldBuffDef", "");
            Scribe_Values.Look(ref expireTick, "ebFieldExpiry", 0);
            Scribe_Values.Look(ref nextPulseTick, "ebFieldNextPulse", 0);
            Scribe_Values.Look(ref pulseTicks, "ebFieldPulsePeriod", 120);
            Scribe_References.Look(ref caster, "ebFieldCaster");
            Scribe_Values.Look(ref essenceName, "ebFieldEssence", "");
            Scribe_Values.Look(ref slotIndex, "ebFieldSlot", -1);
            Scribe_References.Look(ref summonedPawn, "ebSummonedPawn");
        }
    }

    public sealed class MapComponent_EssenceAbilityWorld : MapComponent
    {
        private List<EssenceWorldEffect> effects = new List<EssenceWorldEffect>();
        public MapComponent_EssenceAbilityWorld(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref effects, "ebAbilityWorldEffects", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && effects == null)
                effects = new List<EssenceWorldEffect>();
        }

        public void Enqueue(string kind, Pawn caster, EssenceAbilitySlot slot,
            IntVec3 center, float radius, float magnitude, int duration,
            string damageDef = "", string buffDef = "", int delay = 120)
        {
            if (effects == null) effects = new List<EssenceWorldEffect>();
            int now = Find.TickManager.TicksGame;
            effects.Add(new EssenceWorldEffect
            {
                kind = kind, caster = caster,
                essenceName = slot?.essenceDefName ?? "",
                slotIndex = slot?.index ?? -1,
                center = center, radius = radius, magnitude = magnitude,
                expireTick = now + Math.Max(1, duration),
                nextPulseTick = now + Math.Max(1, delay),
                pulseTicks = kind == "powerAura" || kind == "wardDome" ? 180 : 120,
                damageDef = damageDef, buffDef = buffDef
            });
        }

        public void TrackSummon(Pawn pawn, Pawn owner, EssenceAbilitySlot slot, int duration)
        {
            if (effects == null) effects = new List<EssenceWorldEffect>();
            effects.Add(new EssenceWorldEffect
            {
                kind = "summon", caster = owner, summonedPawn = pawn,
                essenceName = slot?.essenceDefName ?? "",
                slotIndex = slot?.index ?? -1,
                expireTick = Find.TickManager.TicksGame + Math.Max(60, duration),
                nextPulseTick = Find.TickManager.TicksGame + 180,
                pulseTicks = 180
            });
        }

        public List<Pawn> SummonsOf(Pawn owner)
        {
            return effects == null ? new List<Pawn>() :
                effects.Where(e => e != null && e.kind == "summon" &&
                    e.caster == owner && e.summonedPawn != null &&
                    e.summonedPawn.Spawned && !e.summonedPawn.Dead)
                    .Select(e => e.summonedPawn).ToList();
        }

        public List<Pawn> AlliedSummons(Pawn owner)
        {
            return effects == null || owner?.Faction == null ? new List<Pawn>() :
                effects.Where(e => e != null && e.kind == "summon" &&
                    e.caster != null && e.caster.Faction == owner.Faction &&
                    e.summonedPawn != null && !e.summonedPawn.Dead &&
                    e.summonedPawn.Spawned && e.summonedPawn.Map == map)
                    .Select(e => e.summonedPawn).ToList();
        }

        public override void MapComponentTick()
        {
            if (effects == null || effects.Count == 0 ||
                Find.TickManager.TicksGame % 30 != 0) return;
            int now = Find.TickManager.TicksGame;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                EssenceWorldEffect effect = effects[i];
                if (effect == null) { effects.RemoveAt(i); continue; }
                if (effect.kind == "summon")
                {
                    if (now >= effect.expireTick ||
                        effect.summonedPawn == null || effect.summonedPawn.Dead ||
                        !effect.summonedPawn.Spawned || effect.summonedPawn.Map != map)
                    {
                        if (effect.summonedPawn != null && effect.summonedPawn.Spawned &&
                            !effect.summonedPawn.Dead)
                            effect.summonedPawn.Destroy(DestroyMode.Vanish);
                        effects.RemoveAt(i);
                        continue;
                    }
                    if (now >= effect.nextPulseTick)
                    {
                        // Only credit active summoned assistance while enemies
                        // are nearby, rather than rewarding idle companions.
                        if (map.mapPawns.AllPawnsSpawned.Any(p => p != null && !p.Dead &&
                            effect.caster != null && p.HostileTo(effect.caster) &&
                            p.Position.DistanceTo(effect.summonedPawn.Position) <= 10f))
                            Credit(effect, 1f);
                        effect.nextPulseTick = now + effect.pulseTicks;
                    }
                    continue;
                }
                if (now >= effect.nextPulseTick && now <= effect.expireTick)
                {
                    Pulse(effect);
                    effect.nextPulseTick = now + Math.Max(30, effect.pulseTicks);
                    if (effect.kind == "delayedBlast")
                        effect.expireTick = now;
                }
                if (now >= effect.expireTick)
                    effects.RemoveAt(i);
            }
        }

        private void Pulse(EssenceWorldEffect effect)
        {
            if (effect.caster == null || effect.caster.Dead ||
                effect.caster.Map != map || !effect.center.InBounds(map)) return;
            var pawns = map.mapPawns.AllPawnsSpawned.Where(p =>
                p != null && !p.Dead && p.Position.DistanceTo(effect.center) <= effect.radius &&
                GenSight.LineOfSight(effect.center, p.Position, map)).ToList();
            float work = 0f;
            switch (effect.kind)
            {
                case "delayedBlast":
                case "hazardZone":
                    foreach (Pawn enemy in pawns.Where(p => p.HostileTo(effect.caster)))
                    {
                        if (EssencePowerExecutor.FieldDamage(effect.caster, enemy,
                            effect.damageDef, effect.magnitude, out float actual))
                            work += actual;
                    }
                    break;
                case "healingZone":
                    foreach (Pawn ally in pawns.Where(p => !p.HostileTo(effect.caster)))
                    {
                        if (EssencePowerExecutor.FieldHealing(ally, effect.magnitude,
                            out float actual)) work += actual;
                    }
                    break;
                case "powerAura":
                case "wardDome":
                    foreach (Pawn ally in pawns.Where(p => !p.HostileTo(effect.caster)))
                        EssencePowerExecutor.FieldBuff(ally, effect.buffDef, 240,
                            effect.caster, effect.essenceName, effect.slotIndex);
                    break;
            }
            Credit(effect, work);
        }

        private static void Credit(EssenceWorldEffect effect, float amount)
        {
            if (amount <= 0f || effect.caster == null || effect.caster.Dead ||
                string.IsNullOrEmpty(effect.essenceName) || effect.slotIndex < 0) return;
            Hediff_EssenceUser.ForPawn(effect.caster)?.AddAbilityWork(
                effect.essenceName, effect.slotIndex, amount);
        }
    }
}
