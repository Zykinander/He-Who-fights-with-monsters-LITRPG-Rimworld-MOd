using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Essencebound
{
    // New states append to the historical enum to keep save/profile wording
    // compatible. A generated confluence is playable, not an unresolved slot.
    public enum ConfluenceState
    {
        Incomplete, Unknown, Resolved, Ambiguous, Generated, ConflictResolved
    }

    public sealed class ConfluenceOutcome
    {
        public ConfluenceState state;
        public EssenceDef essence;
        public bool restricted;
        public string restrictionStatus = "not_identified";
        public int matchingRecipes;
    }

    public static class ConfluenceResolver
    {
        /// <summary>
        /// Every set of three different normal essences resolves to one of the
        /// available confluence essences. Explicit recipes take priority; missing
        /// and conflicting recipes have deterministic, order-independent results.
        /// No per-combination XML or random saved seed is required.
        /// </summary>
        public static ConfluenceOutcome Resolve(IReadOnlyList<EssenceDef> inputs)
        {
            var outcome = new ConfluenceOutcome { state = ConfluenceState.Incomplete };
            if (inputs == null || inputs.Count != 3 ||
                inputs.Any(e => e == null || e.isConfluence || string.IsNullOrEmpty(e.defName)))
                return outcome;

            var names = new HashSet<string>(inputs.Select(e => e.defName), StringComparer.Ordinal);
            // Never allow repeated essences to form a confluence.
            if (names.Count != 3) return outcome;

            var recipes = DefDatabase<ConfluenceRecipeDef>.AllDefsListForReading
                .Where(r => r.first != null && r.second != null && r.third != null &&
                    r.result != null && r.result.isConfluence &&
                    r.first.defName != r.second.defName &&
                    r.first.defName != r.third.defName &&
                    r.second.defName != r.third.defName)
                .ToList();

            var matches = recipes.Where(r =>
                names.Contains(r.first.defName) &&
                names.Contains(r.second.defName) &&
                names.Contains(r.third.defName)).ToList();

            outcome.matchingRecipes = matches.Count;
            if (matches.Count > 0)
            {
                var options = matches.Select(r => r.result)
                    .GroupBy(e => e.defName, StringComparer.Ordinal)
                    .Select(g => g.First())
                    .OrderBy(e => e.defName, StringComparer.Ordinal).ToList();

                bool conflict = matches.Any(r => r.conflict) || options.Count > 1;
                outcome.essence = StableChoice(options, names);
                outcome.state = conflict ? ConfluenceState.ConflictResolved : ConfluenceState.Resolved;

                // Carry over restriction metadata for the selected explicit recipe.
                var selectedRecipes = matches
                    .Where(r => r.result.defName == outcome.essence.defName).ToList();
                outcome.restricted = selectedRecipes.Any(r =>
                    r.restrictionStatus == "always" || r.restrictionStatus == "combination");
                outcome.restrictionStatus = selectedRecipes
                    .FirstOrDefault(r => r.restrictionStatus != "not_identified")
                    ?.restrictionStatus ?? "not_identified";
                return outcome;
            }

            // The community catalog specifies only a small portion of all
            // possible combinations. Prefer a confluence whose known recipes
            // share the most component essences with this new triple. Break all
            // ties with a stable hash so no reload or absorption order rerolls it.
            var confluences = DefDatabase<EssenceDef>.AllDefsListForReading
                .Where(e => e != null && e.isConfluence && !string.IsNullOrEmpty(e.defName))
                .OrderBy(e => e.defName, StringComparer.Ordinal).ToList();
            if (confluences.Count == 0)
            {
                outcome.state = ConfluenceState.Unknown; // Invalid/incomplete mod Def setup only.
                return outcome;
            }

            var scores = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var confluence in confluences) scores[confluence.defName] = 0;
            foreach (var recipe in recipes)
            {
                string output = recipe.result.defName;
                if (!scores.ContainsKey(output)) continue;
                int overlap = (names.Contains(recipe.first.defName) ? 1 : 0) +
                              (names.Contains(recipe.second.defName) ? 1 : 0) +
                              (names.Contains(recipe.third.defName) ? 1 : 0);
                if (overlap > scores[output]) scores[output] = overlap;
            }
            int highest = scores.Values.Max();
            var bestMatches = confluences.Where(e => scores[e.defName] == highest).ToList();
            outcome.essence = StableChoice(bestMatches, names);
            outcome.state = ConfluenceState.Generated;
            return outcome;
        }

        private static EssenceDef StableChoice(IReadOnlyList<EssenceDef> candidates,
            IEnumerable<string> names)
        {
            if (candidates == null || candidates.Count == 0) return null;
            string key = string.Join("|", names.OrderBy(n => n, StringComparer.Ordinal));
            // FNV-1a: unlike string.GetHashCode(), this hash is stable across
            // OS versions, application restarts and save/load cycles.
            uint hash = 2166136261u;
            unchecked
            {
                foreach (char character in key)
                {
                    hash ^= character;
                    hash *= 16777619u;
                }
            }
            var ordered = candidates.OrderBy(e => e.defName, StringComparer.Ordinal).ToList();
            return ordered[(int)(hash % (uint)ordered.Count)];
        }
    }
}
