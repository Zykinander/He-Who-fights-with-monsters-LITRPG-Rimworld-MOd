using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// Rank-weighted, independent rolls for a normal essence and an awakening
    /// stone. A corpse may yield both items, just one, or nothing.
    /// </summary>
    public static class EssenceCreatureLoot
    {
        // Indices 0..5 correspond to Unranked, Iron, Bronze, Silver, Gold, Diamond.
        // Essence crystals are much rarer than awakening stones at every tier:
        // approximately 5-10 times less likely from ordinary creature deaths.
        private static readonly float[] EssenceChances =
            { 0.001f, 0.01f, 0.02f, 0.04f, 0.07f, 0.12f };
        private static readonly float[] StoneChances =
            { 0.01f, 0.08f, 0.16f, 0.26f, 0.40f, 0.60f };

        // Rarity weights for each rank: Common / Uncommon / Rare / Epic / Legendary.
        // A higher rank provides access to rarer stones, not a guaranteed drop.
        private static readonly int[][] StoneRarityWeights =
        {
            new[] { 100, 0, 0, 0, 0 },
            new[] { 85, 15, 0, 0, 0 },
            new[] { 55, 35, 10, 0, 0 },
            new[] { 25, 40, 30, 5, 0 },
            new[] { 10, 20, 40, 25, 5 },
            new[] { 5, 10, 30, 40, 15 }
        };
        private static readonly string[] RarityNames =
            { "common", "uncommon", "rare", "epic", "legendary" };

        public static float EssenceChance(int rank, bool manifestation = false)
        {
            float chance = EssenceChances[Math.Max(0, Math.Min(5, rank + 1))];
            return Math.Min(0.95f, chance * (manifestation ? 1.20f : 1f));
        }

        public static float StoneChance(int rank, bool manifestation = false)
        {
            float chance = StoneChances[Math.Max(0, Math.Min(5, rank + 1))];
            return Math.Min(0.80f, chance * (manifestation ? 1.20f : 1f));
        }

        // The optional multiplier lets Astral Loot make a separate, reduced
        // extraction roll after ordinary death drops without guaranteeing loot.
        public static bool RollDrops(int rank, bool manifestation, IntVec3 cell,
            Map map, out string description, float chanceMultiplier = 1f,
            string archetypeId = null)
        {
            description = "nothing";
            if (map == null || !cell.IsValid || !cell.InBounds(map)) return false;

            var found = new List<string>();
            if (Rand.Value < EssenceChance(rank, manifestation) * chanceMultiplier &&
                MapComponent_EssenceWorld.SpawnEssence(cell, map, out string essence,
                    EssenceMonsterArchetypes.FavoredEssences(archetypeId)))
                found.Add(essence + " essence");

            if (Rand.Value < StoneChance(rank, manifestation) * chanceMultiplier &&
                TrySpawnStone(rank, cell, map, out string stone))
                found.Add(stone);

            if (found.Count == 0) return false;
            description = string.Join(" and ", found);
            return true;
        }

        private static bool TrySpawnStone(int rank, IntVec3 center, Map map,
            out string label)
        {
            label = "";
            int[] weights = StoneRarityWeights[Math.Max(0, Math.Min(5, rank + 1))];
            int roll = Rand.Range(0, weights.Sum());
            int rarityIndex = 0;
            while (rarityIndex < weights.Length - 1 && roll >= weights[rarityIndex])
            {
                roll -= weights[rarityIndex];
                rarityIndex++;
            }

            // Only select defs with a real matching item, even if other mods
            // add custom stone definitions without corresponding ThingDefs.
            var options = DefDatabase<AwakeningStoneDef>.AllDefsListForReading
                .Where(stone => stone != null &&
                    string.Equals(stone.rarity, RarityNames[rarityIndex],
                        StringComparison.OrdinalIgnoreCase) &&
                    StoneItemDef(stone) != null)
                .OrderBy(stone => stone.defName).ToList();

            // A missing rarity tier should not lose a successful stone roll.
            if (options.Count == 0)
                options = DefDatabase<AwakeningStoneDef>.AllDefsListForReading
                    .Where(stone => stone != null && StoneItemDef(stone) != null)
                    .OrderBy(stone => stone.defName).ToList();
            if (options.Count == 0) return false;

            AwakeningStoneDef chosen = options[Rand.Range(0, options.Count)];
            ThingDef def = StoneItemDef(chosen);
            if (def == null || !TryDropItem(def, center, map)) return false;
            label = chosen.label;
            return true;
        }

        private static ThingDef StoneItemDef(AwakeningStoneDef stone)
        {
            if (stone == null || !stone.defName.StartsWith("EB_Stone_", StringComparison.Ordinal))
                return null;
            return DefDatabase<ThingDef>.GetNamedSilentFail(
                "EB_AwakeningStone_" + stone.defName.Substring("EB_Stone_".Length));
        }

        private static bool TryDropItem(ThingDef def, IntVec3 center, Map map)
        {
            IntVec3 cell;
            if (!MapComponent_EssenceWorld.TryFindNear(center, map, out cell))
            {
                if (!center.Standable(map)) return false;
                cell = center;
            }
            GenSpawn.Spawn(ThingMaker.MakeThing(def), cell, map);
            return true;
        }
    }

    /// <summary>
    /// One roll per actual death, not on corpse spawn/reload. This also avoids
    /// rerolling loot when corpses are hauled, moved, or saved and loaded.
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
    public static class Patch_EssenceCreatureDeathLoot
    {
        private static void Prefix(Pawn __instance, out bool __state)
        {
            __state = __instance != null && !__instance.Dead &&
                __instance.RaceProps != null && !__instance.RaceProps.Humanlike;
        }

        private static void Postfix(Pawn __instance, bool __state)
        {
            if (!__state || __instance == null || !__instance.Dead) return;
            Corpse corpse = __instance.Corpse;
            if (corpse == null || !corpse.Spawned || corpse.Map == null) return;

            var marker = __instance.health?.hediffSet?.hediffs
                ?.OfType<Hediff_AstralManifestation>().FirstOrDefault();
            bool manifestation = marker != null;
            int rank = EssenceRankCombat.RankOf(__instance);
            if (EssenceCreatureLoot.RollDrops(rank, manifestation, corpse.Position,
                corpse.Map, out string found, 1f, marker?.archetypeId))
            {
                // Do not spam the event log for ordinary small animal deaths.
                if (manifestation || rank >= 3)
                    Messages.Message(corpse.LabelShort + " dropped " + found + ".",
                        RimWorld.MessageTypeDefOf.PositiveEvent, false);
            }
        }
    }
}
