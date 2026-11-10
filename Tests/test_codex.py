"""Structural regression checks for the colony-wide, save-backed Codex."""
from pathlib import Path
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
state = (ROOT / "Source/GameComponent_EssenceCodex.cs").read_text()
ui = (ROOT / "Source/Dialog_EssenceCodex.cs").read_text()
pawn = (ROOT / "Source/Hediff_EssenceUser.cs").read_text()
profile = (ROOT / "Source/Dialog_EssenceProfile.cs").read_text()
settings = (ROOT / "Source/EssenceboundMod.cs").read_text()
essence = (ROOT / "Source/CompEssenceItem.cs").read_text()
stone = (ROOT / "Source/CompAwakeningStoneItem.cs").read_text()
workflow = (ROOT / ".github/workflows/build-essencebound.yml").read_text()

def count(path, tag):
    return sum(e.tag == tag for e in ET.parse(ROOT / path).getroot())

assert count("1.7/Defs/EssenceDefs/NormalEssences.xml", "Essencebound.EssenceDef") == 125
assert count("1.7/Defs/EssenceDefs/ConfluenceEssences.xml", "Essencebound.EssenceDef") == 101
assert count("1.7/Defs/AwakeningDefs/AwakeningStones.xml", "Essencebound.AwakeningStoneDef") == 30
assert count("1.7/Defs/PowerDefs/EssencePowers.xml", "Essencebound.EssencePowerDef") == 100

assert "public sealed class GameComponent_EssenceCodex : GameComponent" in state
assert "Verse.Current.Game?.GetComponent<GameComponent_EssenceCodex>()" in state
assert "public sealed class EssenceCodexRecipe : IExposable" in state
assert 'components.OrderBy(n => n, StringComparer.Ordinal)' in state
assert 'public string Key => first + "|" + second + "|" + third;' in state
assert "LookMode.Deep" in state and "ebCodexRecipes" in state
for name in [
    "ebCodexNormalEssences", "ebCodexConfluences", "ebCodexAwakeningStones",
    "ebCodexPowers", "ebCodexCreatures", "ebCodexAstralRanks",
]:
    assert name in state
assert "LoadSaveMode.PostLoadInit" in state
assert "Distinct(StringComparer.Ordinal)" in state

assert "DiscoverEssence" in state
assert "DiscoverStone" in state
assert "DiscoverPower" in state
assert "DiscoverCreature" in state
assert "DiscoverConfluence" in state
assert "ObserveColonist" in state
assert "RefreshObservedWorld" in state
assert "GameComponentTick" in state
assert "map.IsPlayerHome" in state
assert "thing.Position.Fogged(map)" in state
assert "pawn.Position.Fogged(map)" in state
assert "map.listerThings.AllThings" in state
assert "map.mapPawns.FreeColonistsSpawned" in state
assert "if (!recipes.Any(r => r.Key == record.Key))" in state
assert "state.OwnedEssenceNames" not in state  # Explicit variable named essenceUser
assert "essenceUser.NormalEssences.Count == 3" in state
assert "essenceUser.AbilitySlots" in state

assert "DiscoveryEssence => Essence" in essence
assert "DiscoveryStone => Stone" in stone
assert pawn.count("GameComponent_EssenceCodex.Current?.ObserveColonist(pawn)") >= 2
assert "defaultLabel = \"Essence Codex\"" in pawn
assert "new Dialog_EssenceCodex()" in pawn
assert "new Dialog_EssenceCodex()" in profile
assert "new Dialog_EssenceCodex()" in settings

assert "public sealed class Dialog_EssenceCodex : Window" in ui
for tab in ["Essences", "Confluences", "Stones", "Abilities", "Bestiary"]:
    assert tab in ui
assert "Widgets.TextField" in ui
assert "Widgets.BeginScrollView" in ui and "Widgets.EndScrollView" in ui
assert 'title.IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase)' in ui
assert "if (!discovered.Contains(def.defName)) continue;" in ui
assert "if (!journal.AwakeningStones.Contains(def.defName)) continue;" in ui
assert "if (!journal.LearnedPowers.Contains(def.defName)) continue;" in ui
assert "if (!journal.CreatureRaces.Contains(def.defName)) continue;" in ui
assert "if (!journal.AstralRanks.Contains(i.ToString())) continue;" in ui
assert 'key = "locked"' in ui
assert 'title = "???' in ui
assert "journal.Recipes" in ui
assert "case \"generated\"" in ui and "Mod-generated synthesis" in ui
assert "EssenceCreatureLoot.EssenceChance" in ui
assert "EssenceCreatureLoot.StoneChance" in ui
assert "test_codex.py" in workflow

print("PASS: save-scoped Codex, five discovery-only tabs, no spoilers, "
      "old-save backfill, acquisition integration and source wiring.")
