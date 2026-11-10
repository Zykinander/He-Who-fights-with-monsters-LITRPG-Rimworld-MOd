"""Static integration checks for five new ranked astral monster kinds."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
base = ROOT / "1.7/Defs/PawnKindDefs/AstralMonsterKinds.xml"
defs = ET.parse(base).getroot()
monsters = [x for x in defs if x.tag == "PawnKindDef"]
assert len(monsters) == 5
assert all(x.attrib.get("ParentName") == "AnimalKindBase" for x in monsters)
defnames = [x.findtext("defName") for x in monsters]
assert len(set(defnames)) == 5
assert defnames == [
    "EB_Monster_Glassmote", "EB_Monster_Emberfang",
    "EB_Monster_Duskweaver", "EB_Monster_Stormhide",
    "EB_Monster_PrismaticSovereign",
]
expected_races = ["Megascarab", "Warg", "Megaspider", "Rhinoceros", "Thrumbo"]
for monster, race in zip(monsters, expected_races):
    assert monster.findtext("race") == race
    assert float(monster.findtext("combatPower")) > 0
    assert monster.findtext("ecoSystemWeight") == "0"
    assert monster.findtext("canArriveManhunter") == "false"
    stages = monster.find("lifeStages")
    assert stages is not None and len(stages) == 3
    for stage in stages:
        graphic = stage.find("bodyGraphicData")
        assert graphic is not None
        assert graphic.findtext("texPath").startswith("Things/Pawn/Animal/")
        assert graphic.findtext("color")
        assert float(graphic.findtext("drawSize")) > 0

archetypes = (ROOT / "Source/EssenceMonsterArchetypes.cs").read_text()
marker = (ROOT / "Source/Hediff_AstralManifestation.cs").read_text()
world = (ROOT / "Source/EssenceWorld.cs").read_text()
loot = (ROOT / "Source/EssenceCreatureLoot.cs").read_text()
corpse = (ROOT / "Source/Hediff_EssenceUser.cs").read_text()
codex = (ROOT / "Source/GameComponent_EssenceCodex.cs").read_text()
ui = (ROOT / "Source/Dialog_EssenceCodex.cs").read_text()
workflow = (ROOT / ".github/workflows/build-essencebound.yml").read_text()

for n in defnames:
    assert n in archetypes
assert set(re.findall(r'rank = (\d+), pulseInterval', archetypes)) == {
    "0", "1", "2", "3", "4"
}
assert "TriggerPulse(Hediff_AstralManifestation manifestation)" in archetypes
assert "RenewNearbyAstralCreatures" in archetypes
assert "EB_Debuff_Static" in archetypes
assert "EB_Debuff_Webbed" in archetypes
assert "target.TakeDamage(" in archetypes
assert "GenSight.LineOfSight" in archetypes
assert "map.mapPawns.FreeColonistsSpawned" in archetypes
assert '"EB_Fire"' in archetypes and '"EB_Star"' in archetypes

essence_ids = {
    e.findtext("defName")
    for e in ET.parse(ROOT / "1.7/Defs/EssenceDefs/NormalEssences.xml").getroot()
}
for favored_list in re.findall(r'favoredEssences = new\[\] \{([^}]+)\}', archetypes):
    for name in re.findall(r'"(EB_\w+)"', favored_list):
        assert name in essence_ids

assert "Scribe_Values.Look(ref archetypeId" in marker
assert "Scribe_Values.Look(ref nextPulseTick" in marker
assert "EssenceMonsterArchetypes.TriggerPulse(this)" in marker
assert "AssignArchetype" in marker
assert "marker.AssignArchetype(archetype.id)" in world
assert "EssenceMonsterArchetypes.ForRank(monsterRank)" in world
assert "kind ?? fallbackKind" not in world
assert "fallbackKind" in world

assert "EssenceMonsterArchetypes.FavoredEssences(archetypeId)" in loot
assert "marker?.archetypeId" in loot
assert "monsterType" in corpse
assert "string archetypeId = null" in loot
assert "Rand.Value < 0.45f" in world  # preference after successful roll
assert "{ 0.001f, 0.01f, 0.02f, 0.04f, 0.07f, 0.12f }" in loot
assert "{ 0.01f, 0.08f, 0.16f, 0.26f, 0.40f, 0.60f }" in loot

assert 'ebCodexMonsterTypes' in codex
assert 'AddUnique(monsterTypes, archetype.id)' in codex
assert 'journal.MonsterTypes.Contains(archetype.id)' in ui
assert 'key.StartsWith("monster:"' in ui
assert 'EssenceMonsterArchetypes.ForId(key.Substring(' in ui
assert "test_magical_monsters.py" in workflow

print("PASS: five custom PawnKinds, tiered behavior, saved powers, "
      "thematic loot with unchanged rarity, and Codex discovery.")
