using System;
using System.Linq;
using RimWorld;
using Verse;

namespace Essencebound
{
    // Per-map density is rolled once, persisted, and scaled by Mod Settings.
    public sealed class MapComponent_EssenceWorld : MapComponent
    {
        private float ambientDensity = -1f;
        private int nextManifestationCheck = -1;
        public MapComponent_EssenceWorld(Map map) : base(map) { }

        public float MagicDensity
        {
            get
            {
                InitializeIfNeeded();
                return Math.Max(0.05f, Math.Min(1f,
                    ambientDensity * EssenceboundMod.MagicDensityMultiplier));
            }
        }

        private void InitializeIfNeeded()
        {
            if (ambientDensity < 0f)
            {
                float baseline = 0.25f + Rand.Value * 0.5f;
                string biome = map.Biome?.defName ?? "";
                if (biome.Contains("Tropical")) baseline += 0.12f;
                if (biome.Contains("IceSheet") || biome.Contains("ExtremeDesert")) baseline -= 0.08f;
                ambientDensity = Math.Max(0.1f, Math.Min(0.9f, baseline));
            }
            if (nextManifestationCheck < 0)
                nextManifestationCheck = Find.TickManager.TicksGame + 60000;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ambientDensity, "ebAmbientDensity", -1f);
            Scribe_Values.Look(ref nextManifestationCheck, "ebNextManifestationCheck", -1);
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager == null) return;
            InitializeIfNeeded();
            int now = Find.TickManager.TicksGame;
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
            tear.Initialize(MagicDensity);
            GenSpawn.Spawn(tear, cell, map);
            Messages.Message("An astral tear is opening! Local magic density: " +
                (MagicDensity * 100f).ToString("0") + "%.",
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

        private bool SpawnMonsterGroup(IntVec3 center, float intensity)
        {
            string preferred = intensity >= 0.78f ? "Thrumbo" :
                intensity >= 0.46f ? "Warg" : "Megascarab";
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(preferred);
            kind ??= DefDatabase<PawnKindDef>.GetNamedSilentFail("Warg");
            kind ??= DefDatabase<PawnKindDef>.GetNamedSilentFail("Megascarab");
            if (kind == null)
            {
                Log.Warning("[Essencebound] No vanilla pawn kind for an astral manifestation.");
                return false;
            }

            HediffDef markerDef = DefDatabase<HediffDef>.GetNamedSilentFail("EB_AstralManifestation");
            int count = intensity >= 0.78f ? 2 : 1;
            int spawned = 0;
            for (int n = 0; n < count; n++)
            {
                IntVec3 cell = center;
                if (n > 0 && !TryFindNear(center, map, out cell)) break;
                Pawn monster = PawnGenerator.GeneratePawn(kind);
                if (monster == null) continue;
                if (markerDef != null)
                {
                    var marker = HediffMaker.MakeHediff(markerDef, monster) as Hediff_AstralManifestation;
                    if (marker != null)
                    {
                        marker.magicDensity = intensity;
                        monster.health.AddHediff(marker);
                    }
                }
                GenSpawn.Spawn(monster, cell, map);
                if (monster.mindState != null)
                    monster.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                spawned++;
            }
            if (spawned > 0)
                Messages.Message("The astral tear unleashed " + spawned +
                    " magical monster(s)! An essence user can loot their remains.",
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

        public static bool SpawnEssence(IntVec3 center, Map map, out string label)
        {
            label = "";
            if (map == null) return false;
            var pool = DefDatabase<EssenceDef>.AllDefsListForReading
                .Where(e => e != null && !e.isConfluence).ToList();
            if (pool.Count == 0) return false;
            EssenceDef essence = pool[Rand.Range(0, pool.Count)];
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
