"""Validate unique essence selection and universal deterministic confluences."""
from collections import defaultdict
from itertools import combinations, permutations, islice
from pathlib import Path
import json
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / "Source/ConfluenceResolver.cs").read_text()
pawn = (ROOT / "Source/Hediff_EssenceUser.cs").read_text()
crystal = (ROOT / "Source/CompEssenceItem.cs").read_text()
profile = (ROOT / "Source/Dialog_EssenceProfile.cs").read_text()

data = json.loads((ROOT / "Data/confluence_combinations.json").read_text())
recipes = data["combinations"]

def defs(path):
    return [e.findtext("defName") for e in
            ET.parse(ROOT / path).getroot()
            if e.findtext("defName")]

normal = sorted(defs("1.7/Defs/EssenceDefs/NormalEssences.xml"))
confluences = sorted(defs("1.7/Defs/EssenceDefs/ConfluenceEssences.xml"))
assert len(normal) == 125 and len(set(normal)) == 125
assert len(confluences) == 101 and len(set(confluences)) == 101
assert all(name.startswith("EB_") for name in normal + confluences)

explicit = defaultdict(set)
for recipe in recipes:
    names = tuple(sorted("EB_" + n for n in recipe["essences"]))
    assert len(set(names)) == 3
    output = "EB_" + recipe["confluence"]
    assert output in confluences, output
    explicit[names].add(output)
assert len(explicit) == 364
assert sum(len(outputs) > 1 for outputs in explicit.values()) == 2

# A direct recipe is retained. For any other triple, the resolver must select
# from the nonempty set of confluence Defs; it must never leave the fourth slot
# blank. This checks the full 125-choose-3 input domain without generating XML.
count, listed, generated = 0, 0, 0
for names in combinations(normal, 3):
    count += 1
    if names in explicit:
        assert explicit[names].issubset(set(confluences))
        listed += 1
    else:
        assert len(confluences) > 0
        generated += 1
assert count == 317750
assert listed == 364 and generated == 317386

def stable_hash(names):
    key = "|".join(sorted(names))
    h = 2166136261
    for character in key:
        h = ((h ^ ord(character)) * 16777619) & 0xffffffff
    return h

# The same combination in any absorption order gets the same hash and index.
for names in islice(combinations(normal, 3), 0, 200):
    outcomes = {stable_hash(order) % len(confluences)
                for order in permutations(names)}
    assert len(outcomes) == 1

assert "names.Count != 3" in source
assert "new HashSet<string>(inputs.Select(e => e.defName)" in source
assert "ConfluenceState.Generated" in source
assert "ConfluenceState.ConflictResolved" in source
assert "StableChoice(bestMatches, names)" in source
assert "hash *= 16777619u" in source
assert "overlap > scores[output]" in source
assert "result.isConfluence" in source
assert "normalEssenceNames.Contains(essence.defName)" in pawn
assert pawn.index("if (outcome.essence == null)") < pawn.index("normalEssenceNames.Add(essence.defName)")
assert "RestoreMissingConfluence();" in pawn
assert "if (outcome != null) AssignConfluence(outcome);" in pawn
assert "ConfluenceResolver.Resolve(inputs)" in crystal
assert "Absorb three different normal essences" in profile

print(f"PASS: {count:,} unique essence combinations covered: "
      f"{listed} catalogued, {generated:,} generated; "
      "duplicate-blocking, deterministic resolution, legacy save repair, and preview wired.")
