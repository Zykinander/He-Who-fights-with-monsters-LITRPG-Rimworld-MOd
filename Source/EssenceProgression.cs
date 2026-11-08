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
                case "Power": return 0.04f;      // Damage dealt by essence abilities
                case "Speed": return 0.025f;     // Ability cooldown reduction
                case "Spirit": return 0.05f;     // Positive buff duration
                case "Recovery": return 0.06f;   // Effective healing
                default: return 0f;
            }
        }

        public static string WorkLabel(string effectKind)
        {
            switch (effectKind)
            {
                case "damage":
                case "damageDebuff":
                case "pulse": return "effective damage";
                case "heal": return "injuries healed";
                case "buff": return "seconds of active benefit";
                case "cleanse": return "cleansing work";
                default: return "progress";
            }
        }
    }
}
