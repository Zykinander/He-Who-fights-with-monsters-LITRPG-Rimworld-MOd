using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// v0.19: the final thirty themed techniques. Each case has actual game
    /// behavior; no planned design is treated as playable without a handler.
    /// All damage goes through Pawn.TakeDamage to preserve major-rank defense.
    /// </summary>
    public static class EssenceHundredAbilities
    {
        public static bool CastPawn(Pawn caster, EssencePowerDef power, Pawn target,
            EssenceAbilitySlot slot, float powerScale, float recoveryScale,
            int duration, out string message, out float work)
        {
            work = 0f;
            var world = caster.Map.GetComponent<MapComponent_EssenceAbilityWorld>();
            switch (power.effectKind)
            {
                case "restorationZone":
                    if (NearbyHostiles(caster, caster.Position, power.areaRadius + 4f).Any())
                    { message = "Oasis Well requires an area free of nearby hostiles."; return false; }
                    world.Enqueue("restorationZone", caster, slot, caster.Position,
                        power.areaRadius, power.magnitude * recoveryScale, duration);
                    message = "A restorative sanctuary formed; real healing earns training.";
                    return true;
                case "familiar":
                    return EssenceAdvancedAbilities.SummonCreatures(caster, power, slot,
                        "Megascarab", 1, duration, out message, out work);
                case "summonFlock":
                    return EssenceAdvancedAbilities.SummonCreatures(caster, power, slot,
                        "Turkey", 3, duration, out message, out work);
                case "concealment":
                    return ApplyBoon(caster, "EB_Buff_Mistwalk", caster, slot,
                        duration, "Mistwalk grants evasive movement.", out message);
                case "swapTeleport":
                    return SwapAllies(caster, target, out message, out work);
                case "piercingSound":
                    if (!EssencePowerExecutor.FieldPiercingDamage(caster, target,
                        power.damageDefName, power.magnitude * powerScale, 0.65f,
                        out work))
                    { message = "Sonic Lance could not resolve its damage."; return false; }
                    message = "Sonic Lance cut through part of the target's armor for " +
                        work.ToString("0.#") + " effective damage.";
                    return true;
                case "radiantPurge":
                    return RadiantPurge(caster, power, powerScale, out message, out work);
                case "conditionalBuff":
                    if (caster.Map.glowGrid.GameGlowAt(caster.Position) > 0.4f)
                    { message = "Moonshadow can only be invoked in dim light."; return false; }
                    return ApplyBoon(caster, "EB_Buff_Moonshadow", caster, slot,
                        duration, "Moonshadow enhances evasion in low light.", out message);
                case "sustainedLeech":
                    world.Enqueue("sustainedLeech", caster, slot, caster.Position,
                        power.areaRadius, power.magnitude * powerScale, duration,
                        power.damageDefName);
                    message = "The Hunger Field begins draining nearby hostiles.";
                    return true;
                case "armorBreak":
                    if (!EssencePowerExecutor.FieldDamage(caster, target,
                        power.damageDefName, power.magnitude * powerScale,
                        out work))
                    { message = "Armor breaking strike failed."; return false; }
                    if (!target.Dead)
                        EssencePowerExecutor.AdvancedTimed(target,
                            "EB_Debuff_ShatteredArmor", duration, null, null);
                    message = "Shattered enemy defenses with " +
                        work.ToString("0.#") + " effective damage.";
                    return true;
                case "harvestAura":
                    return ApplyToAllies(caster, power, slot, duration,
                        "EB_Buff_HarvestBlessing", out message);
                case "linkedDefense":
                    if (target == caster || target.Faction != caster.Faction)
                    { message = "Weave Ward requires another allied pawn."; return false; }
                    return ApplyReaction(target, "EB_Buff_WeaveWard",
                        "linkedDefense", duration, 0f, caster, slot, caster,
                        "Linked this ally to the caster's protective weave.", out message);
                case "counterStance":
                    return ApplyReaction(caster, "EB_Buff_IronReprisal",
                        "counterStance", duration, power.magnitude * powerScale,
                        caster, slot, null,
                        "The counterstance will retaliate against close attackers.", out message);
                case "grapple":
                    return ApplyHostileDebuff(caster, target, slot,
                        "EB_Debuff_SerpentCoil", duration,
                        "Serpent Coil binds and disorients the hostile.", out message);
                case "emergencyHeal":
                    return ApplyReaction(caster, "EB_Buff_DawnRebirth",
                        "emergencyHeal", duration, power.magnitude * recoveryScale,
                        caster, slot, null,
                        "Dawn Rebirth will mend injuries if health falls dangerously low.", out message);
                case "attributeShift":
                    return ApplyBoon(caster, "EB_Buff_AdeptConvergence", caster, slot,
                        duration, "Adept Convergence redirects some Speed into Power.", out message);
                case "confusion":
                    return ApplyHostileDebuff(caster, target, slot,
                        "EB_Debuff_FeyGlamour", duration,
                        "Fey Glamour disrupts the hostile's accuracy and coordination.", out message);
                case "mobileWard":
                    if (target.HostileTo(caster))
                    { message = "Nimbus Guard protects allied or neutral pawns only."; return false; }
                    return ApplyBoon(target, "EB_Buff_NimbusGuard", caster, slot,
                        duration, "A mobile storm ward protects the selected pawn.", out message);
                case "orbitStrike":
                    world.Enqueue("orbitStrike", caster, slot, caster.Position,
                        power.areaRadius, power.magnitude * powerScale,
                        duration, power.damageDefName);
                    message = "Orbiting blades will strike hostiles near the moving caster.";
                    return true;
                case "sustainedDebuff":
                    return CorruptingSeed(caster, target, power, slot, powerScale,
                        duration, out message);
                case "storedTrigger":
                    return ApplyReaction(caster, "EB_Buff_TalismanSeal",
                        "storedTrigger", duration, 0f, caster, slot, null,
                        "Talisman Seal will absorb part of the next pawn attack.", out message);
                case "teamLink":
                    return ApplyToAllies(caster, power, slot, duration,
                        "EB_Buff_PactOfUnity", out message);
                case "survivalBoon":
                    if (target.HostileTo(caster))
                    { message = "Phoenix Ashes must protect a nonhostile pawn."; return false; }
                    return ApplyReaction(target, "EB_Buff_PhoenixAshes",
                        "survivalBoon", duration, power.magnitude * recoveryScale,
                        caster, slot, null,
                        "Phoenix Ashes will mend the pawn during a severe injury crisis.", out message);
                case "meleeCombo":
                    if (target == caster || caster.Position.DistanceTo(target.Position) > 2.5f)
                    { message = "Feral Claw requires another pawn within melee distance."; return false; }
                    return ComboStrike(caster, target, power, slot,
                        powerScale, out message, out work);
                case "debuffReflect":
                    return ApplyReaction(caster, "EB_Buff_WardEcho",
                        "debuffReflect", duration, 0f, caster, slot, null,
                        "Ward Echo will purge and return temporary Essencebound curses.", out message);
                default:
                    message = "Unimplemented final-stage pawn technique: " + power.effectKind;
                    return false;
            }
        }

        public static bool CastSpatial(Pawn caster, EssencePowerDef power,
            LocalTargetInfo target, EssenceAbilitySlot slot,
            out string message, out float work)
        {
            work = 0f;
            if (caster == null || caster.Dead || !caster.Spawned || caster.Map == null)
            { message = "The caster is not on a map."; return false; }
            IntVec3 c = target.Cell;
            if (!c.IsValid || !c.InBounds(caster.Map) ||
                caster.Position.DistanceTo(c) > power.range ||
                c.Fogged(caster.Map))
            { message = "Choose explored ground within the technique's range."; return false; }
            var world = caster.Map.GetComponent<MapComponent_EssenceAbilityWorld>();
            if (power.effectKind == "longTeleport")
            {
                if (!c.Standable(caster.Map) ||
                    c.GetFirstPawn(caster.Map) != null ||
                    c.GetFirstBuilding(caster.Map) != null)
                { message = "Worldstep needs a clear destination."; return false; }
                world.Enqueue("delayedTeleport", caster, slot, c,
                    0f, 0f, 330, delay: 240);
                message = "Worldstep channel started; teleportation will complete shortly.";
                return true;
            }
            if (!GenSight.LineOfSight(caster.Position, c, caster.Map))
            { message = "This field requires a clear line of sight."; return false; }
            switch (power.effectKind)
            {
                case "webZone":
                case "slowField":
                    if (!c.Standable(caster.Map))
                    { message = "A slowing field needs open ground."; return false; }
                    world.Enqueue(power.effectKind, caster, slot, c,
                        power.areaRadius, 0f, power.durationTicks,
                        buffDef: power.buffHediffDefName);
                    message = "A slowing field now affects hostile pawns crossing the area.";
                    return true;
                case "pullZone":
                    if (!c.Standable(caster.Map))
                    { message = "Gravity Well needs open ground."; return false; }
                    world.Enqueue("gravityWell", caster, slot, c,
                        power.areaRadius, 0f, power.durationTicks,
                        buffDef: "EB_Debuff_GravitySnare");
                    message = "Gravity Well draws nearby enemies toward its center.";
                    return true;
                case "groundWard":
                    if (!c.Standable(caster.Map))
                    { message = "The rune needs clear ground."; return false; }
                    world.Enqueue("wardDome", caster, slot, c,
                        power.areaRadius, 0f, power.durationTicks,
                        buffDef: "EB_Buff_RuneBeacon");
                    message = "A static defensive rune protects allies nearby.";
                    return true;
                default:
                    message = "Unimplemented final-stage map technique: " + power.effectKind;
                    return false;
            }
        }

        private static IEnumerable<Pawn> NearbyHostiles(Pawn caster,
            IntVec3 center, float radius)
        {
            return caster.Map.mapPawns.AllPawnsSpawned.Where(p =>
                p != null && p != caster && !p.Dead && p.HostileTo(caster) &&
                p.Position.DistanceTo(center) <= radius &&
                GenSight.LineOfSight(center, p.Position, caster.Map));
        }

        private static bool ApplyBoon(Pawn target, string defName, Pawn caster,
            EssenceAbilitySlot slot, int duration, string text, out string message)
        {
            if (!EssencePowerExecutor.AdvancedTimed(target, defName,
                duration, caster, slot))
            { message = "Missing boon definition: " + defName; return false; }
            message = text + " Training accrues while the boon remains active.";
            return true;
        }

        private static bool ApplyToAllies(Pawn caster, EssencePowerDef power,
            EssenceAbilitySlot slot, int duration, string defName,
            out string message)
        {
            int applied = 0;
            foreach (Pawn ally in caster.Map.mapPawns.AllPawnsSpawned.Where(p =>
                p != null && !p.Dead && !p.HostileTo(caster) &&
                p.Position.DistanceTo(caster.Position) <= power.areaRadius &&
                GenSight.LineOfSight(caster.Position, p.Position, caster.Map)).ToList())
            {
                if (EssencePowerExecutor.AdvancedTimed(ally, defName,
                    duration, caster, slot))
                    applied++;
            }
            message = applied > 0 ? "Boon applied to " + applied +
                " nearby allied pawn(s)." : "No reachable allies received the boon.";
            return applied > 0;
        }

        private static bool ApplyHostileDebuff(Pawn caster, Pawn target,
            EssenceAbilitySlot slot, string defName, int duration,
            string text, out string message)
        {
            if (target == null || !target.HostileTo(caster))
            { message = "This technique requires a hostile pawn."; return false; }
            if (!EssencePowerExecutor.AdvancedTimed(target,
                defName, duration, caster, slot, true))
            { message = "Missing hostile status: " + defName; return false; }
            message = text + " Work accrues while the hostile is affected.";
            return true;
        }

        private static bool ApplyReaction(Pawn target, string defName, string kind,
            int duration, float strength, Pawn caster, EssenceAbilitySlot slot,
            Pawn linked, string description, out string message)
        {
            var def = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
            if (def == null) { message = "Missing reactive status: " + defName; return false; }
            var state = target.health.hediffSet.GetFirstHediffOfDef(def)
                as Hediff_EssenceReactive;
            if (state == null)
            {
                state = HediffMaker.MakeHediff(def, target) as Hediff_EssenceReactive;
                if (state == null) { message = "Unable to apply reactive status."; return false; }
                target.health.AddHediff(state);
            }
            state.Configure(kind, duration, strength, caster, slot, linked);
            message = description;
            return true;
        }

        private static bool SwapAllies(Pawn caster, Pawn ally,
            out string message, out float work)
        {
            work = 0f;
            if (ally == null || ally == caster || ally.Faction != caster.Faction ||
                ally.Dead || ally.Downed || !ally.Spawned || ally.Map != caster.Map)
            { message = "Mirror Step requires another standing allied pawn."; return false; }
            IntVec3 a = caster.Position;
            IntVec3 b = ally.Position;
            if (!a.Standable(caster.Map) || !b.Standable(caster.Map))
            { message = "Both pawns need safe standing locations."; return false; }
            if (a.DistanceTo(b) <= 1f)
            { message = "Move farther apart before swapping."; return false; }
            caster.pather.StopDead();
            ally.pather.StopDead();
            // Temporarily deselect the occupied target cells by swapping
            // pawn position directly; both were valid standing cells.
            caster.Position = b;
            ally.Position = a;
            caster.Notify_Teleported();
            ally.Notify_Teleported();
            work = Math.Max(1f, a.DistanceTo(b) * 0.4f);
            message = "Swapped allied positions over " +
                a.DistanceTo(b).ToString("0.#") + " cells.";
            return true;
        }

        private static bool RadiantPurge(Pawn caster, EssencePowerDef power,
            float powerScale, out string message, out float work)
        {
            work = 0f;
            int cleansed = 0, struck = 0;
            string[] dark = { "EB_Debuff_OmenHex", "EB_Debuff_FeyGlamour",
                              "EB_Debuff_VenomHaze", "EB_Debuff_GravitySnare" };
            foreach (Pawn pawn in caster.Map.mapPawns.AllPawnsSpawned.Where(p =>
                p != null && !p.Dead &&
                p.Position.DistanceTo(caster.Position) <= power.areaRadius &&
                GenSight.LineOfSight(caster.Position, p.Position, caster.Map)).ToList())
            {
                if (pawn.HostileTo(caster))
                {
                    if (EssencePowerExecutor.FieldDamage(caster, pawn, "Flame",
                        power.magnitude * powerScale, out float actual))
                    { work += actual; struck++; }
                }
                else
                {
                    foreach (Hediff bad in pawn.health.hediffSet.hediffs
                        .Where(h => dark.Contains(h.def.defName)).ToList())
                    {
                        pawn.health.RemoveHediff(bad);
                        work += 10f;
                        cleansed++;
                    }
                }
            }
            message = "Sunward Flare struck " + struck + " hostiles and lifted " +
                cleansed + " Essencebound curses.";
            return cleansed > 0 || struck > 0;
        }

        private static bool CorruptingSeed(Pawn caster, Pawn target,
            EssencePowerDef power, EssenceAbilitySlot slot, float powerScale,
            int duration, out string message)
        {
            if (!target.HostileTo(caster))
            { message = "Corrupting Seed can affect hostile pawns only."; return false; }
            var def = DefDatabase<HediffDef>.GetNamedSilentFail("EB_Debuff_CorruptingSeed");
            if (def == null) { message = "Corruption status is missing."; return false; }
            var effect = target.health.hediffSet.GetFirstHediffOfDef(def)
                as Hediff_EssenceCorruption;
            if (effect == null)
            {
                effect = HediffMaker.MakeHediff(def, target) as Hediff_EssenceCorruption;
                if (effect == null) { message = "Corruption effect creation failed."; return false; }
                target.health.AddHediff(effect);
            }
            effect.Refresh(duration, power.magnitude * powerScale, caster, slot);
            message = "Corruption will erode armor and damage the hostile over time.";
            return true;
        }

        private static bool ComboStrike(Pawn caster, Pawn target,
            EssencePowerDef power, EssenceAbilitySlot slot, float powerScale,
            out string message, out float work)
        {
            work = 0f;
            var def = DefDatabase<HediffDef>.GetNamedSilentFail("EB_Buff_FeralCombo");
            if (def == null) { message = "Feral combo status missing."; return false; }
            var chain = caster.health.hediffSet.GetFirstHediffOfDef(def)
                as Hediff_EssenceReactive;
            if (chain == null)
            {
                chain = HediffMaker.MakeHediff(def, caster) as Hediff_EssenceReactive;
                if (chain == null) { message = "Could not initiate combo."; return false; }
                caster.health.AddHediff(chain);
                chain.Configure("meleeCombo", 480, 0f, caster, slot);
            }
            float bonus = chain.ComboFactor;
            if (!EssencePowerExecutor.FieldDamage(caster, target, "Blunt",
                power.magnitude * powerScale * bonus, out work))
            { message = "Melee combo strike failed."; return false; }
            if (work > 0f) chain.RecordCombo();
            message = "Feral Claw struck with a " + bonus.ToString("0.##") +
                "x combo for " + work.ToString("0.#") + " effective damage.";
            return true;
        }
    }
}
