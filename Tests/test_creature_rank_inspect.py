"""Selected animals must show their actual combat rank in the inspect pane."""
from pathlib import Path

root = Path(__file__).resolve().parents[1]
patch = (root / "Source/Patch_EssenceCreatureRankInspectString.cs").read_text()
rank = (root / "Source/EssenceRankCombat.cs").read_text()
creatures = (root / "Source/EssenceCreatureRanks.cs").read_text()
settings = (root / "Source/EssenceboundMod.cs").read_text()

assert "[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]" in patch
assert "private static void Postfix(Pawn __instance, ref string __result)" in patch
assert "EssenceRankCombat.RankOf(__instance)" in patch
assert "EssenceCreatureRanks.RankLabel(rank)" in patch
assert '__instance.RaceProps.Humanlike || __instance.Dead' in patch
assert "OfType<Hediff_AstralManifestation>()" in patch
assert '"Astral rank: "' in patch
assert '"Creature rank: "' in patch
assert "__result.TrimEnd(" in patch and "+ rankLine" in patch
assert 'return EssenceCreatureRanks.InherentRankOf(pawn)' in rank
assert 'if (manifestation != null)' in rank
assert '{ "Squirrel", -1 }' in creatures
assert '{ "Thrumbo", 4 }' in creatures
assert "PatchAll(typeof(Patch_EssenceRankIncomingDamage).Assembly)" in settings

print("PASS: inspect pane displays actual creature/astral ranks via shared resolver.")
