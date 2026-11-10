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

health = (root / "Source/Hediff_CreatureRank.cs").read_text()
xml = (root / "1.7/Defs/HediffDefs/CreatureRanks.xml").read_text()
assert "[HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]" in health
assert "private static void Postfix(Pawn __instance)" in health
assert "__instance.RaceProps.Humanlike || __instance.Dead" in health
assert "GetFirstHediffOfDef(rankDef) != null" in health
assert '__instance.health.AddHediff(rankDef)' in health
assert 'public override string LabelBase' in health
assert "EssenceRankCombat.RankOf(pawn)" in health
assert "EssenceCreatureRanks.RankLabel(tier)" in health
assert '"Astral rank: "' in health and '"Creature rank: "' in health
assert "<defName>EB_CreatureRank</defName>" in xml
assert "<hediffClass>Essencebound.Hediff_CreatureRank</hediffClass>" in xml
assert "<isBad>false</isBad>" in xml
assert "<initialSeverity>1</initialSeverity>" in xml
assert "<maxSeverity>1</maxSeverity>" in xml
assert "new Harmony(" in settings and ".PatchAll(" in settings

print("PASS: inspect pane and Health tab both display actual creature/astral ranks via shared resolver.")

