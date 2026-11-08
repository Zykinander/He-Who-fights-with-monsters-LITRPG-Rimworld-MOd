using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Essencebound
{
    public static class EssencePowerCatalog
    {
        public sealed class AwakeningOption
        {
            public AwakeningAspectDef aspect;
            public EssencePowerDef power;
        }

        public static List<AwakeningOption> GetOptions(
            AwakeningStoneDef stone, string essenceName, IEnumerable<EssenceAbilitySlot> slots)
        {
            var options = new List<AwakeningOption>();
            if (stone?.possibleAspects == null || string.IsNullOrEmpty(essenceName)) return options;
            var usedAspects = new HashSet<string>(StringComparer.Ordinal);
            var usedPowers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var slot in slots ?? Enumerable.Empty<EssenceAbilitySlot>())
            {
                if (slot == null) continue;
                if (!string.IsNullOrEmpty(slot.awakeningAspectDefName)) usedAspects.Add(slot.awakeningAspectDefName);
                if (!string.IsNullOrEmpty(slot.abilityDefName)) usedPowers.Add(slot.abilityDefName);
            }
            foreach (var aspect in stone.possibleAspects.Where(a => a != null && !usedAspects.Contains(a.defName)))
            foreach (var power in DefDatabase<EssencePowerDef>.AllDefsListForReading)
            {
                if (usedPowers.Contains(power.defName)) continue;
                if (power.compatibleAspects != null && power.compatibleAspects.Contains(aspect))
                    options.Add(new AwakeningOption { aspect = aspect, power = power });
            }
            return options;
        }

        public static AwakeningOption PickOption(AwakeningStoneDef stone, string essenceName,
            IEnumerable<EssenceAbilitySlot> slots)
        {
            var options = GetOptions(stone, essenceName, slots);
            if (options.Count == 0) return null;
            // Favor an essence-aligned power without making every pawn identical.
            var favored = options.Where(x => x.power.favoredEssences != null &&
                x.power.favoredEssences.Any(e => e != null && e.defName == essenceName)).ToList();
            var pool = favored.Count > 0 ? favored : options;
            // Keep the stone's listed aspect order as a weak, predictable preference.
            bool stoneFavored = stone.favoredEssences != null &&
                stone.favoredEssences.Any(e => e != null && e.defName == essenceName);
            if (stoneFavored)
            {
                var first = stone.possibleAspects.FirstOrDefault(a => a != null && pool.Any(x => x.aspect == a));
                if (first != null) pool = pool.Where(x => x.aspect == first).ToList();
            }
            return pool[Rand.Range(0, pool.Count)];
        }

        // Save compatibility: old v0.3-v0.5 imprint slots become playable without replacing stones.
        public static void BackfillLegacyPowers(List<EssenceAbilitySlot> slots)
        {
            if (slots == null) return;
            foreach (var slot in slots.Where(s => s != null && s.IsAwakened &&
                string.IsNullOrEmpty(s.abilityDefName)).OrderBy(s => s.essenceDefName).ThenBy(s => s.index))
            {
                var aspect = DefDatabase<AwakeningAspectDef>.GetNamedSilentFail(slot.awakeningAspectDefName);
                if (aspect == null) continue;
                var used = new HashSet<string>(slots.Where(s => s != null &&
                    s.essenceDefName == slot.essenceDefName && !string.IsNullOrEmpty(s.abilityDefName))
                    .Select(s => s.abilityDefName), StringComparer.Ordinal);
                var choices = DefDatabase<EssencePowerDef>.AllDefsListForReading
                    .Where(p => !used.Contains(p.defName) && p.compatibleAspects != null &&
                           p.compatibleAspects.Contains(aspect))
                    .OrderByDescending(p => p.favoredEssences != null &&
                        p.favoredEssences.Any(e => e != null && e.defName == slot.essenceDefName))
                    .ThenBy(p => p.defName)
                    .ToList();
                if (choices.Count > 0) slot.abilityDefName = choices[0].defName;
            }
        }
    }
}
