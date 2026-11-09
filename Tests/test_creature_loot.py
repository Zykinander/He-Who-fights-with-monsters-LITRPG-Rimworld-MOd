"""Check ranked loot data, independent rolls and XML stone/item coverage."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / "Source/EssenceCreatureLoot.cs").read_text()
extract = (ROOT / "Source/Hediff_EssenceUser.cs").read_text()
workflow = (ROOT / ".github/workflows/build-essencebound.yml").read_text()

def read_float_array(name):
    match = re.search(rf"{name}\s*=\s*\{{([^}}]+)\}}", source)
    assert match, name
    return [float(n) for n in re.findall(r"(\d+(?:\.\d+)?)f", match.group(1))]

essences = read_float_array("EssenceChances")
stones = read_float_array("StoneChances")
assert len(essences) == len(stones) == 6
assert all(0 < a < b < 1 for a, b in zip(essences, essences[1:]))
assert all(0 < a < b < 1 for a, b in zip(stones, stones[1:]))
assert essences[0] < 0.05 and stones[0] < 0.05, "Unranked loot should be extremely rare."
assert essences[-1] < 1 and stones[-1] < 1, "No guaranteed drop."
assert "Rand.Value < EssenceChance" in source
assert "Rand.Value < StoneChance" in source
assert "RankOf(__instance)" in source
assert "nameof(Pawn.Kill)" in source
assert "out bool __state" in source and "if (!__state" in source
assert "corpse.Spawned" in source
assert "chanceMultiplier = 1f" in source
assert "RollDrops(rank, true" in extract
assert "0.60f" in extract
assert "corpse.Destroy(DestroyMode.Vanish)" in extract
assert "test_creature_loot.py" in workflow

rarities = ("common", "uncommon", "rare", "epic", "legendary")
stone_defs = list(ET.parse(ROOT / "1.7/Defs/AwakeningDefs/AwakeningStones.xml").getroot())
thing_defs = {node.findtext("defName")
              for node in ET.parse(ROOT / "1.7/Defs/ThingDefs/AwakeningStones.xml").getroot()}
stone_rarities = {r: 0 for r in rarities}
for node in stone_defs:
    rarity = node.findtext("rarity")
    name = node.findtext("defName")
    assert rarity in rarities, (name, rarity)
    assert name.startswith("EB_Stone_")
    item_name = "EB_AwakeningStone_" + name[len("EB_Stone_"):]
    assert item_name in thing_defs, item_name
    stone_rarities[rarity] += 1
assert len(stone_defs) == 30 and all(stone_rarities.values())

block = source.split("StoneRarityWeights =", 1)[1].split("};", 1)[0]
rows = [list(map(int, re.findall(r"\d+", row)))
        for row in re.findall(r"new\[\]\s*\{([^}]+)\}", block)]
assert len(rows) == 6, rows
assert all(len(row) == 5 and sum(row) == 100 for row in rows)
assert rows[0] == [100, 0, 0, 0, 0]
assert rows[1][-1] == 0
assert rows[-1][-1] > rows[-2][-1] > 0

print("Creature loot chance, stone rarity, and interaction checks passed.")
