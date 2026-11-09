using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Persists defensive triggers, ally links, emergency recovery and
    // anti-curse reactions. No credit is awarded merely for casting.
    public sealed class Hediff_EssenceReactive : HediffWithComps
    {
        public string mode = "";
        public Pawn linkedPawn;
        public float magnitude;
        private int remainingTicks;
        private int tickCounter;
        private int combo;
        private Pawn creditCaster;
        private string creditEssence = "";
        private int creditSlot = -1;

        public void Configure(string kind, int duration, float strength,
            Pawn caster, EssenceAbilitySlot slot, Pawn link = null)
        {
            mode = kind;
            remainingTicks = Math.Max(60, duration);
            magnitude = strength;
            linkedPawn = link;
            creditCaster = caster;
            creditEssence = slot?.essenceDefName ?? "";
            creditSlot = slot?.index ?? -1;
            tickCounter = 0;
            if (kind != "meleeCombo") combo = 0;
        }

        public bool Active => remainingTicks > 0;
        public float ComboFactor => 1f + Math.Min(4, combo) * 0.18f;

        public void RecordCombo()
        {
            if (!Active) return;
            combo = Math.Min(4, combo + 1);
            remainingTicks = 480;
        }

        public void Credit(float amount)
        {
            if (amount > 0f && creditCaster != null && !creditCaster.Dead &&
                !string.IsNullOrEmpty(creditEssence) && creditSlot >= 0)
                Hediff_EssenceUser.ForPawn(creditCaster)?.AddAbilityWork(
                    creditEssence, creditSlot, amount);
        }

        public void Consume()
        {
            remainingTicks = 0;
            if (pawn?.health != null)
                pawn.health.RemoveHediff(this);
        }

        public override void Tick()
        {
            base.Tick();
            if (!Active || pawn?.health == null) return;
            if (++tickCounter >= 60)
            {
                tickCounter = 0;
                if ((mode == "emergencyHeal" || mode == "survivalBoon") &&
                    pawn.Spawned && !pawn.Dead &&
                    pawn.health.summaryHealth.SummaryHealthPercent <=
                        (mode == "emergencyHeal" ? 0.65f : 0.45f))
                {
                    if (EssencePowerExecutor.FieldHealing(pawn,
                        Math.Max(1f, magnitude), out float healed))
                        Credit(healed);
                    Consume();
                    return;
                }
                if (mode == "debuffReflect" && pawn.Spawned && !pawn.Dead)
                {
                    var negatives = pawn.health.hediffSet.hediffs
                        .OfType<Hediff_EssenceTimed>()
                        .Where(h => h.def.isBad &&
                            h.def.defName.StartsWith("EB_Debuff_"))
                        .Take(2).ToList();
                    foreach (var negative in negatives)
                    {
                        Pawn source = negative.CreditCaster;
                        string defName = negative.def.defName;
                        pawn.health.RemoveHediff(negative);
                        if (source != null && source.Spawned && !source.Dead &&
                            source.Map == pawn.Map && source.HostileTo(pawn))
                            EssencePowerExecutor.AdvancedTimed(source, defName,
                                300, null, null);
                        Credit(4f);
                    }
                }
            }
            if (--remainingTicks <= 0)
                Consume();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref mode, "ebReactionMode", "");
            Scribe_Values.Look(ref magnitude, "ebReactionMagnitude", 0f);
            Scribe_Values.Look(ref remainingTicks, "ebReactionTicks", 0);
            Scribe_Values.Look(ref tickCounter, "ebReactionTickCounter", 0);
            Scribe_Values.Look(ref combo, "ebReactionCombo", 0);
            Scribe_References.Look(ref linkedPawn, "ebReactionLinkPawn");
            Scribe_References.Look(ref creditCaster, "ebReactionCaster");
            Scribe_Values.Look(ref creditEssence, "ebReactionEssence", "");
            Scribe_Values.Look(ref creditSlot, "ebReactionSlot", -1);
        }
    }

    // Runs through the same shared pawn damage hook as rank mitigation.
    // Re-entrancy protection prevents retaliation and reflected hits from
    // recursively triggering other reactive wards.
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_EssenceReactiveDamage
    {
        [ThreadStatic] private static bool applyingCounter;

        private static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (applyingCounter || __instance?.health?.hediffSet == null ||
                dinfo.Amount <= 0f || !(dinfo.Instigator is Pawn attacker) ||
                attacker == __instance || attacker.Dead || !attacker.Spawned ||
                !__instance.Spawned || attacker.Map != __instance.Map) return;
            var wards = __instance.health.hediffSet.hediffs
                .OfType<Hediff_EssenceReactive>().Where(h => h.Active).ToList();
            foreach (var ward in wards)
            {
                if (ward.mode == "storedTrigger")
                {
                    float absorbed = dinfo.Amount * 0.55f;
                    dinfo.SetAmount(dinfo.Amount - absorbed);
                    ward.Credit(absorbed);
                    ward.Consume();
                    break;
                }
                if (ward.mode == "linkedDefense" &&
                    ward.linkedPawn != null && !ward.linkedPawn.Dead &&
                    ward.linkedPawn.Spawned && ward.linkedPawn.Map == __instance.Map)
                {
                    float shared = dinfo.Amount * 0.15f;
                    dinfo.SetAmount(dinfo.Amount - shared);
                    try
                    {
                        applyingCounter = true;
                        ward.linkedPawn.TakeDamage(new DamageInfo(dinfo.Def,
                            shared, 0f, -1f, attacker));
                        ward.Credit(shared * 0.5f);
                    }
                    finally { applyingCounter = false; }
                }
                if (ward.mode == "counterStance" &&
                    __instance.Position.DistanceTo(attacker.Position) <= 2.5f)
                {
                    float absorbed = dinfo.Amount * 0.12f;
                    dinfo.SetAmount(dinfo.Amount - absorbed);
                    try
                    {
                        applyingCounter = true;
                        if (EssencePowerExecutor.FieldDamage(__instance, attacker,
                            "Blunt", Math.Max(1f, ward.magnitude), out float inflicted))
                            ward.Credit(inflicted);
                    }
                    finally { applyingCounter = false; }
                }
            }
        }
    }
}
