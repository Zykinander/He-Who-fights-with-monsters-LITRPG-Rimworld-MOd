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
                    var badNames = new HashSet<string> { "EB_Debuff_Chilled", "EB_Debuff_Static" };
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
            var result = target.TakeDamage(new DamageInfo(def, amount, 0f, -1f, caster));
            float inflicted = Math.Max(0f, result.totalDamageDealt);
            // Damage to friendly/neutral pawns is still allowed, but doesn't
            // train attacks. Prevents the easiest friendly-fire XP exploit.
            work = target.HostileTo(caster) ? inflicted : 0f;
            message = inflicted > 0f
                ? "Hit " + target.LabelShort + " for " + inflicted.ToString("0.#") + " damage."
                : "Hit " + target.LabelShort + ", but the damage was resisted or absorbed.";
            return true;
        }

        private static bool ApplyTimed(Pawn pawn, string defName, int durationTicks,
            Pawn caster, EssenceAbilitySlot slot, bool trainSupport)
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
                trainSupport ? slot : null);
            return true;
        }
    }
}
