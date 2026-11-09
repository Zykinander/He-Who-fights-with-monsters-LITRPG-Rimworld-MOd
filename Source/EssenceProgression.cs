using System;
using Verse;

namespace Essencebound
{
    public sealed class EssenceAffinityRecord : IExposable
    {
        public string essenceDefName = "";
        public string attribute = "";
        public void ExposeData()
        {
            Scribe_Values.Look(ref essenceDefName, "essenceDefName", "");
            Scribe_Values.Look(ref attribute, "attribute", "");
        }
    }

    public sealed class EssenceRankRecord : IExposable
    {
        public string essenceDefName = "";
        public int rankIndex;
        // Never decreases automatically: temporarily lowering the global slot cap
        // must not allow players to skip already-required abilities.
        public int requiredSlotCount = 3;
        public void ExposeData()
        {
            Scribe_Values.Look(ref essenceDefName, "essenceDefName", "");
            Scribe_Values.Look(ref rankIndex, "rankIndex", 0);
            Scribe_Values.Look(ref requiredSlotCount, "requiredSlotCount", 3);
            rankIndex = Math.Max(0, Math.Min(4, rankIndex));
            requiredSlotCount = Math.Max(2, Math.Min(5, requiredSlotCount));
        }
    }

    public static class EssenceProgression
    {
        public static readonly string[] Ranks = { "Iron", "Bronze", "Silver", "Gold", "Diamond" };
        private static readonly float[] Thresholds = { 40f, 120f, 360f, 1080f };

        public static float NextRankWork(int currentRank)
        {
            return currentRank < 0 || currentRank >= 4 ? 0f : Thresholds[currentRank];
        }

        // Ten equal training steps within each rank. The total per-major-rank
        // work remains unchanged from v0.11.
        public static float NextSublevelWork(int currentRank)
        {
            return NextRankWork(currentRank) / 10f;
        }

        public static string RankWithLevel(int majorRank, int sublevel)
        {
            return RankLabel(majorRank) + " " +
                Math.Max(0, Math.Min(9, majorRank >= 4 ? 0 : sublevel));
        }

        // Smoothly approach the NEXT tier's doubled bonus.
        // Iron 0 = x1; Iron 1 = x1.1; Iron 9 = x1.9; Bronze 0 = x2.
        // Bronze 9 = x3.8; Silver 0 = x4, etc.
        public static float RankBonusMultiplier(int rankIndex, int sublevel)
        {
            float major = RankBonusMultiplier(rankIndex);
            if (rankIndex >= 4) return major;
            return major * (1f + Math.Max(0, Math.Min(9, sublevel)) / 10f);
        }

        public static string RankLabel(int index)
        {
            return Ranks[Math.Max(0, Math.Min(4, index))];
        }

        public static string RollAttribute(EssenceDef essence)
        {
            if (essence == null) return "Spirit";
            // The same essence can roll differently for two pawns. The roll is
            // performed once, then its result is stored on the pawn's hediff.
            float power = Math.Max(0.01f, essence.powerWeight);
            float speed = Math.Max(0.01f, essence.speedWeight);
            float spirit = Math.Max(0.01f, essence.spiritWeight);
            float recovery = Math.Max(0.01f, essence.recoveryWeight);
            float roll = Rand.Value * (power + speed + spirit + recovery);
            if ((roll -= power) < 0f) return "Power";
            if ((roll -= speed) < 0f) return "Speed";
            return (roll -= spirit) < 0f ? "Spirit" : "Recovery";
        }

        public static float BonusPerTier(string attribute)
        {
            switch (attribute)
            {
                case "Power": return 0.04f;      // Damage dealt by essence abilities at Iron
                case "Speed": return 0.025f;     // Ability cooldown reduction at Iron
                case "Spirit": return 0.05f;     // Positive buff duration at Iron
                case "Recovery": return 0.06f;   // Effective healing at Iron
                default: return 0f;
            }
        }

        // Iron=1, Bronze=2, Silver=4, Gold=8, Diamond=16.
        // These are the total bonuses at each rank, not cumulative rank-ups.
        public static int RankBonusMultiplier(int rankIndex)
        {
            int clamped = Math.Max(0, Math.Min(4, rankIndex));
            return 1 << clamped;
        }

        // A pawn can possess multiple essences with the same attribute.
        // Never let cooldown reduction reach 100%; other bonuses allow
        // the full stacked values of four Diamond-rank essences.
        public static float MaxAttributeBonus(string attribute)
        {
            return attribute == "Speed" ? 0.80f : 4.00f;
        }

        public static string WorkLabel(string effectKind)
        {
            switch (effectKind)
            {
                case "damage":
                case "damageDebuff":
                case "pulse":
                case "chain":
                case "splash":
                case "drain":
                case "damageAreaDebuff":
                case "leechPulse":
                case "execute":
                case "delayedBlast":
                case "blinkStrike":
                case "hazardZone":
                case "piercingSound":
                case "radiantPurge":
                case "sustainedLeech":
                case "armorBreak":
                case "orbitStrike":
                case "sustainedDebuff":
                case "meleeCombo": return "effective damage";
                case "heal":
                case "healGroup":
                case "regeneration":
                case "areaHeal":
                case "chainHeal":
                case "areaRegen":
                case "healingZone":
                case "branchingHeal":
                case "transfusion":
                case "restorationZone":
                case "emergencyHeal":
                case "survivalBoon": return "actual healing";
                case "healWard": return "healing plus ward duration";
                case "buff":
                case "auraBuff":
                case "reflect":
                case "adaptiveBuff":
                case "powerAura":
                case "wardDome":
                case "markedTarget":
                case "concealment":
                case "conditionalBuff":
                case "harvestAura":
                case "linkedDefense":
                case "counterStance":
                case "attributeShift":
                case "mobileWard":
                case "storedTrigger":
                case "teamLink":
                case "debuffReflect": return "active support and triggered protection";
                case "debuff":
                case "containment":
                case "summonOrder":
                case "grapple":
                case "confusion":
                case "webZone":
                case "slowField":
                case "pullZone": return "active control";
                case "cleanse":
                case "areaCleanse":
                case "purgeHeal": return "healing and cleansing work";
                case "summon":
                case "decoy":
                case "familiar":
                case "summonFlock": return "active summoned assistance";
                case "returnTeleport":
                case "teleport":
                case "remoteManipulation":
                case "tileBarrier":
                case "survey":
                case "swapTeleport":
                case "groundWard":
                case "longTeleport": return "successful utility work";
                default: return "progress";
            }
        }
    }
}
