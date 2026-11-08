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
            if (!ValidateTarget(caster, power, target, out reason)) return false;
            if (power.targetKind == "self") target = caster;
            switch (power.effectKind)
            {
                case "damage":
                    return Damage(caster, target, power.damageDefName, power.magnitude, out reason);
                case "damageDebuff":
                    if (!Damage(caster, target, power.damageDefName, power.magnitude, out reason)) return false;
                    if (!target.Dead) ApplyTimed(target, power.buffHediffDefName, power.durationTicks);
                    reason = "Effect applied.";
                    return true;
                case "heal":
                    var injury = target.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                        .Where(i => !i.IsPermanent())
                        .OrderByDescending(i => i.Severity).FirstOrDefault();
                    if (injury == null) { reason = "Target has no healable injury."; return false; }
                    injury.Heal(power.magnitude);
                    reason = "Injury healed.";
                    return true;
                case "buff":
                    if (!ApplyTimed(target, power.buffHediffDefName, power.durationTicks))
                    { reason = "Missing status definition for this power."; return false; }
                    reason = "Temporary boon applied.";
                    return true;
                case "cleanse":
                    var badNames = new HashSet<string> { "EB_Debuff_Chilled", "EB_Debuff_Static" };
                    var affliction = target.health.hediffSet.hediffs.FirstOrDefault(h => badNames.Contains(h.def.defName));
                    if (affliction == null) { reason = "Target has no Essencebound affliction to cleanse."; return false; }
                    target.health.RemoveHediff(affliction);
                    reason = "Essencebound affliction removed.";
                    return true;
                case "pulse":
                    var hostilePawns = caster.Map.mapPawns.AllPawnsSpawned
                        .Where(p => p != caster && !p.Dead && p.HostileTo(caster) &&
                                    p.Position.DistanceTo(caster.Position) <= power.range).ToList();
                    if (hostilePawns.Count == 0)
                    { reason = "No hostile pawns are within pulse range."; return false; }
                    foreach (Pawn other in hostilePawns) Damage(caster, other, power.damageDefName, power.magnitude, out _);
                    reason = "Pulse affected " + hostilePawns.Count + " hostile pawn(s).";
                    return true;
                default:
                    reason = "Unknown effect kind: " + power.effectKind;
                    return false;
            }
        }

        private static bool Damage(Pawn caster, Pawn target, string damageName, float amount, out string message)
        {
            // Backwards compatibility for v0.6's incorrect damage definition.
            string resolvedDamageName = damageName == "Burn" ? "Flame" : damageName;
            var def = DefDatabase<DamageDef>.GetNamedSilentFail(resolvedDamageName);
            if (def == null)
            {
                message = "Missing RimWorld damage definition: " + resolvedDamageName;
                Log.Warning("[Essencebound] " + message);
                return false;
            }
            var result = target.TakeDamage(new DamageInfo(def, amount, 0f, -1f, caster));
            float inflicted = result.totalDamageDealt;
            message = inflicted > 0f
                ? "Hit " + target.LabelShort + " for " + inflicted.ToString("0.#") + " damage."
                : "Hit " + target.LabelShort + ", but the damage was resisted or absorbed.";
            return true;
        }

        private static bool ApplyTimed(Pawn pawn, string defName, int durationTicks)
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
            active.Refresh(durationTicks);
            return true;
        }
    }
}
