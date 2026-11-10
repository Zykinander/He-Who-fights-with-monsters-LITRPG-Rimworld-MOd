using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    // A recipe is discovered by the COLONY, not bound to a specific pawn.
    public sealed class EssenceCodexRecipe : IExposable
    {
        public string first = "";
        public string second = "";
        public string third = "";
        public string confluence = "";
        public string origin = "";

        public EssenceCodexRecipe() { }

        public EssenceCodexRecipe(IEnumerable<string> components, string result, string source)
        {
            var names = components.OrderBy(n => n, StringComparer.Ordinal).ToList();
            first = names[0];
            second = names[1];
            third = names[2];
            confluence = result;
            origin = source ?? "";
        }

        public string Key => first + "|" + second + "|" + third;

        public void ExposeData()
        {
            Scribe_Values.Look(ref first, "first", "");
            Scribe_Values.Look(ref second, "second", "");
            Scribe_Values.Look(ref third, "third", "");
            Scribe_Values.Look(ref confluence, "confluence", "");
            Scribe_Values.Look(ref origin, "origin", "");
        }
    }

    /// <summary>
    /// Discovery knowledge belongs to the game/save, not an individual map,
    /// essence-user pawn or global mod setting. Multiple colonies and saves
    /// therefore keep independent codex progress.
    /// </summary>
    public sealed class GameComponent_EssenceCodex : GameComponent
    {
        private List<string> normalEssences = new List<string>();
        private List<string> confluenceEssences = new List<string>();
        private List<string> awakeningStones = new List<string>();
        private List<string> learnedPowers = new List<string>();
        private List<string> creatureRaces = new List<string>();
        private List<string> astralRanks = new List<string>();
        private List<EssenceCodexRecipe> recipes = new List<EssenceCodexRecipe>();

        public GameComponent_EssenceCodex(Game game) { }

        public static GameComponent_EssenceCodex Current =>
            Verse.Current.Game?.GetComponent<GameComponent_EssenceCodex>();

        public IReadOnlyList<string> NormalEssences => normalEssences;
        public IReadOnlyList<string> ConfluenceEssences => confluenceEssences;
        public IReadOnlyList<string> AwakeningStones => awakeningStones;
        public IReadOnlyList<string> LearnedPowers => learnedPowers;
        public IReadOnlyList<string> CreatureRaces => creatureRaces;
        public IReadOnlyList<string> AstralRanks => astralRanks;
        public IReadOnlyList<EssenceCodexRecipe> Recipes => recipes;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref normalEssences, "ebCodexNormalEssences", LookMode.Value);
            Scribe_Collections.Look(ref confluenceEssences, "ebCodexConfluences", LookMode.Value);
            Scribe_Collections.Look(ref awakeningStones, "ebCodexAwakeningStones", LookMode.Value);
            Scribe_Collections.Look(ref learnedPowers, "ebCodexPowers", LookMode.Value);
            Scribe_Collections.Look(ref creatureRaces, "ebCodexCreatures", LookMode.Value);
            Scribe_Collections.Look(ref astralRanks, "ebCodexAstralRanks", LookMode.Value);
            Scribe_Collections.Look(ref recipes, "ebCodexRecipes", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                normalEssences = Distinct(normalEssences);
                confluenceEssences = Distinct(confluenceEssences);
                awakeningStones = Distinct(awakeningStones);
                learnedPowers = Distinct(learnedPowers);
                creatureRaces = Distinct(creatureRaces);
                astralRanks = Distinct(astralRanks);
                recipes ??= new List<EssenceCodexRecipe>();
                recipes = recipes.Where(r => r != null &&
                    !string.IsNullOrEmpty(r.first) && !string.IsNullOrEmpty(r.second) &&
                    !string.IsNullOrEmpty(r.third) && !string.IsNullOrEmpty(r.confluence))
                    .GroupBy(r => r.Key, StringComparer.Ordinal)
                    .Select(g => g.First()).ToList();
            }
        }

        private static List<string> Distinct(List<string> values) =>
            (values ?? new List<string>()).Where(v => !string.IsNullOrEmpty(v))
                .Distinct(StringComparer.Ordinal).ToList();

        private static void AddUnique(List<string> list, string name)
        {
            if (!string.IsNullOrEmpty(name) && !list.Contains(name)) list.Add(name);
        }

        public void DiscoverEssence(string defName)
        {
            var essence = DefDatabase<EssenceDef>.GetNamedSilentFail(defName);
            if (essence == null) return;
            AddUnique(essence.isConfluence ? confluenceEssences : normalEssences, defName);
        }

        public void DiscoverStone(string defName)
        {
            if (DefDatabase<AwakeningStoneDef>.GetNamedSilentFail(defName) != null)
                AddUnique(awakeningStones, defName);
        }

        public void DiscoverPower(string defName)
        {
            if (DefDatabase<EssencePowerDef>.GetNamedSilentFail(defName) != null)
                AddUnique(learnedPowers, defName);
        }

        public void DiscoverCreature(Pawn creature)
        {
            if (creature?.RaceProps == null || creature.RaceProps.Humanlike) return;
            AddUnique(creatureRaces, creature.def.defName);

            bool manifested = creature.health?.hediffSet?.hediffs
                ?.OfType<Hediff_AstralManifestation>().Any() == true;
            if (manifested)
                AddUnique(astralRanks, EssenceRankCombat.RankOf(creature).ToString());
        }

        public void DiscoverConfluence(IEnumerable<string> componentNames,
            string confluenceName, string origin)
        {
            if (componentNames == null ||
                DefDatabase<EssenceDef>.GetNamedSilentFail(confluenceName)?.isConfluence != true)
                return;
            var names = componentNames.ToList();
            if (names.Count != 3 || names.Distinct(StringComparer.Ordinal).Count() != 3 ||
                names.Any(name => DefDatabase<EssenceDef>.GetNamedSilentFail(name) == null))
                return;
            foreach (string name in names) DiscoverEssence(name);
            DiscoverEssence(confluenceName);
            var record = new EssenceCodexRecipe(names, confluenceName, origin);
            if (!recipes.Any(r => r.Key == record.Key)) recipes.Add(record);
        }

        // Also retroactively discovers entries already owned by characters in
        // old saves, without unlocking entries from the whole Def database.
        public void ObserveColonist(Pawn colonist)
        {
            if (colonist == null || colonist.Faction != Faction.OfPlayer ||
                colonist.RaceProps?.Humanlike != true) return;

            Hediff_EssenceUser essenceUser = Hediff_EssenceUser.ForPawn(colonist);
            if (essenceUser == null) return;
            foreach (string essence in essenceUser.OwnedEssenceNames)
                DiscoverEssence(essence);
            foreach (var slot in essenceUser.AbilitySlots)
            {
                if (slot == null || !slot.IsAwakened) continue;
                DiscoverPower(slot.abilityDefName);
                DiscoverStone(slot.awakeningStoneDefName);
            }
            if (essenceUser.NormalEssences.Count == 3 &&
                !string.IsNullOrEmpty(essenceUser.ConfluenceName))
                DiscoverConfluence(essenceUser.NormalEssences,
                    essenceUser.ConfluenceName, essenceUser.ConfluenceStatus);
        }

        public override void GameComponentTick()
        {
            // Ten in-game seconds: enough for discoveries without scanning every
            // frame. Current knowledge is saved; this scan never erases it.
            if (Find.TickManager != null && Find.TickManager.TicksGame % 600 == 0)
                RefreshObservedWorld();
        }

        public void RefreshObservedWorld()
        {
            if (Find.Maps == null) return;
            foreach (Map map in Find.Maps)
            {
                if (map == null) continue;

                // An owned colonist on ANY map contributes their knowledge.
                foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
                    ObserveColonist(pawn);

                // Wild creatures, ground crystals and stones are discovered
                // only if visible on a player home map, not all generated
                // animals/items everywhere or behind fog.
                if (!map.IsPlayerHome) continue;
                foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn != null && pawn.Spawned && !pawn.Position.Fogged(map))
                    {
                        if (pawn.RaceProps != null && !pawn.RaceProps.Humanlike)
                            DiscoverCreature(pawn);
                    }
                }

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (thing == null || !thing.Spawned || thing.Position.Fogged(map))
                        continue;

                    if (thing is Corpse corpse)
                    {
                        DiscoverCreature(corpse.InnerPawn);
                        continue;
                    }

                    if (!(thing is ThingWithComps item)) continue;
                    var essenceItem = item.GetComp<CompEssenceItem>();
                    if (essenceItem?.DiscoveryEssence != null)
                        DiscoverEssence(essenceItem.DiscoveryEssence.defName);

                    var stoneItem = item.GetComp<CompAwakeningStoneItem>();
                    if (stoneItem?.DiscoveryStone != null)
                        DiscoverStone(stoneItem.DiscoveryStone.defName);
                }
            }
        }
    }
}
