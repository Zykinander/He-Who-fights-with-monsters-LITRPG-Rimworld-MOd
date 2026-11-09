using System;
using Verse;

namespace Essencebound
{
    // Rank probabilities depend on the density recorded by an opened tear.
    // Probabilities in each band add to 100. A low-density map can rarely
    // produce a Bronze monster, but never a sudden Diamond encounter.
    public static class EssenceMonsterRanks
    {
        public static int[] WeightsForDensity(float density)
        {
            float d = Math.Max(0f, Math.Min(1f, density));
            if (d < 0.25f) return new[] { 95, 5, 0, 0, 0 };
            if (d < 0.50f) return new[] { 58, 39, 3, 0, 0 };
            if (d < 0.75f) return new[] { 15, 55, 27, 3, 0 };
            if (d < 0.95f) return new[] { 0, 15, 65, 19, 1 };
            return new[] { 0, 0, 20, 75, 5 };
        }

        public static int Roll(float density)
        {
            int[] weights = WeightsForDensity(density);
            int roll = Rand.Range(0, 100);
            for (int rank = 0; rank < weights.Length; rank++)
            {
                roll -= weights[rank];
                if (roll < 0) return rank;
            }
            return 0;
        }

        public static string RankLabel(int rank)
        {
            return EssenceProgression.RankLabel(rank);
        }

        public static string PlaceholderPawnKind(int rank)
        {
            if (rank <= 0) return "Megascarab";
            if (rank == 1) return "Warg";
            return "Thrumbo";
        }

        public static int GroupSize(int rank)
        {
            // High ranks escalate cautiously: two gold / diamond monsters;
            // no three-creature Diamond wave before distinct bosses exist.
            return rank >= 3 ? 2 : 1;
        }
    }
}
