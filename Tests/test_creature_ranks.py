"""Static regression checks for species-based ranks (runtime combat needs in-game QA)."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / "Source/EssenceCreatureRanks.cs").read_text()
combat = (ROOT / "Source/EssenceRankCombat.cs").read_text()

entries = re.findall(r'\{\s*"([A-Za-z][A-Za-z0-9]*)",\s*(-?\d+)\s*\}', source)
rank_map = dict(entries)
assert len(rank_map) == len(entries), "Duplicate species rank mapping"
assert all(-1 <= int(rank) <= 4 for rank in rank_map.values())
assert rank_map["Squirrel"] == "-1"
assert rank_map["Thrumbo"] == "4"
assert rank_map["Megascarab"] == "0"
assert rank_map["Warg"] == "1"
assert rank_map["Megaspider"] == "2"
assert rank_map["Elephant"] == "3"
assert "race.race.Humanlike" in source
assert "EssenceCreatureRankExtension" in source
assert "kind?.combatPower" in source
assert "InherentRankOf(pawn)" in combat

# A manifested animal's explicitly assigned astral tier takes priority.
manifest = combat.index("if (manifestation != null)")
inherent = combat.index("return EssenceCreatureRanks.InherentRankOf(pawn)")
assert manifest < inherent
# Leave normal humanlike essence rank progression intact.
assert "essenceUser.OwnedEssenceNames" in combat
assert "essenceUser.GetEssenceRankIndex" in combat

print(f"Creature rank mapping checks passed ({len(entries)} overrides).")
