using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Essencebound
{
    public enum ConfluenceState { Incomplete, Unknown, Resolved, Ambiguous }

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
        // All three inputs are treated as an unordered SET, with no repeated essence.
        // Unknown triples are NOT mapped to a guessed confluence.
        public static ConfluenceOutcome Resolve(IReadOnlyList<EssenceDef> inputs)
        {
            var result = new ConfluenceOutcome { state = ConfluenceState.Incomplete };
            if (inputs == null || inputs.Count != 3 || inputs.Any(e => e == null || e.isConfluence))
                return result;

            var names = new HashSet<string>(inputs.Select(e => e.defName), StringComparer.Ordinal);
            if (names.Count != 3) return result;

            var matches = DefDatabase<ConfluenceRecipeDef>.AllDefsListForReading
                .Where(r => r.first != null && r.second != null && r.third != null && r.result != null &&
                            names.Contains(r.first.defName) && names.Contains(r.second.defName) &&
                            names.Contains(r.third.defName) &&
                            new HashSet<string>(new[] { r.first.defName, r.second.defName, r.third.defName }).Count == 3)
                .ToList();

            result.matchingRecipes = matches.Count;
            if (matches.Count == 0)
            {
                result.state = ConfluenceState.Unknown;
                return result;
            }

            // Any explicit conflict, or >1 distinct output, is unresolved.
            if (matches.Any(r => r.conflict) || matches.Select(r => r.result.defName).Distinct().Count() != 1)
            {
                result.state = ConfluenceState.Ambiguous;
                return result;
            }

            result.state = ConfluenceState.Resolved;
            result.essence = matches[0].result;
            result.restricted = matches.Any(r => r.restrictionStatus == "always" || r.restrictionStatus == "combination");
            result.restrictionStatus = matches.FirstOrDefault(r => r.restrictionStatus != "not_identified")?.restrictionStatus
                                       ?? "not_identified";
            return result;
        }
    }
}
