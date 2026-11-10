"""Source-level regressions for telegraphed and differentiated astral combat.

These assertions do not replace in-game timing, effects or balance tests.
"""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
combat = (ROOT / "Source/EssenceMonsterArchetypes.cs").read_text()
hediff = (ROOT / "Source/Hediff_AstralManifestation.cs").read_text()
codex = (ROOT / "Source/Dialog_EssenceCodex.cs").read_text()
loot = (ROOT / "Source/EssenceCreatureLoot.cs").read_text()
ranks = (ROOT / "Source/EssenceRankCombat.cs").read_text()
workflow = (ROOT / ".github/workflows/build-essencebound.yml").read_text()

def archetype(name):
    start = combat.index('id = "' + name + '"')
    end = combat.find('favoredEssences = new[]', start)
    assert end > start
    return combat[start:end]

expected = {
    "glassmote": (0, 75, 2.0, "EB_Debuff_Static"),
    "emberfang": (1, 65, 1.6, "Flame"),
    "duskweaver": (2, 110, 2.3, "EB_Debuff_Webbed"),
    "stormhide": (3, 105, 4.0, "Blunt"),
    "sovereign": (4, 135, 2.0, "EB_Buff_GuardianWard"),
}
assert len(expected) == 5
for name, (rank, warning, radius, effect) in expected.items():
    piece = archetype(name)
    assert "rank = " + str(rank) in piece
    assert "windupTicks = " + str(warning) in piece
    assert re.search(r"radius = " + str(radius).rstrip("0").rstrip(".") + r"f", piece)
    if name != "sovereign":
        assert effect in piece
    assert "combatStyle =" in piece
    assert "feature =" in piece

assert "TryChoosePulseCenter(Hediff_AstralManifestation marker" in combat
assert "caster.Map.mapPawns.FreeColonistsSpawned" in combat
assert "GenSight.LineOfSight" in combat
assert 'case "emberfang":' in combat
assert 'case "duskweaver":' in combat
assert 'case "sovereign":' in combat
assert "OrderByDescending(p =>" in combat
assert "OfType<Hediff_Injury>()" in combat
assert 'type.id == "stormhide" ? caster.Position : target.Position' in combat
assert 'type.id == "sovereign" && options.Count == 0' in combat
assert "HasInjuredAstralAlly" in combat
assert "RenewNearbyAstralCreatures" in combat
assert "WardNearbyAstralCreatures" in combat
assert '"EB_Buff_GuardianWard"' in combat
assert "target.TakeDamage(new DamageInfo(damage, type.impact," in combat
assert "0f, -1f, caster" in combat  # rank resistance still sees the attacker
assert "status?.Refresh(duration, null, null)" in combat  # no colonist XP farming
assert "p.Position.DistanceTo(impactCell) <= type.radius" in combat
assert "FleckMaker.ThrowDustPuffThick" in combat
assert "new Color(" in combat and "DrawSpellEffect" in combat
assert "int count = impact ? 10 : 5" in combat

# Charging and attack execution are two distinct actions, so a target can dodge
# the fixed warned cell and neither save/load nor being downed double-casts.
assert "Scribe_Values.Look(ref chargeEndTick, \"ebMonsterChargeEndTick\"" in hediff
assert "Scribe_Values.Look(ref chargedCell, \"ebMonsterChargedCell\"" in hediff
assert "Scribe_Values.Look(ref nextPulseTick, \"ebMonsterNextPulse\"" in hediff
assert "chargeEndTick = now + type.windupTicks" in hediff
assert "if (now < chargeEndTick)" in hediff
assert "EssenceMonsterArchetypes.TriggerPulse(this)" in hediff
assert hediff.index("if (now < chargeEndTick)") < hediff.index("EssenceMonsterArchetypes.TriggerPulse(this)")
assert "chargedCell = center" in hediff
assert "chargeEndTick = 0;" in hediff
assert "if (pawn.Downed)" in hediff
assert "chargeEndTick <= Find.TickManager.TicksGame" in hediff
assert "if (type == null) return;" in hediff
assert "if (now < nextPulseTick) return;" in hediff

for label in ["Combat tactic:", "Impact radius:", "Warning period:", "Spell cooldown:"]:
    assert label in codex, label
assert "kind.combatStyle" in codex

status_defs = ET.parse(ROOT / "1.7/Defs/HediffDefs/EssencePowerEffects.xml").getroot()
statuses = {x.findtext("defName") for x in status_defs}
assert {"EB_Debuff_Webbed", "EB_Debuff_Static", "EB_Buff_GuardianWard"} <= statuses

# Monster combat is not a reason to loosen the unusual essence drop rates or
# rank-specific defensive damage reductions.
assert "{ 0.001f, 0.01f, 0.02f, 0.04f, 0.07f, 0.12f }" in loot
assert "{ 0.01f, 0.08f, 0.16f, 0.26f, 0.40f, 0.60f }" in loot
assert "case 4: return 0.01f;" in ranks
assert "case 3: return 0.20f;" in ranks
assert "python Tests/test_magical_monster_combat.py" in workflow

print("PASS: five distinct target policies, fixed telegraph wind-ups, "
      "saved cast state, color cues, ally-only wards and unchanged ranks/loot.")
