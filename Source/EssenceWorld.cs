using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Magic density follows live colony strength; only the tear event schedule
    // is saved per map. Already opened tears retain their opening snapshot.
    public sealed class MapComponent_EssenceWorld : MapComponent
    {
        private int nextManifestationCheck = -1;
        public MapComponent_EssenceWorld(Map map) : base(map) { }

        public EssenceDensityReport DensityReport => EssenceDensityCalculator.ForMap(map);
        public float MagicDensity => DensityReport.FinalDensity;

        public override void ExposeData()
        {
            base.ExposeData();
            // v0.13 saves may contain ebAmbientDensity. That obsolete value
            // is ignored: density is now determined by the current colony.
            Scribe_Values.Look(ref nextManifestationCheck, "ebNextManifestationCheck", -1);
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager == null) return;
            int now = Find.TickManager.TicksGame;
            if (nextManifestationCheck < 0)
                nextManifestationCheck = now + 60000;
            if (now < nextManifestationCheck) return;
            nextManifestationCheck = now + 60000;
            if (!EssenceboundMod.ManifestationsEnabled || EssenceboundMod.ManifestationFrequency == 0 ||
                map.mapPawns.FreeColonistsSpawned.Count == 0 || !map.IsPlayerHome) return;
            // One attempt per day; low-density maps are considerably quieter.
            float chance = Math.Min(0.8f,
                0.10f * EssenceboundMod.ManifestationFrequency * MagicDensity);
            if (Rand.Value < chance) TryOpenTear();
        }

        public bool TryOpenTear()
        {
            if (map.mapPawns.FreeColonistsSpawned.Count == 0) return false;
            var def = DefDatabase<ThingDef>.GetNamedSilentFail("EB_AstralTear");
            if (def == null || map.listerThings.ThingsOfDef(def).Count > 0) return false;
            if (!TryFindOpenCell(out IntVec3 cell)) return false;
            var tear = ThingMaker.MakeThing(def) as Thing_AstralTear;
            if (tear == null) return false;
            float densityAtOpening = MagicDensity;
            tear.Initialize(densityAtOpening);
            GenSpawn.Spawn(tear, cell, map);
            Messages.Message("An astral tear is opening! Colony-driven magic density: " +
                (densityAtOpening * 100f).ToString("0") + "%.",
                MessageTypeDefOf.ThreatSmall, false);
            return true;
        }

        private bool TryFindOpenCell(out IntVec3 result)
        {
            result = IntVec3.Invalid;
            var colonists = map.mapPawns.FreeColonistsSpawned;
            if (colonists.Count == 0) return false;
            Pawn focus = colonists[Rand.Range(0, colonists.Count)];
            for (int i = 0; i < 160; i++)
            {
                IntVec3 c = new IntVec3(focus.Position.x + Rand.Range(-24, 25), 0,
                    focus.Position.z + Rand.Range(-24, 25));
                if (!c.InBounds(map) || !c.Standable(map) || c.Fogged(map) ||
                    c.Roofed(map) || c.GetFirstBuilding(map) != null ||
                    c.DistanceTo(focus.Position) < 9f) continue;
                result = c;
                return true;
            }
            return false;
        }

        public void ResolveTear(Thing_AstralTear tear)
        {
            if (tear == null || !tear.Spawned) return;
            float intensity = tear.OpeningDensity;
            bool summon = EssenceboundMod.ManifestationMonstersEnabled &&
                          Rand.Value < 0.15f + 0.65f * intensity;
            if (summon && SpawnMonsterGroup(tear.Position, intensity)) return;
            if (SpawnEssence(tear.Position, map, out string label))
                Messages.Message("An astral manifestation condensed into " + label + " essence.",
                    MessageTypeDefOf.PositiveEvent, false);
        }

        // Developer-only shortcut: test any monster tier beside the selected
        // colonist without changing the colony's actual magic density.
        public bool TrySpawnDevMonster(Pawn observer, int rank)
        {
            if (!Prefs.DevMode || observer == null || observer.Map != map ||
                !observer.Spawned) return false;
            for (int i = 0; i < 120; i++)
            {
                IntVec3 cell = new IntVec3(observer.Position.x + Rand.Range(-10, 11), 0,
                    observer.Position.z + Rand.Range(-10, 11));
                float distance = cell.DistanceTo(observer.Position);
                if (distance < 5f || distance > 10f ||
                    !cell.InBounds(map) || !cell.Standable(map) ||
                    cell.Fogged(map) || cell.GetFirstBuilding(map) != null)
                    continue;
                return SpawnMonsterGroup(cell, MagicDensity, Math.Max(0, Math.Min(4, rank)));
            }
            return false;
        }

        private bool SpawnMonsterGroup(IntVec3 center, float intensity, int forcedRank = -1)
        {
            int monsterRank = forcedRank >= 0 ? Math.Max(0, Math.Min(4, forcedRank)) :
                EssenceMonsterRanks.Roll(intensity);
            var archetype = EssenceMonsterArchetypes.ForRank(monsterRank);
            PawnKindDef customKind = archetype == null ? null :
                DefDatabase<PawnKindDef>.GetNamedSilentFail(archetype.kindDef);
            string preferred = EssenceMonsterRanks.PlaceholderPawnKind(monsterRank);
            PawnKindDef fallbackKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(preferred);
            fallbackKind ??= DefDatabase<PawnKindDef>.GetNamedSilentFail("Warg");
            fallbackKind ??= DefDatabase<PawnKindDef>.GetNamedSilentFail("Megascarab");
            PawnKindDef kind = customKind ?? fallbackKind;
            if (kind == null)
            {
                Log.Warning("[Essencebound] No vanilla pawn kind for an astral manifestation.");
                return false;
            }

            HediffDef markerDef = DefDatabase<HediffDef>.GetNamedSilentFail("EB_AstralManifestation");
            int count = EssenceMonsterRanks.GroupSize(monsterRank);
            int spawned = 0;
            for (int n = 0; n < count; n++)
            {
                IntVec3 cell = center;
                if (n > 0 && !TryFindNear(center, map, out cell)) break;
                Pawn monster = null;
                bool customSpawned = kind == customKind && customKind != null;
                try
                {
                    monster = PawnGenerator.GeneratePawn(kind);
                }
                catch (Exception exception)
                {
                    Log.Warning("[Essencebound] Could not generate " + kind.defName +
                        "; falling back to a vanilla animal. " + exception.Message);
                    if (fallbackKind != null && kind != fallbackKind)
                    {
                        customSpawned = false;
                        try { monster = PawnGenerator.GeneratePawn(fallbackKind); }
                        catch (Exception fallbackError)
                        {
                            Log.Error("[Essencebound] Monster fallback generation failed: " +
                                fallbackError.Message);
                        }
                    }
                }
                if (monster == null) continue;
                if (markerDef != null)
                {
                    var marker = HediffMaker.MakeHediff(markerDef, monster) as Hediff_AstralManifestation;
                    if (marker != null)
                    {
                        marker.magicDensity = intensity;
                        marker.rankIndex = monsterRank;
                        marker.Severity = monsterRank + 1f;
                        if (customSpawned && archetype != null)
                            marker.AssignArchetype(archetype.id);
                        monster.health.AddHediff(marker);
                    }
                }
                GenSpawn.Spawn(monster, cell, map);
                if (monster.mindState != null)
                    monster.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                spawned++;
            }
            if (spawned > 0)
                Messages.Message("The astral tear unleashed " + spawned + " " +
                    EssenceMonsterRanks.RankLabel(monsterRank) + "-rank " +
                    (customKind != null ? archetype.name : "magical monster") +
                    (spawned == 1 ? "!" : " creatures!") +
                    " Lower-ranked fighters deal reduced damage.",
                    MessageTypeDefOf.ThreatBig, false);
            return spawned > 0;
        }

        public static bool TryFindNear(IntVec3 center, Map map, out IntVec3 result)
        {
            result = IntVec3.Invalid;
            for (int i = 0; i < 60; i++)
            {
                IntVec3 c = new IntVec3(center.x + Rand.Range(-3, 4), 0,
                    center.z + Rand.Range(-3, 4));
                if (c.InBounds(map) && c.Standable(map) && c.GetFirstBuilding(map) == null)
                { result = c; return true; }
            }
            return false;
        }

        public static bool SpawnEssence(IntVec3 center, Map map, out string label,
            System.Collections.Generic.IEnumerable<string> preferredEssenceNames = null)
        {
            label = "";
            if (map == null) return false;
            var pool = DefDatabase<EssenceDef>.AllDefsListForReading
                .Where(e => e != null && !e.isConfluence).ToList();
            if (pool.Count == 0) return false;
            // On a successful monster loot roll, a thematic essence is
            // favored, but never guaranteed. Drop *rates* are unchanged.
            var favorites = preferredEssenceNames == null ? null :
                new System.Collections.Generic.HashSet<string>(
                    preferredEssenceNames, StringComparer.Ordinal);
            var themed = favorites == null ? null :
                pool.Where(e => favorites.Contains(e.defName)).ToList();
            var selection = themed != null && themed.Count > 0 && Rand.Value < 0.45f
                ? themed : pool;
            EssenceDef essence = selection[Rand.Range(0, selection.Count)];
            string suffix = essence.defName.StartsWith("EB_")
                ? essence.defName.Substring(3) : essence.defName;
            ThingDef crystal = DefDatabase<ThingDef>.GetNamedSilentFail("EB_" + suffix + "_Crystal");
            if (crystal == null) return false;
            // The corpse's own tile is a valid fallback when surrounded by
            // structures: do not discard the loot after a successful cast.
            if (!TryFindNear(center, map, out IntVec3 cell))
            {
                if (!center.InBounds(map) || !center.Standable(map)) return false;
                cell = center;
            }
            GenSpawn.Spawn(ThingMaker.MakeThing(crystal), cell, map);
            label = essence.label;
            return true;
        }
    }
}
