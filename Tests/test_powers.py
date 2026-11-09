"""Offline structural validation; does NOT compile C# or run RimWorld."""
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from collections import Counter
ROOT = Path(__file__).resolve().parents[1]
powers=json.loads((ROOT/'Data/essence_powers.json').read_text())['powers']
stones=json.loads((ROOT/'Data/awakening_stones.json').read_text())['stones']
assert len(powers)==12
assert len({p['defName'] for p in powers})==12
assert len(stones)==30
powerdefs=list(ET.parse(ROOT/'1.7/Defs/PowerDefs/EssencePowers.xml').getroot())
assert len(powerdefs)==12
buffdefs=list(ET.parse(ROOT/'1.7/Defs/HediffDefs/EssencePowerEffects.xml').getroot())
assert len(buffdefs)==8
for node in list((ROOT/'1.7/Defs').rglob('*.xml')):
    ET.parse(node)
stone_defs=list(ET.parse(ROOT/'1.7/Defs/AwakeningDefs/AwakeningStones.xml').getroot())
aspects={a.findtext('defName') for a in ET.parse(ROOT/'1.7/Defs/AwakeningDefs/AwakeningAspects.xml').getroot()}
all_essences={e.findtext('defName') for name in ('NormalEssences','ConfluenceEssences')
 for e in ET.parse(ROOT/f'1.7/Defs/EssenceDefs/{name}.xml').getroot()}
for power in powers:
    assert (ROOT/'1.7/Textures'/ (power['icon']+'.png')).exists()
    assert power['effect'] in {'damage','damageDebuff','heal','buff','cleanse','pulse'}
    assert power['target'] in {'self','pawn'}
    assert power['cooldown_ticks']>0
    assert all('EB_Aspect_'+a in aspects for a in power['compatible_aspects'])
    assert all('EB_'+e in all_essences for e in power['favored_essences'])
    if power['status_hediff']:
        assert power['status_hediff'] in {h.findtext('defName') for h in buffdefs}
    assert power['source_status']=='original_mod_gameplay_design'
active_aspects={a for p in powers for a in p['compatible_aspects']}
assert active_aspects=={a.removeprefix('EB_Aspect_') for a in aspects}, active_aspects
for stone in stone_defs:
    usable_aspects=[node.text.removeprefix('EB_Aspect_') for node in stone.find('possibleAspects')]
    assert len(usable_aspects)==3
    assert all(a in active_aspects for a in usable_aspects)
    accessible_powers={p['defName'] for p in powers if set(p['compatible_aspects'])&set(usable_aspects)}
    assert len(accessible_powers)>=3,(stone.findtext('defName'),accessible_powers)
# Cooldown field and migration paths are directly present in source.
slot_source=(ROOT/'Source/EssenceAbilitySlot.cs').read_text()
hediff_source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 'nextUseTick' in slot_source and 'BackfillLegacyPowers' in hediff_source
assert 'TryCastPower' in hediff_source and 'GetGizmos' in hediff_source
print(f'PASS: {len(powers)} power defs, {len(buffdefs)} timed effect defs, '
      f'{len(stone_defs)} stones and {len(aspects)} aspects; all XML is well-formed.')

# v0.7: deterministic innate starter for each known essence.
import csv
with (ROOT/'Data/essence_starter_abilities.csv').open(encoding='utf-8') as file:
    rows=list(csv.DictReader(file))
assert len(rows)==226
starterdefs=list(ET.parse(ROOT/'1.7/Defs/PowerDefs/EssenceStarters.xml').getroot())
assert len(starterdefs)==226
by_essence={x.findtext('essence'):x.findtext('power') for x in starterdefs}
assert len(by_essence)==226 and set(by_essence)==all_essences
assert set(by_essence.values()) <= {p['defName'] for p in powers}
assert by_essence['EB_Fire']=='EB_Power_EmberBolt'
assert by_essence['EB_Phoenix']=='EB_Power_EmberBolt'
assert next(x for x in powerdefs if x.findtext('defName')=='EB_Power_EmberBolt').findtext('damageDefName')=='Flame'
source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 'GrantInnatePower' in source and 'GrantMissingInnatePowers' in source
assert 'slot.sourceKind = "stone"' in source and 'free.sourceKind = "innate"' in source
assert 'if (free == null) return "";' in source
print('PASS: 226 innate mappings and Ember Bolt fire damage definition')

# v0.09 — seven targeted abilities accept any living pawn, not just allies or enemies.
pawn_powers={p['defName'] for p in powers if p['target']=='pawn'}
self_powers={p['defName'] for p in powers if p['target']=='self'}
assert len(pawn_powers)==7 and len(self_powers)==5
xml_targets={e.findtext('defName'):e.findtext('targetKind') for e in powerdefs}
for p in powers:
    assert xml_targets[p['defName']] == p['target']
    if p['target']=='pawn':
        assert p['range_cells'] > 0
assert {'EB_Power_EmberBolt','EB_Power_VerdantMend','EB_Power_GuardianWard','EB_Power_PurifyingTouch'} <= pawn_powers
assert 'EB_Power_ForcePulse' in self_powers
executor=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
validator=executor.split('public static bool ValidateTarget(',1)[1].split('public static bool Apply(',1)[0]
assert 'power.targetKind == "pawn"' in validator
assert 'target.HostileTo(caster)' not in validator
assert 'target.Faction != caster.Faction' not in validator
assert 'GenSight.LineOfSight(' in validator and 'power.range' in validator
assert 'target != caster' in validator  # allow targetable self-healing without LOS from self to self
ui=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 'canTargetPawns = true' in ui and 'canTargetSelf = true' in ui
assert 'Any living pawn (ally or enemy)' in ui
print('PASS: v0.09 seven cross-faction targeted powers; five self-cast powers unchanged')

# The redesigned profile is read-only; targeting and cooldown behavior still run through
# the existing pawn gizmos, with a dynamic active-slot cap.
src=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 's.index < SlotsPerEssence' in src
assert 'slot.index >= SlotsPerEssence' in src
assert 'if (!EssencePowerExecutor.Apply(pawn, power, target, slot, out message, out float realWork)) return false;' in src
assert 'Find.Targeter.BeginTargeting(targets' in src
assert 'GrantInnatePower' in src
print('PASS: old targeting and innate unlock pathways preserved with active slot limit')

# v0.11 — work credits by actual effect, with meaningful type-specific requirements.
executor=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
timed=(ROOT/'Source/Hediff_EssenceTimed.cs').read_text()
assert 'work = Math.Max(0f, before - injury.Severity)' in executor
assert 'work = inflicted;' in executor
assert 'work = 10f' in executor
assert 'work += dealt' in executor
assert 'float powerScale = 1f + (user?.GetAffinityBonus("Power") ?? 0f)' in executor
assert 'float recoveryScale = 1f + (user?.GetAffinityBonus("Recovery") ?? 0f)' in executor
assert 'float spiritScale = 1f + (user?.GetAffinityBonus("Spirit") ?? 0f)' in executor
assert 'GetAffinityBonus("Speed")' in src
assert 'slot.nextUseTick = now + Math.Max(30' in src
assert 'if (++benefitTicks >= 60)' in timed
assert 'AddAbilityWork(' in timed
assert 'Scribe_References.Look(ref creditCaster, "ebCreditCaster")' in timed
ui=(ROOT/'Source/Dialog_EssenceProfile.cs').read_text()
assert 'state.GetAffinity(essenceName)' in ui
assert 'state.GetEssenceRankDisplay(essenceName)' in ui
assert 'slot.rankWork' in ui and 'NextSublevelWork' in ui
print('PASS: ability-type progression work tracking and rank-scaled bonuses')

# The multiplier applies at the affinity contribution, before the existing
# Power/Speed/Spirit/Recovery effect hooks consume the total.
progression_source=(ROOT/'Source/EssenceProgression.cs').read_text()
rank_source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 'public static int RankBonusMultiplier(int rankIndex)' in progression_source
assert 'EssenceProgression.RankBonusMultiplier(tier, level) *' in rank_source
assert 'EssenceProgression.BonusPerTier(attribute)' in rank_source
assert 'Math.Min(EssenceProgression.MaxAttributeBonus(attribute), totalBonus)' in rank_source
assert 'GetAffinityBonus("Speed")' in rank_source
print('PASS: effect scaling uses exponential rank bonus with safe Speed cap')

# v0.12: credit real damage from any faction and expose the credited work.
executor=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
src=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
profile=(ROOT/'Source/Dialog_EssenceProfile.cs').read_text()
assert 'work = target.HostileTo(caster)' not in executor
assert 'work = inflicted;' in executor
assert 'result.totalDamageDealt' in executor
assert 'woundsAfter - woundsBefore' in executor
assert 'message += "  Progress +" + credited.ToString("0.#")' in src
assert 'AddAbilityWork(slot.essenceDefName, slot.index, realWork)' in src
assert 'IsTierTrainingStalled(slot.abilityRankIndex)' in src
assert 'state.IsTierTrainingStalled(slot.abilityRankIndex)' in profile
assert 'NextSublevelWork(slot.abilityRankIndex)' in profile
assert 'RankWithLevel(slot.abilityRankIndex, slot.abilitySublevel)' in profile
print('PASS: v0.12 faction-independent real-damage work and cast feedback')

# Real credited XP is bounded at a major-tier boundary so displays are truthful.
assert 'float remainingToTier = Math.Max(0f,' in src
assert 'float credited = Math.Min(amount, remainingToTier)' in src
assert 'slot.rankWork += credited;' in src
assert 'slot.rankWork += amount;' not in src
print('PASS: no extra XP is banked beyond a locked major-rank boundary')

# v0.13: only tagged astral monsters can yield an essence through Astral Loot.
player=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
world=(ROOT/'Source/EssenceWorld.cs').read_text()
assert 'BeginAstralLoot' in player
assert 'TryLootAstralRemains(Corpse corpse, out string reason)' in player
assert 'Hediff_AstralManifestation' in player and 'corpse.InnerPawn?' in player
assert 'Scribe_Values.Look(ref nextAstralLootTick' in player
assert 'GenSight.LineOfSight(pawn.Position, corpse.Position, pawn.Map)' in player
assert 'corpse.Destroy(DestroyMode.Vanish)' in player
assert 'MapComponent_EssenceWorld.SpawnEssence' in player
assert 'GetNamedSilentFail("EB_AstralManifestation")' in world
assert 'monster.health.AddHediff(marker)' in world
assert 'if (Prefs.DevMode)' in player and 'TryOpenTear()' in player
print('PASS: v0.13 tagged monster corpse loot is single-use, range checked and saved')

# v0.13 looting patch: targeter fallback and nearby-corpse menu.
player=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
world=(ROOT/'Source/EssenceWorld.cs').read_text()
assert 'private void BeginAstralLootTargeting()' in player
assert 'canTargetLocations = true' in player
assert 'local.Cell.GetThingList(pawn.Map).OfType<Corpse>()' in player
assert 'private static bool HasAstralLootMark(Corpse corpse)' in player
assert 'pawn.Map.listerThings.AllThings.OfType<Corpse>()' in player
assert 'new FloatMenu(options)' in player
assert 'No marked astral monster remains within 10 cells and sight' in player
assert 'NotifyAstralLoot(captured)' in player
assert 'if (!HasAstralLootMark(corpse))' in player
assert 'Move within 10 cells of the remains' in player
assert 'Line of sight to the remains is blocked' in player
assert 'if (!TryFindNear(center, map, out IntVec3 cell))' in world
assert 'cell = center;' in world
assert player.index('if (!HasAstralLootMark(corpse))') < player.index('corpse.Destroy(DestroyMode.Vanish)')
print('PASS: corpse targeting fallback, nearby menu and safe single extraction')
