using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;
using Verse;

namespace Essencebound
{
    // Each advanced technique is grounded in a real action (a reposition,
    // successful item pull, applied status, spawned temporary pawn or a field
    // whose subsequent ticks perform meaningful healing/damage).
    public static class EssenceAdvancedAbilities
    {
        public static bool CastPawn(Pawn caster, EssencePowerDef power, Pawn target,
            EssenceAbilitySlot slot, float powerScale, float recoveryScale,
            int duration, out string message, out float work)
        {
            work = 0f;
            var world = caster.Map.GetComponent<MapComponent_EssenceAbilityWorld>();
            switch (power.effectKind)
            {
                case "delayedBlast":
                    if (target == caster || !target.HostileTo(caster))
                    { message = "Select a hostile pawn to mark with the delayed blast."; return false; }
                    world.Enqueue("delayedBlast", caster, slot, target.Position,
                        power.areaRadius, power.magnitude * powerScale, 300,
                        power.damageDefName, delay: 180);
                    message = "An astral comet is descending on the target's current position.";
                    return true;
                case "reflect":
                    var reflectionDef = DefDatabase<HediffDef>
                        .GetNamedSilentFail("EB_Buff_PrismaticDeflection");
                    if (reflectionDef == null)
                    { message = "Prismatic reflection status is missing."; return false; }
                    var reflection = caster.health.hediffSet.GetFirstHediffOfDef(reflectionDef)
                        as Hediff_EssenceReflection;
                    if (reflection == null)
                    {
                        reflection = HediffMaker.MakeHediff(reflectionDef, caster)
                            as Hediff_EssenceReflection;
                        if (reflection == null)
                        { message = "Could not activate the reflective ward."; return false; }
                        caster.health.AddHediff(reflection);
                    }
                    reflection.Refresh(duration, caster, slot);
                    message = "Ranged pawn attacks are partially deflected back to the attacker.";
                    return true;
                case "healingZone":
                    world.Enqueue("healingZone", caster, slot, caster.Position,
                        power.areaRadius, power.magnitude * recoveryScale, duration);
                    message = "Lotus sanctuary established here; wounds actually healed will train it.";
                    return true;
                case "returnTeleport":
                    return UseChronoAnchor(caster, power, out message, out work);
                case "summon":
                    return SummonCreatures(caster, power, slot, "Wolf",
                        Math.Max(1, Math.Min(3, power.maxExtraTargets + 1)),
                        duration, out message, out work);
                case "adaptiveBuff":
                    bool threatened = caster.Map.mapPawns.AllPawnsSpawned.Any(p =>
                        p != caster && !p.Dead && p.HostileTo(caster) &&
                        p.Position.DistanceTo(caster.Position) <= 8f);
                    string def = threatened ? "EB_Buff_ChimeraCarapace" :
                        "EB_Buff_ChimeraAgility";
                    if (!EssencePowerExecutor.AdvancedTimed(caster, def,
                        duration, caster, slot))
                    { message = "Chimeric adaptation status is missing."; return false; }
                    message = threatened ? "Adapted into defensive carapace." :
                        "Adapted into agile movement.";
                    return true;
                case "transfusion":
                    if (caster.needs?.rest == null || caster.needs.rest.CurLevel < 0.16f)
                    { message = "The caster is too exhausted for the transfusion."; return false; }
                    if (!EssencePowerExecutor.FieldHealing(target,
                        power.magnitude * recoveryScale, out work))
                    { message = "The target has no treatable injuries."; return false; }
                    caster.needs.rest.CurLevel = Math.Max(0.02f,
                        caster.needs.rest.CurLevel - 0.12f);
                    message = "Transfused energy through fatigue, healing " +
                        work.ToString("0.#") + " injury severity.";
                    return true;
                case "summonOrder":
                    if (target == null || !target.HostileTo(caster))
                    { message = "Select a hostile pawn for the summoned allies to attack."; return false; }
                    var summons = world.AlliedSummons(caster);
                    int directed = 0;
                    foreach (Pawn ally in summons)
                    {
                        if (ally.jobs == null || ally.Dead) continue;
                        ally.jobs.StartJob(JobMaker.MakeJob(JobDefOf.AttackMelee, target));
                        directed++;
                    }
                    if (directed == 0)
                    { message = "No active summons are available to command."; return false; }
                    work = directed * 0.5f;
                    message = "Directed " + directed + " magical companion(s) toward the foe.";
                    return true;
                case "markedTarget":
                    if (!target.HostileTo(caster))
                    { message = "Predator Mark only applies to a hostile target."; return false; }
                    if (!EssencePowerExecutor.AdvancedTimed(target,
                        "EB_Debuff_PredatorMark", duration, caster, slot, true))
                    { message = "Missing predator mark status."; return false; }
                    message = "Marked hostile weaknesses for friendly essence attacks.";
                    return true;
                case "branchingHeal":
                    return EssencePowerExecutor.AdvancedAreaRegeneration(
                        caster, power, slot, duration, recoveryScale, out message);
                case "containment":
                    if (!target.HostileTo(caster))
                    { message = "Crystal Prison requires a hostile living pawn."; return false; }
                    int walls = CreatePrison(caster, target.Position, duration);
                    bool slowed = EssencePowerExecutor.AdvancedTimed(target,
                        "EB_Debuff_CrystalPrison", duration, caster, slot, true);
                    if (walls == 0 && !slowed)
                    { message = "Unable to form a crystal prison here."; return false; }
                    message = "Formed " + walls +
                        " temporary crystal walls and bound the hostile target.";
                    return true;
                case "decoy":
                    return SummonCreatures(caster, power, slot, "Hare",
                        1, duration, out message, out work);
                case "powerAura":
                    world.Enqueue("powerAura", caster, slot, caster.Position,
                        power.areaRadius, 0f, duration,
                        buffDef: "EB_Buff_ResonanceField");
                    message = "A stationary field now empowers nearby essence attacks.";
                    return true;
                case "wardDome":
                    world.Enqueue("wardDome", caster, slot, caster.Position,
                        power.areaRadius, 0f, duration,
                        buffDef: "EB_Buff_GuardianBastion");
                    message = "A stationary defensive dome supports nearby allies.";
                    return true;
                default:
                    message = "Unknown advanced pawn ability: " + power.effectKind;
                    return false;
            }
        }

        public static bool CastSpatial(Pawn caster, EssencePowerDef power,
            LocalTargetInfo target, EssenceAbilitySlot slot,
            out string message, out float work)
        {
            work = 0f;
            if (caster == null || caster.Dead || !caster.Spawned || caster.Map == null)
            { message = "The caster must be alive and on the map."; return false; }
            IntVec3 cell = target.Cell;
            if (!cell.IsValid || !cell.InBounds(caster.Map) ||
                caster.Position.DistanceTo(cell) > power.range)
            { message = "Select a valid location within ability range."; return false; }
            if (power.targetKind == "thing")
            {
                if (power.effectKind != "remoteManipulation")
                { message = "This technique cannot manipulate items."; return false; }
                Thing item = target.Thing;
                if (item == null || item is Pawn || !item.Spawned || item.Map != caster.Map ||
                    item.def.category != ThingCategory.Item || !item.def.EverHaulable)
                { message = "Select a loose haulable item on the map."; return false; }
                if (cell.DistanceTo(caster.Position) <= 1.5f)
                { message = "That item is already within easy reach."; return false; }
                if (!GenSight.LineOfSight(caster.Position, cell, caster.Map))
                { message = "The item is blocked by walls."; return false; }
                int before = (int)caster.Position.DistanceTo(cell);
                item.DeSpawn();
                if (!GenPlace.TryPlaceThing(item, caster.Position, caster.Map,
                    ThingPlaceMode.Near))
                {
                    GenSpawn.Spawn(item, cell, caster.Map);
                    message = "Telekinetic retrieval could not find a nearby safe location.";
                    return false;
                }
                work = Math.Max(0.5f, before * 0.4f);
                message = "Pulled the loose item to the caster without walking to it.";
                return true;
            }

            if (power.targetKind != "cell")
            { message = "Expected a map-cell target."; return false; }
            var world = caster.Map.GetComponent<MapComponent_EssenceAbilityWorld>();
            switch (power.effectKind)
            {
                case "teleport":
                    if (!CanTeleport(caster, cell, out message)) return false;
                    float distance = caster.Position.DistanceTo(cell);
                    MovePawn(caster, cell);
                    work = Math.Max(1f, distance * 0.5f);
                    message = "Blinked " + distance.ToString("0.#") + " cells.";
                    return true;
                case "blinkStrike":
                    if (!CanTeleport(caster, cell, out message)) return false;
                    float moved = caster.Position.DistanceTo(cell);
                    MovePawn(caster, cell);
                    int hits = 0;
                    foreach (Pawn enemy in caster.Map.mapPawns.AllPawnsSpawned
                        .Where(p => p != caster && !p.Dead && p.HostileTo(caster) &&
                            p.Position.DistanceTo(cell) <= power.areaRadius &&
                            GenSight.LineOfSight(cell, p.Position, caster.Map)).ToList())
                    {
                        if (EssencePowerExecutor.FieldDamage(caster, enemy,
                            power.damageDefName, power.magnitude, out float dealt))
                        { work += dealt; hits++; }
                    }
                    if (hits == 0) work = Math.Max(1f, moved * 0.3f);
                    message = "Lightning step moved " + moved.ToString("0.#") +
                        " cells and struck " + hits + " hostile pawn(s).";
                    return true;
                case "tileBarrier":
                    if (cell.Fogged(caster.Map) ||
                        !GenSight.LineOfSight(caster.Position, cell, caster.Map))
                    { message = "The wall must be placed on visible ground."; return false; }
                    int barriers = CreateWall(caster, cell, power.durationTicks);
                    if (barriers == 0)
                    { message = "No clear cells were available for the water wall."; return false; }
                    work = barriers * 0.5f;
                    message = "Created " + barriers + " temporary tidal barriers.";
                    return true;
                case "survey":
                    int revealed = 0;
                    int radius = Math.Max(1, Math.Min(9, (int)power.areaRadius));
                    for (int dx = -radius; dx <= radius; dx++)
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        IntVec3 c = new IntVec3(cell.x + dx, 0, cell.z + dz);
                        if (!c.InBounds(caster.Map) ||
                            c.DistanceTo(cell) > radius || !c.Fogged(caster.Map))
                            continue;
                        caster.Map.fogGrid.Unfog(c);
                        revealed++;
                    }
                    if (revealed == 0)
                    { message = "There are no unexplored cells in this area."; return false; }
                    work = Math.Min(12f, revealed * 0.08f);
                    message = "Revealed " + revealed + " previously fogged cells.";
                    return true;
                case "hazardZone":
                    if (!cell.Standable(caster.Map) || cell.Fogged(caster.Map) ||
                        !GenSight.LineOfSight(caster.Position, cell, caster.Map))
                    { message = "Molten Rain needs visible open ground."; return false; }
                    world.Enqueue("hazardZone", caster, slot, cell, power.areaRadius,
                        power.magnitude * (1f +
                            (Hediff_EssenceUser.ForPawn(caster)?.GetAffinityBonus("Power") ?? 0f)),
                        power.durationTicks, power.damageDefName, delay: 120);
                    message = "Molten rain will repeatedly strike hostiles near this location.";
                    return true;
                default:
                    message = "Unknown map or item technique: " + power.effectKind;
                    return false;
            }
        }

        private static bool UseChronoAnchor(Pawn caster, EssencePowerDef power,
            out string message, out float work)
        {
            work = 0f;
            var def = DefDatabase<HediffDef>.GetNamedSilentFail("EB_Buff_ChronoAnchor");
            if (def == null)
            { message = "Chrono Anchor effect definition is missing."; return false; }
            var anchor = caster.health.hediffSet.GetFirstHediffOfDef(def)
                as Hediff_EssenceChronoAnchor;
            if (anchor != null && anchor.HasAnchor)
            {
                if (!CanTeleport(caster, anchor.anchor, out message, false)) return false;
                float distance = caster.Position.DistanceTo(anchor.anchor);
                MovePawn(caster, anchor.anchor);
                caster.health.RemoveHediff(anchor);
                work = Math.Max(1f, distance * 0.4f);
                message = "Returned to the stored anchor (" +
                    distance.ToString("0.#") + " cells).";
                return true;
            }
            if (anchor == null)
            {
                anchor = HediffMaker.MakeHediff(def, caster)
                    as Hediff_EssenceChronoAnchor;
                if (anchor == null)
                { message = "Could not form the time anchor."; return false; }
                caster.health.AddHediff(anchor);
            }
            anchor.SetAnchor(caster.Position, power.durationTicks);
            message = "Time anchor stored at current location; cast again to return before it expires.";
            return true;
        }

        internal static bool CanTeleport(Pawn caster, IntVec3 cell, out string message,
            bool requiresSight = true)
        {
            if (!cell.IsValid || !cell.InBounds(caster.Map) ||
                !cell.Standable(caster.Map) || cell.Fogged(caster.Map) ||
                cell.GetFirstBuilding(caster.Map) != null ||
                cell.GetFirstPawn(caster.Map) != null)
            { message = "Destination is not a visible, open, unoccupied tile."; return false; }
            if (requiresSight && !GenSight.LineOfSight(caster.Position, cell, caster.Map))
            { message = "Cannot blink through a solid wall."; return false; }
            if (cell == caster.Position)
            { message = "The caster is already at that destination."; return false; }
            message = "";
            return true;
        }

        internal static void MovePawn(Pawn caster, IntVec3 cell)
        {
            caster.pather.StopDead();
            caster.Position = cell;
            caster.Notify_Teleported();
        }

        internal static bool SummonCreatures(Pawn caster, EssencePowerDef power,
            EssenceAbilitySlot slot, string kindName, int count, int duration,
            out string message, out float work)
        {
            work = 0f;
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(kindName);
            if (kind == null)
            { message = "Missing temporary creature type " + kindName + "."; return false; }
            var world = caster.Map.GetComponent<MapComponent_EssenceAbilityWorld>();
            int spawned = 0;
            for (int i = 0; i < count; i++)
            {
                if (!MapComponent_EssenceWorld.TryFindNear(caster.Position, caster.Map,
                    out IntVec3 cell)) break;
                if (cell.GetFirstPawn(caster.Map) != null) continue;
                Pawn animal = PawnGenerator.GeneratePawn(kind);
                if (animal == null) continue;
                animal.SetFaction(Faction.OfPlayer);
                GenSpawn.Spawn(animal, cell, caster.Map);
                world.TrackSummon(animal, caster, slot, duration);
                spawned++;
            }
            if (spawned == 0)
            { message = "No safe adjacent tiles for temporary companions."; return false; }
            message = "Manifested " + spawned + " temporary allied " +
                kindName.ToLowerInvariant() + "(s).";
            return true;
        }

        private static int CreateWall(Pawn caster, IntVec3 center, int duration)
        {
            int count = 0;
            for (int z = -2; z <= 2; z++)
                if (PlaceBarrier(caster.Map,
                    new IntVec3(center.x, 0, center.z + z), duration)) count++;
            return count;
        }

        private static int CreatePrison(Pawn caster, IntVec3 center, int duration)
        {
            int count = 0;
            int[] dx = { -1, 1, 0, 0 };
            int[] dz = { 0, 0, -1, 1 };
            for (int i = 0; i < 4; i++)
                if (PlaceBarrier(caster.Map,
                    new IntVec3(center.x + dx[i], 0, center.z + dz[i]), duration))
                    count++;
            return count;
        }

        private static bool PlaceBarrier(Map map, IntVec3 cell, int duration)
        {
            if (!cell.InBounds(map) || !cell.Standable(map) ||
                cell.GetFirstBuilding(map) != null || cell.GetFirstPawn(map) != null ||
                cell.Fogged(map)) return false;
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("EB_TemporaryEssenceBarrier");
            if (def == null) return false;
            var barrier = ThingMaker.MakeThing(def) as Thing_EssenceBarrier;
            if (barrier == null) return false;
            barrier.Initialize(duration);
            GenSpawn.Spawn(barrier, cell, map);
            return true;
        }
    }
}
