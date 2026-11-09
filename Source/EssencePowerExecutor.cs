using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Prototype effects: intentional small, reusable gameplay primitives rather
    // than a separate C# class for every essence/stone combination.
    public static class EssencePowerExecutor
    {
        public static bool ValidateTarget(Pawn caster, EssencePowerDef power, Pawn target, out string reason)
        {
            if (caster == null || caster.Dead || !caster.Spawned || caster.Map == null || power == null)
            { reason = "The caster must be alive and on a map."; return false; }
            if (power.targetKind == "self") target = caster;
            if (target == null || target.Dead || !target.Spawned || target.Map != caster.Map)
            { reason = "Choose a living pawn on the same map."; return false; }
            // Every pawn-targeting ability can target allies, enemies, neutral pawns,
            // and the caster. The legacy "ally"/"hostile" strings remain accepted
            // for compatibility with third-party XML based on earlier prototypes.
            bool pawnTargeted = power.targetKind == "pawn" ||
                                power.targetKind == "hostile" || power.targetKind == "ally";
            if (!pawnTargeted && power.targetKind != "self")
            { reason = "Unknown power target kind: " + power.targetKind; return false; }
            if (pawnTargeted && target != caster)
            {
                if (caster.Position.DistanceTo(target.Position) > power.range)
                { reason = "Target is out of range (" + power.range + " cells)."; return false; }
                if (!GenSight.LineOfSight(caster.Position, target.Position, caster.Map))
                { reason = "Line of sight is blocked."; return false; }
            }
            reason = "";
            return true;
        }

        public static bool Apply(Pawn caster, EssencePowerDef power, Pawn target, out string reason)
        {
            return Apply(caster, power, target, null, out reason, out _);
        }

        // Progress is credited only for actual work. Casting on an invalid target
        // or doing zero effective work never advances a damage/heal ability.
        public static bool Apply(Pawn caster, EssencePowerDef power, Pawn target,
            EssenceAbilitySlot slot, out string reason, out float work)
        {
            work = 0f;
            if (!ValidateTarget(caster, power, target, out reason)) return false;
            if (power.targetKind == "self") target = caster;

            var user = Hediff_EssenceUser.ForPawn(caster);
            float powerScale = 1f + (user?.GetAffinityBonus("Power") ?? 0f);
            float recoveryScale = 1f + (user?.GetAffinityBonus("Recovery") ?? 0f);
            float spiritScale = 1f + (user?.GetAffinityBonus("Spirit") ?? 0f);
            int buffDuration = Math.Max(1, (int)(power.durationTicks * spiritScale));

            switch (power.effectKind)
            {
                case "areaHeal":
                    return HealNearbyAllies(caster, power, recoveryScale, out reason, out work);
                case "chainHeal":
                    return ChainHeal(caster, target, power, recoveryScale, out reason, out work);
                case "areaRegen":
                    return RegenerateNearbyAllies(caster, power, slot, buffDuration,
                        recoveryScale, out reason);
                case "purgeHeal":
                    return PurgeAndHeal(target, power, recoveryScale, out reason, out work);
                case "damageAreaDebuff":
                    return BurstAndWeaken(caster, target, power, powerScale, buffDuration,
                        out reason, out work);
                case "leechPulse":
                    return LifeDrainingPulse(caster, power, powerScale, recoveryScale,
                        out reason, out work);
                case "execute":
                    return ExecuteWounded(caster, target, power, powerScale, out reason, out work);
                case "chain":
                    return ChainDamage(caster, target, power, powerScale, out reason, out work);
                case "splash":
                    return SplashDamage(caster, target, power, powerScale, out reason, out work);
                case "drain":
                    return DrainHealth(caster, target, power, powerScale, recoveryScale, out reason, out work);
                case "healGroup":
                    return HealInjuries(target, power.magnitude * recoveryScale, out reason, out work);
                case "regeneration":
                    return ApplyRegeneration(caster, target, power, slot, buffDuration,
                        recoveryScale, out reason);
                case "auraBuff":
                    return ApplyAuraBuff(caster, power, slot, buffDuration, out reason);
                case "areaCleanse":
                    return CleanseArea(caster, power, out reason, out work);
                case "debuff":
                    if (!target.HostileTo(caster))
                    { reason = "This control power needs a hostile target."; return false; }
                    if (!ApplyTimed(target, power.buffHediffDefName, buffDuration,
                        caster, slot, true, true))
                    { reason = "Missing control effect definition."; return false; }
                    reason = "Target restrained; training accrues while the hostile remains affected.";
                    return true;
                case "healWard":
                    float healed = 0f;
                    HealInjuries(target, power.magnitude * recoveryScale, out _, out healed);
                    work = healed;
                    if (!ApplyTimed(target, power.buffHediffDefName, buffDuration,
                        caster, slot, true))
                    { reason = "Missing protective boon definition."; return false; }
                    reason = "Restored " + work.ToString("0.#") +
                        " injury severity and applied a protective boon.";
                    return true;
                case "damage":
                    return Damage(caster, target, power.damageDefName,
                        power.magnitude * powerScale, out reason, out work);
                case "damageDebuff":
                    if (!Damage(caster, target, power.damageDefName,
                        power.magnitude * powerScale, out reason, out work)) return false;
                    if (!target.Dead && ApplyTimed(target, power.buffHediffDefName,
                        power.durationTicks, null, null, false))
                        reason += " Applied secondary status effect.";
                    return true;
                case "heal":
                    var injury = target.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                        .Where(i => !i.IsPermanent())
                        .OrderByDescending(i => i.Severity).FirstOrDefault();
                    if (injury == null)
                    { reason = "Target has no healable injury."; return false; }
                    float before = injury.Severity;
                    injury.Heal(power.magnitude * recoveryScale);
                    work = Math.Max(0f, before - injury.Severity);
                    reason = "Healed " + work.ToString("0.#") + " injury severity.";
                    return work > 0f;
                case "buff":
                    if (!ApplyTimed(target, power.buffHediffDefName, buffDuration,
                        caster, slot, true))
                    { reason = "Missing status definition for this power."; return false; }
                    reason = "Temporary boon applied; training accumulates while active.";
                    return true;
                case "cleanse":
                    var badNames = RemovableDebuffs;
                    var affliction = target.health.hediffSet.hediffs.FirstOrDefault(h => badNames.Contains(h.def.defName));
                    if (affliction == null)
                    { reason = "Target has no Essencebound affliction to cleanse."; return false; }
                    target.health.RemoveHediff(affliction);
                    work = 10f; // Only a real removed affliction grants work.
                    reason = "Essencebound affliction removed.";
                    return true;
                case "pulse":
                    var hostilePawns = caster.Map.mapPawns.AllPawnsSpawned
                        .Where(p => p != caster && !p.Dead && p.HostileTo(caster) &&
                                    p.Position.DistanceTo(caster.Position) <= power.range).ToList();
                    if (hostilePawns.Count == 0)
                    { reason = "No hostile pawns are within pulse range."; return false; }
                    foreach (Pawn other in hostilePawns)
                    {
                        if (Damage(caster, other, power.damageDefName, power.magnitude * powerScale,
                            out _, out float dealt))
                            work += dealt;
                    }
                    reason = "Pulse hit " + hostilePawns.Count + " hostile pawn(s), dealing " +
                        work.ToString("0.#") + " effective damage.";
                    return true;
                default:
                    reason = "Unknown effect kind: " + power.effectKind;
                    return false;
            }
        }

        // Area heal: up to (1 + maxExtraTargets) actually wounded nearby allies.
        // Only severity restored counts toward the originating ability's XP.
        private static bool HealNearbyAllies(Pawn caster, EssencePowerDef power,
            float recoveryScale, out string reason, out float work)
        {
            work = 0f;
            int count = 0;
            var injured = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != null && !p.Dead && !p.HostileTo(caster) &&
                    p.Position.DistanceTo(caster.Position) <= power.areaRadius &&
                    GenSight.LineOfSight(caster.Position, p.Position, caster.Map) &&
                    p.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                        .Any(i => !i.IsPermanent()))
                .OrderByDescending(p => p.health.hediffSet.hediffs
                    .OfType<Hediff_Injury>().Where(i => !i.IsPermanent()).Sum(i => i.Severity))
                .Take(Math.Max(1, Math.Min(6, power.maxExtraTargets + 1)))
                .ToList();
            foreach (Pawn ally in injured)
            {
                if (HealInjuries(ally, power.magnitude * recoveryScale, out _, out float healed))
                { work += healed; count++; }
            }
            reason = count > 0 ? "Restored " + work.ToString("0.#") +
                " injury severity across " + count + " pawn(s)." :
                "No wounded allies nearby to heal.";
            return count > 0;
        }

        // Chain healing starts on the selected pawn, then jumps to nearby
        // nonhostile injured pawns. Unlike a blanket heal, it prioritizes the
        // highest injury severity in line of sight of the primary target.
        private static bool ChainHeal(Pawn caster, Pawn target, EssencePowerDef power,
            float recoveryScale, out string reason, out float work)
        {
            work = 0f;
            int count = 0;
            if (HealInjuries(target, power.magnitude * recoveryScale, out _, out float first))
            { work += first; count++; }
            var secondary = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != null && p != target && !p.Dead && !p.HostileTo(caster) &&
                    p.Position.DistanceTo(target.Position) <= power.areaRadius &&
                    GenSight.LineOfSight(target.Position, p.Position, caster.Map) &&
                    p.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                        .Any(i => !i.IsPermanent()))
                .OrderBy(p => p.Position.DistanceTo(target.Position))
                .Take(Math.Max(0, Math.Min(5, power.maxExtraTargets)))
                .ToList();
            foreach (Pawn ally in secondary)
            {
                if (HealInjuries(ally, power.magnitude * 0.65f * recoveryScale,
                    out _, out float restored))
                { work += restored; count++; }
            }
            reason = count > 0 ? "Chain-healed " + count + " pawn(s), restoring " +
                work.ToString("0.#") + " injury severity." :
                "No reachable healing targets have treatable wounds.";
            return count > 0;
        }

        // A group regeneration only sticks to injured nearby allies, giving
        // no free progress for casting on fully healthy pawns.
        private static bool RegenerateNearbyAllies(Pawn caster, EssencePowerDef power,
            EssenceAbilitySlot slot, int duration, float recoveryScale, out string reason)
        {
            int count = 0;
            var injured = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != null && !p.Dead && !p.HostileTo(caster) &&
                    p.Position.DistanceTo(caster.Position) <= power.areaRadius &&
                    GenSight.LineOfSight(caster.Position, p.Position, caster.Map) &&
                    p.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                        .Any(i => !i.IsPermanent()))
                .Take(Math.Max(1, Math.Min(6, power.maxExtraTargets + 1)))
                .ToList();
            foreach (Pawn ally in injured)
            {
                if (ApplyRegeneration(caster, ally, power, slot, duration, recoveryScale,
                    out _)) count++;
            }
            reason = count > 0 ? "Regeneration seeded on " + count +
                " injured pawn(s); real healing earns progress over time." :
                "No injured allies are within the regenerative area.";
            return count > 0;
        }

        private static bool PurgeAndHeal(Pawn target, EssencePowerDef power,
            float recoveryScale, out string reason, out float work)
        {
            HealInjuries(target, power.magnitude * recoveryScale, out _, out float healed);
            int removed = CleanseTarget(target);
            work = healed + removed * 10f;
            reason = work > 0f ? "Healed " + healed.ToString("0.#") +
                " injury severity and dispelled " + removed + " debuff(s)." :
                "Target has neither treatable wounds nor Essencebound debuffs.";
            return work > 0f;
        }

        private static readonly HashSet<string> RemovableDebuffs =
            new HashSet<string> {
                "EB_Debuff_Chilled", "EB_Debuff_Static",
                "EB_Debuff_GravitySnare", "EB_Debuff_CorrodedArmor",
                "EB_Debuff_VenomHaze", "EB_Debuff_BriarBind",
                "EB_Debuff_RuneBind", "EB_Debuff_OmenHex",
                "EB_Debuff_ThreadSnare"
            };

        private static int CleanseTarget(Pawn target)
        {
            if (target?.health?.hediffSet == null) return 0;
            var ailments = target.health.hediffSet.hediffs
                .Where(h => h?.def != null && RemovableDebuffs.Contains(h.def.defName))
                .ToList();
            foreach (Hediff ailment in ailments)
                target.health.RemoveHediff(ailment);
            return ailments.Count;
        }

        // Primary strike works on any pawn as before. Only nearby hostile
        // pawns with line of sight from the impact are hit by secondary
        // effects and receive the area debuff.
        private static bool BurstAndWeaken(Pawn caster, Pawn target, EssencePowerDef power,
            float powerScale, int duration, out string reason, out float work)
        {
            work = 0f;
            if (!Damage(caster, target, power.damageDefName,
                power.magnitude * powerScale, out _, out float primary))
            { reason = "Burst attack failed to resolve."; return false; }
            work += primary;
            int affected = 1;
            if (!target.Dead && target.HostileTo(caster))
                ApplyTimed(target, power.buffHediffDefName, duration, null, null, false);
            var nearby = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != caster && p != target && !p.Dead && p.HostileTo(caster) &&
                    p.Position.DistanceTo(target.Position) <= power.areaRadius &&
                    GenSight.LineOfSight(target.Position, p.Position, caster.Map))
                .Take(Math.Max(0, Math.Min(8, power.maxExtraTargets)))
                .ToList();
            foreach (Pawn other in nearby)
            {
                if (Damage(caster, other, power.damageDefName,
                    power.magnitude * 0.45f * powerScale, out _, out float dealt))
                {
                    work += dealt;
                    affected++;
                    if (!other.Dead)
                        ApplyTimed(other, power.buffHediffDefName, duration,
                            null, null, false);
                }
            }
            reason = "Burst touched " + affected + " pawn(s) and dealt " +
                work.ToString("0.#") + " effective damage.";
            return true;
        }

        private static bool LifeDrainingPulse(Pawn caster, EssencePowerDef power,
            float powerScale, float recoveryScale, out string reason, out float work)
        {
            work = 0f;
            int hits = 0;
            foreach (Pawn enemy in caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != caster && !p.Dead && p.HostileTo(caster) &&
                    p.Position.DistanceTo(caster.Position) <= power.range &&
                    GenSight.LineOfSight(caster.Position, p.Position, caster.Map)).ToList())
            {
                if (Damage(caster, enemy, power.damageDefName,
                    power.magnitude * powerScale, out _, out float dealt))
                { hits++; work += dealt; }
            }
            if (hits == 0)
            { reason = "No hostiles are visible inside the draining pulse."; return false; }
            float restored = 0f;
            if (work > 0f && !caster.Dead)
                HealInjuries(caster, work * 0.3f * recoveryScale, out _, out restored);
            reason = "Draining pulse hit " + hits + " foe(s) for " +
                work.ToString("0.#") + " damage and healed " +
                restored.ToString("0.#") + " injury severity.";
            return true;
        }

        // Finishing strikes reward real damage, not casting or repeatedly
        // hitting a target that has no remaining wounds.
        private static bool ExecuteWounded(Pawn caster, Pawn target, EssencePowerDef power,
            float powerScale, out string reason, out float work)
        {
            float previousWounds = target.health.hediffSet.hediffs
                .OfType<Hediff_Injury>().Sum(i => i.Severity);
            float finisher = previousWounds >= 5f ? 1.6f : 1f;
            if (!Damage(caster, target, power.damageDefName,
                power.magnitude * finisher * powerScale, out reason, out work))
                return false;
            if (finisher > 1f) reason += " Injured-target finisher bonus applied.";
            return true;
        }

        private static bool HealInjuries(Pawn target, float budget, out string reason, out float work)
        {
            work = 0f;
            if (target?.health?.hediffSet == null || budget <= 0f)
            { reason = "No healing was possible."; return false; }

            var wounds = target.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                .Where(i => !i.IsPermanent())
                .OrderByDescending(i => i.Severity).ToList();
            foreach (var injury in wounds)
            {
                if (budget <= 0f) break;
                float before = injury.Severity;
                injury.Heal(Math.Min(budget, before));
                float healed = Math.Max(0f, before - injury.Severity);
                work += healed;
                budget -= healed;
            }
            reason = "Restored " + work.ToString("0.#") + " total injury severity.";
            return work > 0f;
        }

        private static bool DrainHealth(Pawn caster, Pawn target, EssencePowerDef power,
            float powerScale, float recoveryScale, out string reason, out float work)
        {
            if (!Damage(caster, target, power.damageDefName,
                    power.magnitude * powerScale, out reason, out work)) return false;
            float transferred = 0f;
            if (work > 0f && !caster.Dead)
                HealInjuries(caster, work * 0.5f * recoveryScale, out _, out transferred);
            reason += " Life siphoned: " + transferred.ToString("0.#") + ".";
            // Damage work is awarded even when the caster is already healthy.
            return true;
        }

        private static bool ChainDamage(Pawn caster, Pawn target, EssencePowerDef power,
            float powerScale, out string reason, out float work)
        {
            work = 0f;
            IntVec3 center = target.Position;
            if (!Damage(caster, target, power.damageDefName,
                    power.magnitude * powerScale, out _, out float first)) 
            { reason = "Primary chain strike failed."; return false; }
            work += first;
            var nearby = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != caster && p != target && !p.Dead && p.HostileTo(caster) &&
                    p.Position.DistanceTo(center) <= power.areaRadius &&
                    GenSight.LineOfSight(center, p.Position, caster.Map))
                .OrderBy(p => p.Position.DistanceTo(center))
                .Take(Math.Max(0, Math.Min(5, power.maxExtraTargets))).ToList();
            int hitCount = 1;
            foreach (var other in nearby)
            {
                if (Damage(caster, other, power.damageDefName,
                    power.magnitude * 0.7f * powerScale, out _, out float dealt))
                { work += dealt; hitCount++; }
            }
            reason = "Chain struck " + hitCount + " pawn(s) for " +
                work.ToString("0.#") + " effective damage.";
            return true;
        }

        private static bool SplashDamage(Pawn caster, Pawn target, EssencePowerDef power,
            float powerScale, out string reason, out float work)
        {
            work = 0f;
            IntVec3 center = target.Position;
            if (!Damage(caster, target, power.damageDefName,
                    power.magnitude * powerScale, out _, out float first))
            { reason = "Primary area strike failed."; return false; }
            work += first;
            int hitCount = 1;
            var nearby = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => p != caster && p != target && !p.Dead && p.HostileTo(caster) &&
                    p.Position.DistanceTo(center) <= power.areaRadius &&
                    GenSight.LineOfSight(center, p.Position, caster.Map)).ToList();
            foreach (Pawn other in nearby)
            {
                if (Damage(caster, other, power.damageDefName,
                    power.magnitude * 0.5f * powerScale, out _, out float dealt))
                { work += dealt; hitCount++; }
            }
            reason = "Burst struck " + hitCount + " pawn(s) for " +
                work.ToString("0.#") + " effective damage.";
            return true;
        }

        private static bool ApplyRegeneration(Pawn caster, Pawn target, EssencePowerDef power,
            EssenceAbilitySlot slot, int duration, float recoveryScale, out string reason)
        {
            var def = DefDatabase<HediffDef>.GetNamedSilentFail(power.buffHediffDefName);
            if (def == null)
            { reason = "Missing regeneration effect."; return false; }
            var effect = target.health.hediffSet.GetFirstHediffOfDef(def) as Hediff_EssenceRegeneration;
            if (effect == null)
            {
                effect = HediffMaker.MakeHediff(def, target) as Hediff_EssenceRegeneration;
                if (effect == null)
                { reason = "Regeneration effect is not available."; return false; }
                target.health.AddHediff(effect);
            }
            effect.Refresh(duration, power.magnitude * recoveryScale, caster, slot);
            reason = "Regeneration applied; training is earned only when injuries actually heal.";
            return true;
        }

        private static bool ApplyAuraBuff(Pawn caster, EssencePowerDef power,
            EssenceAbilitySlot slot, int duration, out string reason)
        {
            var friends = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => !p.Dead && !p.HostileTo(caster) &&
                    p.Position.DistanceTo(caster.Position) <= power.areaRadius).ToList();
            int applied = 0;
            foreach (Pawn ally in friends)
            {
                if (ApplyTimed(ally, power.buffHediffDefName, duration, caster, slot, true))
                    applied++;
            }
            reason = applied > 0
                ? "Protected " + applied + " nearby pawn(s); training accrues while active."
                : "No valid pawns received the aura.";
            return applied > 0;
        }

        private static bool CleanseArea(Pawn caster, EssencePowerDef power,
            out string reason, out float work)
        {
            work = 0f;
            var debuffs = RemovableDebuffs;
            int removed = 0;
            var friends = caster.Map.mapPawns.AllPawnsSpawned
                .Where(p => !p.Dead && !p.HostileTo(caster) &&
                    p.Position.DistanceTo(caster.Position) <= power.areaRadius).ToList();
            foreach (Pawn ally in friends)
            {
                var active = ally.health.hediffSet.hediffs
                    .Where(h => debuffs.Contains(h.def.defName)).ToList();
                foreach (Hediff affliction in active)
                {
                    ally.health.RemoveHediff(affliction);
                    removed++;
                    work += 10f;
                }
            }
            reason = removed > 0
                ? "Cleansed " + removed + " essence affliction(s)."
                : "No removable Essencebound afflictions nearby.";
            return removed > 0;
        }

        private static bool Damage(Pawn caster, Pawn target, string damageName,
            float amount, out string message, out float work)
        {
            work = 0f;
            string resolvedDamageName = damageName == "Burn" ? "Flame" : damageName;
            var def = DefDatabase<DamageDef>.GetNamedSilentFail(resolvedDamageName);
            if (def == null)
            {
                message = "Missing RimWorld damage definition: " + resolvedDamageName;
                Log.Warning("[Essencebound] " + message);
                return false;
            }
            // Prefer RimWorld's actual damage result. Some unusual damage
            // handlers may report zero even though injury severity increased,
            // so measure the injury change as a safe compatibility fallback.
            float woundsBefore = target.health.hediffSet.hediffs
                .OfType<Hediff_Injury>().Sum(i => i.Severity);
            var result = target.TakeDamage(new DamageInfo(def, amount, 0f, -1f, caster));
            float inflicted = Math.Max(0f, result.totalDamageDealt);
            float woundsAfter = target.health.hediffSet.hediffs
                .OfType<Hediff_Injury>().Sum(i => i.Severity);
            if (inflicted <= 0f)
                inflicted = Math.Max(0f, woundsAfter - woundsBefore);
            // Every selected pawn (ally, neutral or hostile) gives progress
            // only when the hit actually inflicted health damage.
            work = inflicted;
            message = inflicted > 0f
                ? "Hit " + target.LabelShort + " for " + inflicted.ToString("0.#") + " damage."
                : "Hit " + target.LabelShort + ", but the damage was resisted or absorbed.";
            return true;
        }

        private static bool ApplyTimed(Pawn pawn, string defName, int durationTicks,
            Pawn caster, EssenceAbilitySlot slot, bool trainSupport,
            bool hostileOnly = false)
        {
            var def = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
            if (def == null) return false;
            var active = pawn.health.hediffSet.GetFirstHediffOfDef(def) as Hediff_EssenceTimed;
            if (active == null)
            {
                active = HediffMaker.MakeHediff(def, pawn) as Hediff_EssenceTimed;
                if (active == null) return false;
                pawn.health.AddHediff(active);
            }
            active.Refresh(durationTicks, trainSupport ? caster : null,
                trainSupport ? slot : null, hostileOnly);
            return true;
        }
    }
}
