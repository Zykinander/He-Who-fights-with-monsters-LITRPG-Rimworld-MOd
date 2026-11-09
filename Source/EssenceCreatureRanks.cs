using System;
using System.Collections.Generic;
using Verse;

namespace Essencebound
{
    // Optional XML extension for modded races (on their ThingDef):
    // <modExtensions><li Class="Essencebound.EssenceCreatureRankExtension">
    //   <rankIndex>2</rankIndex></li></modExtensions>
    // -1 = unranked, 0 = Iron, 1 = Bronze, 2 = Silver, 3 = Gold, 4 = Diamond.
    public sealed class EssenceCreatureRankExtension : DefModExtension
    {
        public int rankIndex = -1;
    }

    /// <summary>
    /// Creature rank is inherent to the species, not randomly rolled per spawn.
    /// The explicit vanilla table wins over all fallback stats; modded races can
    /// override it through EssenceCreatureRankExtension on their ThingDef.
    /// </summary>
    public static class EssenceCreatureRanks
    {
        private static readonly Dictionary<string, int> SpeciesRanks =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                // Unranked wildlife and small pets.
                { "Squirrel", -1 }, { "Rat", -1 }, { "Hare", -1 },
                { "Snowhare", -1 }, { "GuineaPig", -1 }, { "Chinchilla", -1 },
                { "Chicken", -1 }, { "Duck", -1 }, { "Turkey", -1 },
                { "Cat", -1 }, { "YorkshireTerrier", -1 },

                // Iron: ordinary or lightly dangerous animals.
                { "Boomrat", 0 }, { "Raccoon", 0 }, { "Tortoise", 0 },
                { "FennecFox", 0 }, { "RedFox", 0 }, { "ArcticFox", 0 },
                { "Alpaca", 0 }, { "Goat", 0 }, { "Sheep", 0 },
                { "Pig", 0 }, { "LabradorRetriever", 0 }, { "Husky", 0 },
                { "Megascarab", 0 },

                // Bronze: pack animals, predators and sturdy livestock.
                { "Boomalope", 1 }, { "Muffalo", 1 }, { "Bison", 1 },
                { "Horse", 1 }, { "Donkey", 1 }, { "Dromedary", 1 },
                { "Caribou", 1 }, { "Elk", 1 }, { "Deer", 1 },
                { "WildBoar", 1 }, { "TimberWolf", 1 }, { "ArcticWolf", 1 },
                { "Cougar", 1 }, { "Panther", 1 }, { "Lynx", 1 },
                { "Warg", 1 }, { "Spelopede", 1 },

                // Silver: heavyweight threats.
                { "GrizzlyBear", 2 }, { "PolarBear", 2 },
                { "Bear", 2 }, { "Gorilla", 2 }, { "Megaspider", 2 },

                // Gold: exceptional, very large creatures.
                { "Elephant", 3 }, { "Rhinoceros", 3 }, { "Megasloth", 3 },

                // Diamond: rare legendary wildlife.
                { "Thrumbo", 4 }
            };

        public static int InherentRankOf(Pawn pawn)
        {
            return pawn == null ? -1 : InherentRankFor(pawn.def, pawn.kindDef);
        }

        public static int InherentRankFor(ThingDef race, PawnKindDef kind)
        {
            // Humanlike pawns remain governed only by their actual essences.
            if (race?.race == null || race.race.Humanlike) return -1;

            var overrideRank = race.GetModExtension<EssenceCreatureRankExtension>();
            if (overrideRank != null)
                return Clamp(overrideRank.rankIndex);

            if (SpeciesRanks.TryGetValue(race.defName, out int preset))
                return preset;

            // For non-vanilla creatures without an explicit override, use their
            // pawn-kind threat budget. These values are species/kind definitions,
            // not the individual creature's health, age, or temporary bonuses.
            float threat = kind?.combatPower ?? 0f;
            if (threat > 0f)
            {
                if (threat < 45f) return -1;
                if (threat < 100f) return 0;
                if (threat < 180f) return 1;
                if (threat < 300f) return 2;
                if (threat < 500f) return 3;
                return 4;
            }

            // A safe fallback for custom creatures with no declared combat power.
            float bodySize = race.race.baseBodySize;
            if (bodySize < 0.4f) return -1;
            if (bodySize < 1f) return 0;
            if (bodySize < 2f) return 1;
            if (bodySize < 4f) return 2;
            return 3;
        }

        public static string RankLabel(int rank)
        {
            return rank < 0 ? "Unranked" : EssenceProgression.RankLabel(Clamp(rank));
        }

        private static int Clamp(int rank)
        {
            return Math.Max(-1, Math.Min(4, rank));
        }
    }
}
