using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Essencebound
{
    /// <summary>
    /// Colony-wide journal with five browsing tabs, search and a selected
    /// entry's details. Unknown entries stay masked until actually discovered.
    /// This is deliberately vanilla Window/Widgets UI (no extra dependencies).
    /// </summary>
    public sealed class Dialog_EssenceCodex : Window
    {
        private enum Section { Essences, Confluences, Stones, Abilities, Bestiary }

        private sealed class CodexRow
        {
            public string key;
            public string title;
        }

        private static readonly Color Background = new Color(0.13f, 0.15f, 0.18f, 1f);
        private static readonly Color Selected = new Color(0.24f, 0.33f, 0.37f, 1f);
        private static readonly Color Muted = new Color(0.62f, 0.66f, 0.70f, 1f);

        private Section section;
        private string search = "";
        private string selectedKey = "";
        private Vector2 listScroll;
        private Vector2 detailScroll;

        public override Vector2 InitialSize => new Vector2(1060f, 715f);

        public Dialog_EssenceCodex()
        {
            doCloseX = true;
            draggable = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            GameComponent_EssenceCodex.Current?.RefreshObservedWorld();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var journal = GameComponent_EssenceCodex.Current;
            if (journal == null)
            {
                Widgets.Label(inRect, "Load a colony to open the Essencebound Codex.");
                return;
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 32f),
                "ESSENCEBOUND CODEX");
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(inRect.x, inRect.y + 34f, inRect.width, 23f),
                "Colony-wide discoveries  •  Knowledge persists in this save  •  Unknown entries stay hidden");

            DrawTabs(new Rect(inRect.x, inRect.y + 67f, inRect.width, 37f));

            Widgets.Label(new Rect(inRect.x, inRect.y + 117f, 75f, 25f), "Search");
            Rect searchBox = new Rect(inRect.x + 76f, inRect.y + 113f,
                inRect.width * 0.43f - 82f, 30f);
            string next = Widgets.TextField(searchBox, search);
            if (next != search)
            {
                search = next;
                listScroll = Vector2.zero;
                selectedKey = "";
            }
            Widgets.Label(new Rect(inRect.x + inRect.width * 0.45f,
                inRect.y + 117f, inRect.width * 0.54f, 25f), Summary(journal));

            List<CodexRow> rows = Entries(journal);
            Rect outer = new Rect(inRect.x, inRect.y + 152f,
                inRect.width, inRect.height - 157f);
            float leftWidth = Mathf.Min(410f, outer.width * 0.41f);
            DrawList(new Rect(outer.x, outer.y, leftWidth, outer.height), rows);
            DrawDetails(new Rect(outer.x + leftWidth + 12f, outer.y,
                outer.width - leftWidth - 12f, outer.height), journal, rows);
        }

        private void DrawTabs(Rect bounds)
        {
            string[] labels = { "Essences", "Confluences", "Stones", "Abilities", "Bestiary" };
            float width = (bounds.width - 24f) / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                Rect tab = new Rect(bounds.x + i * (width + 6f), bounds.y, width, bounds.height);
                if ((int)section == i) Widgets.DrawBoxSolid(tab, Selected);
                if (Widgets.ButtonText(tab, labels[i]) && (int)section != i)
                {
                    section = (Section)i;
                    selectedKey = "";
                    search = "";
                    listScroll = Vector2.zero;
                    detailScroll = Vector2.zero;
                }
            }
        }

        private string Summary(GameComponent_EssenceCodex journal)
        {
            switch (section)
            {
                case Section.Essences:
                    return journal.NormalEssences.Count + " / " +
                        DefDatabase<EssenceDef>.AllDefsListForReading.Count(e => !e.isConfluence) +
                        " essences found";
                case Section.Confluences:
                    return journal.ConfluenceEssences.Count + " / " +
                        DefDatabase<EssenceDef>.AllDefsListForReading.Count(e => e.isConfluence) +
                        " confluences  •  " + journal.Recipes.Count + " recipes recorded";
                case Section.Stones:
                    return journal.AwakeningStones.Count + " / " +
                        DefDatabase<AwakeningStoneDef>.AllDefsListForReading.Count +
                        " stones found";
                case Section.Abilities:
                    return journal.LearnedPowers.Count + " / " +
                        DefDatabase<EssencePowerDef>.AllDefsListForReading.Count +
                        " powers discovered";
                default:
                    return journal.CreatureRaces.Count + " species  •  " +
                        journal.MonsterTypes.Count + " / " + EssenceMonsterArchetypes.All.Count +
                        " magical monsters  •  " + journal.AstralRanks.Count + " / 5 astral ranks";
            }
        }

        private List<CodexRow> Entries(GameComponent_EssenceCodex journal)
        {
            var items = new List<CodexRow>();
            int total = 0;
            int known = 0;

            if (section == Section.Essences || section == Section.Confluences)
            {
                bool confluence = section == Section.Confluences;
                var defs = DefDatabase<EssenceDef>.AllDefsListForReading
                    .Where(e => e != null && e.isConfluence == confluence).ToList();
                var discovered = confluence ? journal.ConfluenceEssences : journal.NormalEssences;
                total = defs.Count;
                foreach (var def in defs)
                {
                    if (!discovered.Contains(def.defName)) continue;
                    known++;
                    AddFiltered(items, (confluence ? "con:" : "ess:") + def.defName,
                        def.label.CapitalizeFirst());
                }

                if (confluence)
                {
                    foreach (var recipe in journal.Recipes)
                        AddFiltered(items, "recipe:" + recipe.Key,
                            "Recipe: " + EssenceName(recipe.first) + " + " +
                            EssenceName(recipe.second) + " + " + EssenceName(recipe.third));
                }
            }
            else if (section == Section.Stones)
            {
                var defs = DefDatabase<AwakeningStoneDef>.AllDefsListForReading;
                total = defs.Count;
                foreach (var def in defs)
                {
                    if (!journal.AwakeningStones.Contains(def.defName)) continue;
                    known++;
                    AddFiltered(items, "stone:" + def.defName,
                        def.label.CapitalizeFirst());
                }
            }
            else if (section == Section.Abilities)
            {
                var defs = DefDatabase<EssencePowerDef>.AllDefsListForReading;
                total = defs.Count;
                foreach (var def in defs)
                {
                    if (!journal.LearnedPowers.Contains(def.defName)) continue;
                    known++;
                    AddFiltered(items, "power:" + def.defName,
                        def.label.CapitalizeFirst());
                }
            }
            else
            {
                var defs = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(d => d?.race != null && !d.race.Humanlike).ToList();
                total = defs.Count + 5 + EssenceMonsterArchetypes.All.Count;
                foreach (var def in defs)
                {
                    if (!journal.CreatureRaces.Contains(def.defName)) continue;
                    known++;
                    AddFiltered(items, "creature:" + def.defName,
                        def.label.CapitalizeFirst());
                }
                foreach (var archetype in EssenceMonsterArchetypes.All)
                {
                    if (!journal.MonsterTypes.Contains(archetype.id)) continue;
                    known++;
                    AddFiltered(items, "monster:" + archetype.id,
                        archetype.name + " [" + EssenceProgression.RankLabel(archetype.rank) + "]");
                }
                for (int i = 0; i <= 4; i++)
                {
                    if (!journal.AstralRanks.Contains(i.ToString())) continue;
                    known++;
                    AddFiltered(items, "astral:" + i, "Astral manifestation: " +
                        EssenceProgression.RankLabel(i));
                }
            }

            items = items.OrderBy(e => e.title, StringComparer.OrdinalIgnoreCase).ToList();
            // Do not leak names of unknown Defs through search, lists or details.
            if (string.IsNullOrWhiteSpace(search) && known < total)
                items.Add(new CodexRow { key = "locked",
                    title = "???    (" + (total - known) + " still undiscovered)" });
            return items;
        }

        private void AddFiltered(List<CodexRow> items, string key, string title)
        {
            if (string.IsNullOrWhiteSpace(search) ||
                title.IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
                items.Add(new CodexRow { key = key, title = title });
        }

        private void DrawList(Rect bounds, List<CodexRow> rows)
        {
            Widgets.DrawBoxSolid(bounds, Background);
            Rect scrollRect = new Rect(bounds.x + 5f, bounds.y + 5f,
                bounds.width - 10f, bounds.height - 10f);
            float rowHeight = 40f;
            Rect view = new Rect(0f, 0f, scrollRect.width - 18f,
                Mathf.Max(scrollRect.height, rows.Count * (rowHeight + 4f) + 8f));
            Widgets.BeginScrollView(scrollRect, ref listScroll, view);
            try
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var entry = rows[i];
                    Rect rect = new Rect(3f, i * (rowHeight + 4f) + 4f,
                        view.width - 5f, rowHeight);
                    if (selectedKey == entry.key)
                        Widgets.DrawBoxSolid(rect, Selected);
                    if (Widgets.ButtonText(rect, entry.title))
                    {
                        selectedKey = entry.key;
                        detailScroll = Vector2.zero;
                    }
                }
            }
            finally { Widgets.EndScrollView(); }
        }

        private void DrawDetails(Rect bounds, GameComponent_EssenceCodex journal,
            List<CodexRow> rows)
        {
            Widgets.DrawBoxSolid(bounds, Background);
            if (string.IsNullOrEmpty(selectedKey) ||
                !rows.Any(e => e.key == selectedKey))
            {
                selectedKey = rows.FirstOrDefault(e => e.key != "locked")?.key ??
                    rows.FirstOrDefault()?.key ?? "";
                detailScroll = Vector2.zero;
            }

            string detail = Detail(journal, selectedKey);
            Rect scrollRect = new Rect(bounds.x + 13f, bounds.y + 15f,
                bounds.width - 26f, bounds.height - 30f);
            Text.Font = GameFont.Small;
            float height = Text.CalcHeight(detail, scrollRect.width - 22f);
            Rect view = new Rect(0f, 0f, scrollRect.width - 17f,
                Mathf.Max(scrollRect.height, height + 20f));
            Widgets.BeginScrollView(scrollRect, ref detailScroll, view);
            try
            {
                Widgets.Label(new Rect(2f, 3f, view.width - 8f, height + 6f), detail);
            }
            finally { Widgets.EndScrollView(); }
        }

        private static string EssenceName(string defName)
        {
            return DefDatabase<EssenceDef>.GetNamedSilentFail(defName)
                ?.label.CapitalizeFirst() ?? defName;
        }

        private static string Percent(float chance) =>
            (100f * chance).ToString("0.##") + "%";

        private static string Detail(GameComponent_EssenceCodex journal, string key)
        {
            if (key == "locked")
                return "UNDISCOVERED\n\nExplore the colony, find essence crystals " +
                    "and awakening stones, awaken powers and encounter creatures " +
                    "to reveal their entries. Unknown names and recipes stay hidden.";

            if (key.StartsWith("ess:", StringComparison.Ordinal) ||
                key.StartsWith("con:", StringComparison.Ordinal))
            {
                bool confluence = key.StartsWith("con:", StringComparison.Ordinal);
                string name = key.Substring(4);
                EssenceDef essence = DefDatabase<EssenceDef>.GetNamedSilentFail(name);
                if (essence == null) return "This essence definition is unavailable.";
                string result = essence.label.CapitalizeFirst() + "\n\n" +
                    (essence.description ?? "") + "\n\n" +
                    (confluence ? "TYPE: CONFLUENCE ESSENCE" : "TYPE: NORMAL ESSENCE") +
                    "\n\nAffinity weights (Power / Speed / Spirit / Recovery): " +
                    essence.powerWeight + " / " + essence.speedWeight + " / " +
                    essence.spiritWeight + " / " + essence.recoveryWeight;
                var starter = DefDatabase<EssenceStarterPowerDef>.AllDefsListForReading
                    .FirstOrDefault(d => d.essence == essence);
                if (starter?.power != null)
                    result += "\n\nInnate starter: " + starter.power.label.CapitalizeFirst();

                if (confluence)
                {
                    var known = journal.Recipes.Where(r => r.confluence == name).ToList();
                    result += "\n\nDiscovered recipes that form this confluence: " + known.Count;
                    foreach (var recipe in known)
                        result += "\n• " + EssenceName(recipe.first) + " + " +
                            EssenceName(recipe.second) + " + " + EssenceName(recipe.third) +
                            " (" + Origin(recipe.origin) + ")";
                }
                return result;
            }

            if (key.StartsWith("recipe:", StringComparison.Ordinal))
            {
                var recipe = journal.Recipes.FirstOrDefault(r =>
                    r.Key == key.Substring("recipe:".Length));
                if (recipe == null) return "Recipe not found.";
                return "DISCOVERED CONFLUENCE RECIPE\n\n" +
                    EssenceName(recipe.first) + " + " + EssenceName(recipe.second) +
                    " + " + EssenceName(recipe.third) +
                    "\n\nProduces: " + EssenceName(recipe.confluence) +
                    "\n\nOrigin: " + Origin(recipe.origin) +
                    "\n\nAbsorption order does not matter. Three different normal " +
                    "essences are required; duplicate essences are not permitted.";
            }

            if (key.StartsWith("stone:", StringComparison.Ordinal))
            {
                var stone = DefDatabase<AwakeningStoneDef>
                    .GetNamedSilentFail(key.Substring(6));
                if (stone == null) return "This stone definition is unavailable.";
                return stone.label.CapitalizeFirst() + "\n\n" +
                    (stone.description ?? "") + "\n\nRarity: " +
                    (stone.rarity ?? "Unknown").CapitalizeFirst() +
                    "\n\nPossible awakening aspects: " +
                    string.Join(", ", (stone.possibleAspects ?? new List<AwakeningAspectDef>())
                        .Where(x => x != null).Select(x => x.label.CapitalizeFirst())) +
                    "\n\nFavored essences (unknown ones remain masked): " +
                    string.Join(", ", (stone.favoredEssences ?? new List<EssenceDef>())
                        .Where(x => x != null)
                        .Select(x => journal.NormalEssences.Contains(x.defName) ||
                                     journal.ConfluenceEssences.Contains(x.defName)
                            ? x.label.CapitalizeFirst() : "???")) +
                    "\n\nThis stone can awaken a compatible power in an open ability slot.";
            }

            if (key.StartsWith("power:", StringComparison.Ordinal))
            {
                var power = DefDatabase<EssencePowerDef>
                    .GetNamedSilentFail(key.Substring(6));
                if (power == null) return "This power definition is unavailable.";
                return power.label.CapitalizeFirst() + "\n\n" +
                    (power.description ?? "") +
                    "\n\nCategory: " + power.primaryCategory +
                    "\nTarget: " + power.targetKind +
                    "\nTags: " + string.Join(", ", power.abilityTags ?? new List<string>()) +
                    "\nCooldown: " + (power.cooldownTicks / 60f).ToString("0.#") + " seconds" +
                    "\n\nPrimary affinity: " +
                    (power.primaryEssence != null &&
                     (journal.NormalEssences.Contains(power.primaryEssence.defName) ||
                      journal.ConfluenceEssences.Contains(power.primaryEssence.defName))
                         ? power.primaryEssence.label.CapitalizeFirst() : "???") +
                    "\n\nThis ability was revealed by a colony essence user.";
            }

            if (key.StartsWith("monster:", StringComparison.Ordinal))
            {
                var kind = EssenceMonsterArchetypes.ForId(key.Substring("monster:".Length));
                if (kind == null || !journal.MonsterTypes.Contains(kind.id))
                    return "Magical monster not yet discovered.";

                return kind.name.ToUpperInvariant() + "\\n\\n" +
                    "Rank: " + EssenceProgression.RankLabel(kind.rank) +
                    "\\nSignature power: " + kind.feature +
                    "\\nRange: " + kind.range.ToString("0.#") + " cells" +
                    "\\n\\nEssence crystal chance: " +
                    Percent(EssenceCreatureLoot.EssenceChance(kind.rank, true)) +
                    "\\nAwakening stone chance: " +
                    Percent(EssenceCreatureLoot.StoneChance(kind.rank, true)) +
                    "\\n\\nThematic essence affinities (unknown ones masked): " +
                    string.Join(", ", kind.favoredEssences.Select(n =>
                        journal.NormalEssences.Contains(n) ? EssenceName(n) : "???")) +
                    "\\n\\nOnly astral tear encounters spawn this magical creature.";
            }

            if (key.StartsWith("astral:", StringComparison.Ordinal) &&
                int.TryParse(key.Substring(7), out int tier))
            {
                return "ASTRAL MANIFESTATION: " +
                    EssenceProgression.RankLabel(tier).ToUpperInvariant() +
                    "\n\nCreature released by an astral tear, assigned a rank " +
                    "when the tear opens. This tier overrides its base species rank." +
                    "\n\nEssence crystal chance: " +
                    Percent(EssenceCreatureLoot.EssenceChance(tier, true)) +
                    "\nAwakening stone chance: " +
                    Percent(EssenceCreatureLoot.StoneChance(tier, true)) +
                    "\n\nAstral Loot allows one additional reduced extraction from its remains.";
            }

            if (key.StartsWith("creature:", StringComparison.Ordinal))
            {
                ThingDef creature = DefDatabase<ThingDef>
                    .GetNamedSilentFail(key.Substring(9));
                if (creature == null) return "This creature definition is unavailable.";
                PawnKindDef kind = DefDatabase<PawnKindDef>.AllDefsListForReading
                    .FirstOrDefault(k => k.race == creature);
                int rank = EssenceCreatureRanks.InherentRankFor(creature, kind);
                return creature.label.CapitalizeFirst() + "\n\n" +
                    (creature.description ?? "") +
                    "\n\nTypical inherent rank: " + EssenceCreatureRanks.RankLabel(rank) +
                    "\n\nEssence crystal chance: " +
                    Percent(EssenceCreatureLoot.EssenceChance(rank)) +
                    "\nAwakening stone chance: " +
                    Percent(EssenceCreatureLoot.StoneChance(rank)) +
                    "\n\nStone rarity also depends on rank. Astral manifestations " +
                    "can have a different rank and improved drop odds.";
            }

            return "Select a discovered entry to inspect it.";
        }

        private static string Origin(string origin)
        {
            switch (origin)
            {
                case "resolved": return "Catalogued recipe";
                case "generated": return "Mod-generated synthesis";
                case "conflictresolved": return "Disputed catalog recipe, stable choice";
                default: return string.IsNullOrEmpty(origin)
                    ? "Previously discovered" : origin;
            }
        }
    }
}
