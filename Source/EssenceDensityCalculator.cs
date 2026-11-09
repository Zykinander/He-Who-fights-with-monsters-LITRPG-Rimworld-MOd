using System;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Snapshot values are derived from a colony's current strength. They are
    // recomputed when queried rather than rolled or frozen at map creation.
    public readonly struct EssenceDensityReport
    {
        public readonly int Colonists;
        public readonly int EssenceUsers;
        public readonly float ColonyWealth;
        public readonly float WealthFactor;
        public readonly float PopulationFactor;
        public readonly float CombatFactor;
        public readonly float EssenceFactor;
        public readonly float BaseDensity;
        public readonly float FinalDensity;

        public EssenceDensityReport(int colonists, int essenceUsers, float colonyWealth,
            float wealthFactor, float populationFactor, float combatFactor,
            float essenceFactor, float baseDensity, float finalDensity)
        {
            Colonists = colonists;
            EssenceUsers = essenceUsers;
            ColonyWealth = colonyWealth;
            WealthFactor = wealthFactor;
            PopulationFactor = populationFactor;
            CombatFactor = combatFactor;
            EssenceFactor = essenceFactor;
            BaseDensity = baseDensity;
            FinalDensity = finalDensity;
        }

        public string Description =>
            "Magic density: " + (FinalDensity * 100f).ToString("0.#") + "%" +
            " (raw " + (BaseDensity * 100f).ToString("0.#") + "%). " +
            "Colony wealth: " + ColonyWealth.ToString("0") + " silver. " +
            "Colonists: " + Colonists + "; essence users: " + EssenceUsers + ". " +
            "Strength factors — wealth " + (WealthFactor * 100f).ToString("0") +
            "%, population " + (PopulationFactor * 100f).ToString("0") +
            "%, combat skills " + (CombatFactor * 100f).ToString("0") +
            "%, essence ranks " + (EssenceFactor * 100f).ToString("0") + "%.";
    }

    public static class EssenceDensityCalculator
    {
        private static float Clamp01(float amount) =>
            Math.Max(0f, Math.Min(1f, amount));

        // Public and deterministic: easy to rebalance and to validate with
        // numerical test cases independently of any loaded RimWorld map.
        public static EssenceDensityReport Evaluate(float wealth, int colonists,
            float averageCombatSkill, float totalEssencePower, int essenceUsers,
            float settingMultiplier)
        {
            float safeWealth = Math.Max(0f, wealth);
            int safePeople = Math.Max(0, colonists);
            float wealthFactor = Clamp01((float)Math.Sqrt(safeWealth / 200000f));
            float populationFactor = Clamp01((float)Math.Sqrt(safePeople / 12f));
            float combatFactor = Clamp01(averageCombatSkill / 20f);
            float essenceFactor = Clamp01(totalEssencePower / 18f);

            // Low-threat starts, increasing danger as wealth, combat training
            // and especially Essencebound advancement accumulate.
            // Contributions add to 1.0 at full colony strength.
            float baseDensity = safePeople == 0 ? 0.05f :
                0.09f + 0.23f * wealthFactor + 0.14f * populationFactor +
                0.12f * combatFactor + 0.42f * essenceFactor;
            baseDensity = Clamp01(baseDensity);
            float finalDensity = Math.Max(0.05f,
                Math.Min(1f, baseDensity * Math.Max(0f, settingMultiplier)));
            return new EssenceDensityReport(safePeople, essenceUsers, safeWealth,
                wealthFactor, populationFactor, combatFactor, essenceFactor,
                baseDensity, finalDensity);
        }

        public static EssenceDensityReport ForMap(Map map)
        {
            if (map == null)
                return Evaluate(0f, 0, 0f, 0f, 0, EssenceboundMod.MagicDensityMultiplier);

            var colonists = map.mapPawns.FreeColonistsSpawned;
            int population = colonists.Count;
            float combinedCombatSkill = 0f;
            float essencePower = 0f;
            int essenceUsers = 0;
            foreach (Pawn pawn in colonists)
            {
                if (pawn == null || pawn.Dead) continue;
                int shooting = pawn.skills?.GetSkill(SkillDefOf.Shooting)?.Level ?? 0;
                int melee = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0;
                combinedCombatSkill += Math.Max(shooting, melee);

                var user = Hediff_EssenceUser.ForPawn(pawn);
                if (user == null) continue;
                essenceUsers++;
                foreach (string essence in user.OwnedEssenceNames)
                {
                    if (string.IsNullOrEmpty(essence)) continue;
                    // A newly absorbed Iron 0 essence contributes a little;
                    // Bronze, Silver, Gold and Diamond contribute progressively
                    // more, including each numbered sublevel.
                    essencePower += 0.5f + user.GetEssenceRankIndex(essence) +
                        user.GetEssenceSublevel(essence) / 10f;
                }
            }
            float wealth = map.wealthWatcher?.WealthTotal ?? 0f;
            float averageCombat = population > 0 ? combinedCombatSkill / population : 0f;
            return Evaluate(wealth, population, averageCombat,
                essencePower, essenceUsers, EssenceboundMod.MagicDensityMultiplier);
        }
    }
}
