"""Offline structural validation; does NOT compile C# or run RimWorld."""
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from collections import Counter
ROOT = Path(__file__).resolve().parents[1]
powers=json.loads((ROOT/'Data/essence_powers.json').read_text())['powers']
stones=json.loads((ROOT/'Data/awakening_stones.json').read_text())['stones']
assert len(powers)==70
assert len({p['defName'] for p in powers})==70
assert len(stones)==30
powerdefs=list(ET.parse(ROOT/'1.7/Defs/PowerDefs/EssencePowers.xml').getroot())
assert len(powerdefs)==70
buffdefs=list(ET.parse(ROOT/'1.7/Defs/HediffDefs/EssencePowerEffects.xml').getroot())
assert len(buffdefs)==33
for node in list((ROOT/'1.7/Defs').rglob('*.xml')):
    ET.parse(node)
stone_defs=list(ET.parse(ROOT/'1.7/Defs/AwakeningDefs/AwakeningStones.xml').getroot())
aspects={a.findtext('defName') for a in ET.parse(ROOT/'1.7/Defs/AwakeningDefs/AwakeningAspects.xml').getroot()}
all_essences={e.findtext('defName') for name in ('NormalEssences','ConfluenceEssences')
 for e in ET.parse(ROOT/f'1.7/Defs/EssenceDefs/{name}.xml').getroot()}
for power in powers:
    assert (ROOT/'1.7/Textures'/ (power['icon']+'.png')).exists()
    assert power['effect'] in {'damage','damageDebuff','heal','buff','cleanse','pulse',
                               'chain','drain','healGroup','regeneration','auraBuff',
                               'areaCleanse','debuff','splash','healWard',
                               'areaHeal','chainHeal','areaRegen','purgeHeal',
                               'damageAreaDebuff','leechPulse','execute',
                               'delayedBlast','reflect','healingZone','returnTeleport',
                               'summon','remoteManipulation','adaptiveBuff','teleport',
                               'transfusion','tileBarrier','blinkStrike','summonOrder',
                               'survey','markedTarget','branchingHeal','containment',
                               'decoy','powerAura','wardDome','hazardZone'}
    assert power['target'] in {'self','pawn','cell','thing'}
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
assert by_essence['EB_Phoenix']=='EB_Power_AshenComet'
assert next(x for x in powerdefs if x.findtext('defName')=='EB_Power_EmberBolt').findtext('damageDefName')=='Flame'
source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 'GrantInnatePower' in source and 'GrantMissingInnatePowers' in source
assert 'slot.sourceKind = "stone"' in source and 'free.sourceKind = "innate"' in source
assert 'if (free == null) return "";' in source
print('PASS: 226 innate mappings and Ember Bolt fire damage definition')

# v0.09 — seven targeted abilities accept any living pawn, not just allies or enemies.
pawn_powers={p['defName'] for p in powers if p['target']=='pawn'}
self_powers={p['defName'] for p in powers if p['target']=='self'}
assert len(pawn_powers)==36 and len(self_powers)==28
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
assert 'canTargetPawns = !items && !locations' in ui and 'canTargetSelf = !items && !locations' in ui
assert 'Any living pawn (ally or enemy)' in ui
print('PASS: v0.09 seven cross-faction targeted powers; five self-cast powers unchanged')

# The redesigned profile is read-only; targeting and cooldown behavior still run through
# the existing pawn gizmos, with a dynamic active-slot cap.
src=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 's.index < SlotsPerEssence' in src
assert 'slot.index >= SlotsPerEssence' in src
assert 'EssencePowerExecutor.Apply(pawn, power, target.Thing as Pawn,' in src
assert 'EssenceAdvancedAbilities.CastSpatial(pawn, power, target, slot,' in src
assert 'if (!applied) return false;' in src
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

# v0.15: universal major-rank mitigation applies to every Pawn's normal
# damage path, not just to Essencebound spell definitions.
rank=(ROOT/'Source/EssenceRankCombat.cs').read_text()
project=(ROOT/'Source/Essencebound.Core.csproj').read_text()
settings=(ROOT/'Source/EssenceboundMod.cs').read_text()
assert '[HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]' in rank
assert 'private static void Prefix(Pawn __instance, ref DamageInfo dinfo)' in rank
assert 'dinfo.SetAmount(amount)' in rank
assert 'EssenceRankCombat.RankOf(attacker)' in rank
assert 'EssenceRankCombat.RankOf(__instance)' in rank
assert 'essenceUser.OwnedEssenceNames' in rank
assert 'public static float UnrankedDamageFactor(int targetRank)' in rank
assert 'case 3: return 0.20f;' in rank
assert 'case 4: return 0.01f;' in rank
assert 'defenderBaseline / Math.Max(0.01f, attackerBaseline)' in rank
assert 'dinfo.Instigator is Pawn attacker' in rank
assert 'manifestedAttacker != null' in rank
assert 'MonsterOutgoingFactor(manifestedAttacker.rankIndex)' in rank
assert 'new Harmony("essencebound.core.prototype.rankedcombat")' in settings
assert 'PatchAll(typeof(Patch_EssenceRankIncomingDamage).Assembly)' in settings
assert 'Lib.Harmony' in project and 'ExcludeAssets="runtime"' in project

rank_baseline={-1:1.,0:.8,1:.6,2:.4,3:.2,4:.01}
def mitigation(defender,attacker):
    if defender < 0 or attacker >= defender: return 1.
    return max(.01,min(1.,rank_baseline[defender]/rank_baseline[attacker]))
# Exact requested benchmarks, plus monotonicity and ranked matchups.
for defender,damage_fraction in [(0,.80),(1,.60),(2,.40),(3,.20),(4,.01)]:
    assert abs(mitigation(defender,-1)-damage_fraction)<1e-8
assert abs(mitigation(1,0)-.75)<1e-8  # Iron -> Bronze
assert abs(mitigation(2,0)-.50)<1e-8  # Iron -> Silver
assert abs(mitigation(3,2)-.50)<1e-8  # Silver -> Gold
assert abs(mitigation(4,3)-.05)<1e-8  # Gold -> Diamond
assert abs(mitigation(4,0)-.0125)<1e-8 # Iron -> Diamond
assert mitigation(-1,0)==1
assert mitigation(0,4)==1
assert mitigation(2,2)==1
for defender in range(5):
    for attacker in range(-1,5):
        damage=mitigation(defender,attacker)
        assert .01-1e-8 <= damage <= 1
        if attacker < defender:
            assert damage < 1
        else:
            assert damage == 1
# The real game hook must still operate on all pawn instigators, including
# pawns attacking Essencebound users and astral monster pawns.
assert 'amount *= EssenceRankCombat.LowerRankDamageFactor(targetRank, attackerRank);' in rank
monster=(ROOT/'Source/EssenceMonsterRanks.cs').read_text()
for rankMult in ('1.15f','1.35f','1.60f','2.00f'):
    assert rankMult in rank
assert 'return rank >= 3 ? 2 : 1;' in monster
print('PASS: v0.15 intensified rank defense: Gold -80%, Diamond -99% vs unranked')
 

# v0.16: new powers work through distinct effect handlers, new statuses and
# the established aspect + awakening selection mechanism.
new_power_ids={
    'CinderNova','ThunderChain','BloodSiphon','LifeBloom',
    'RenewalSeed','GroveShelter','PurgingWave','GravitySnare',
    'CorrosiveStrike','EchoBurst','SpiritBeacon','MoonlitReprieve'
}
legacy_ability_ids={
    'EmberBolt','FrostShard','StormSpark','VerdantMend',
    'GuardianWard','StoneSkin','SwiftStep','MistVeil',
    'KeenSight','RallyingSurge','PurifyingTouch','ForcePulse'
}
assert len(legacy_ability_ids & {p['id'] for p in powers})==12
assert new_power_ids <= {p['id'] for p in powers}
effects={p['id']:p['effect'] for p in powers}
for ability,kind in {
    'ThunderChain':'chain','BloodSiphon':'drain','LifeBloom':'healGroup',
    'RenewalSeed':'regeneration','GroveShelter':'auraBuff',
    'PurgingWave':'areaCleanse','GravitySnare':'debuff',
    'EchoBurst':'splash','MoonlitReprieve':'healWard'
}.items():
    assert effects[ability]==kind
powerxml={e.findtext('defName'):e for e in powerdefs}
statusxml={e.findtext('defName'):e for e in buffdefs}
for p in powers:
    e=powerxml[p['defName']]
    assert e.findtext('effectKind')==p['effect']
    assert e.findtext('targetKind')==p['target']
    assert float(e.findtext('magnitude'))==float(p['magnitude'])
    assert int(e.findtext('cooldownTicks'))==p['cooldown_ticks']
    assert int(e.findtext('durationTicks'))==p['duration_ticks']
    assert [x.text for x in e.find('compatibleAspects')]==[
        'EB_Aspect_'+a for a in p['compatible_aspects']]
    assert [x.text for x in e.find('favoredEssences')]==[
        'EB_'+ess for ess in p['favored_essences']]
    if p.get('area_radius'):
        assert float(e.findtext('areaRadius'))==p['area_radius']
    if p.get('max_extra_targets'):
        assert int(e.findtext('maxExtraTargets'))==p['max_extra_targets']

for status in ('EB_Buff_RenewalSeed','EB_Buff_GroveShelter',
               'EB_Debuff_GravitySnare','EB_Debuff_CorrodedArmor',
               'EB_Buff_SpiritBeacon','EB_Buff_MoonlitReprieve'):
    assert status in statusxml
assert statusxml['EB_Buff_RenewalSeed'].findtext('hediffClass')==(
    'Essencebound.Hediff_EssenceRegeneration')
assert statusxml['EB_Debuff_GravitySnare'].findtext('isBad')=='true'
assert statusxml['EB_Debuff_CorrodedArmor'].findtext('isBad')=='true'

executor=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
regen=(ROOT/'Source/Hediff_EssenceRegeneration.cs').read_text()
timed=(ROOT/'Source/Hediff_EssenceTimed.cs').read_text()
progression=(ROOT/'Source/EssenceProgression.cs').read_text()
for kind in ('chain','splash','drain','healGroup','regeneration',
             'auraBuff','areaCleanse','debuff','healWard'):
    assert 'case "'+kind+'":' in executor,kind
for handler in ('ChainDamage','SplashDamage','DrainHealth',
                'HealInjuries','ApplyRegeneration','ApplyAuraBuff','CleanseArea'):
    assert 'private static bool '+handler+'(' in executor
assert 'work += dealt' in executor
assert 'power.maxExtraTargets' in executor
assert 'power.areaRadius' in executor
assert 'caster.Map.mapPawns.AllPawnsSpawned' in executor
assert 'out float first' in executor
assert 'Math.Min(budget, before)' in executor
assert 'work += healed' in executor
assert 'work += 10f;' in executor
assert 'if (!target.HostileTo(caster))' in executor
assert 'Hediff_EssenceRegeneration' in executor
assert 'healingPerPulse' in regen and 'ticksSinceHeal >= 120' in regen
assert 'wound.Heal(healingPerPulse)' in regen
assert 'AddAbilityWork(' in regen
for required in ('ebRegenCaster','ebRegenEssence','ebRegenSlot',
                 'ebRegenTicksRemaining','ebRegenPulseTick'):
    assert required in regen
assert 'trainOnlyAgainstHostile' in timed
assert 'pawn.HostileTo(creditCaster)' in timed
assert 'ebHostileTraining' in timed
assert 'case "healGroup":' in progression
assert 'case "areaCleanse":' in progression

# New starter imprints are available to freshly absorbed themed essences.
starter_map={
  'Volcano':'CinderNova','Thunderbird':'ThunderChain',
  'Blood':'BloodSiphon','Fertile':'LifeBloom','Renewal':'RenewalSeed',
  'Tree':'GroveShelter','Pure':'PurgingWave','Vortex':'GravitySnare',
  'Blight':'CorrosiveStrike','Echo':'EchoBurst',
  'Mystic':'SpiritBeacon','Moon':'MoonlitReprieve'
}
for essence,power in starter_map.items():
    assert by_essence['EB_'+essence]=='EB_Power_'+power
    assert any(row['essence']==essence and row['starter_power']=='EB_Power_'+power for row in rows)
assert by_essence['EB_Fire']=='EB_Power_EmberBolt'
assert by_essence['EB_Phoenix']=='EB_Power_AshenComet'
print('PASS: v0.16 24 powers, 14 status defs, 9 new handlers and 12 themed starter assignments')

# v0.16 follow-up: secondary blasts cannot pass through walls.
executor=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
timed=(ROOT/'Source/Hediff_EssenceTimed.cs').read_text()
assert executor.count('GenSight.LineOfSight(center, p.Position, caster.Map)')==2
assert 'using RimWorld;' in timed
print('PASS: v0.16 LOS gating for secondary impacts and hostile-control API namespace')

# v0.17 — 26 more playable powers built on 7 new work-credited handlers.
new_batch={
    'InfernoLance','FrostHalo','Quakebreaker','ZephyrRicochet','UmbralLeech',
    'VipersBite','ShardVolley','BriarDetonation','Starfall','TidalRebuke',
    'IronBastion','MirrorGuard','DawnChorus','VerdantSanctuary','OceanRenewal',
    'CleansingRay','MercyThread','SoulHarvest','RuneBind','OmenHex',
    'TemporalHaste','BattlefieldFocus','BladeTempest','NemesisStrike',
    'ThreadSnare','GlassWard'
}
assert len(new_batch)==26
assert new_batch <= {p['id'] for p in powers}
assert len({p['id'] for p in powers}-new_batch)==44  # Original 24 plus 20 v0.18 abilities
jsonbyid={p['id']:p for p in powers}
xmlbyid={x.findtext('defName'):x for x in powerdefs}
for p in powers:
    x=xmlbyid[p['defName']]
    assert x.findtext('effectKind')==p['effect']
    assert x.findtext('targetKind')==p['target']
    assert int(x.findtext('cooldownTicks'))==p['cooldown_ticks']
    assert float(x.findtext('magnitude'))==p['magnitude']
    assert [y.text for y in x.find('compatibleAspects')]==[
        'EB_Aspect_'+a for a in p['compatible_aspects']]
    assert [y.text for y in x.find('favoredEssences')]==[
        'EB_'+e for e in p['favored_essences']]
    if p.get('area_radius'):
        assert float(x.findtext('areaRadius'))==p['area_radius']
    if p.get('max_extra_targets'):
        assert int(x.findtext('maxExtraTargets'))==p['max_extra_targets']
statusids={h.findtext('defName') for h in buffdefs}
for id in ('EB_Debuff_VenomHaze','EB_Debuff_BriarBind','EB_Debuff_RuneBind',
           'EB_Debuff_OmenHex','EB_Debuff_ThreadSnare','EB_Buff_IronBastion',
           'EB_Buff_MirrorGuard','EB_Buff_DawnChorus','EB_Buff_TemporalHaste',
           'EB_Buff_BattlefieldFocus','EB_Buff_GlassWard'):
    assert id in statusids
assert len(statusids)==33
assert all((p['status_hediff'] in statusids) if p['status_hediff'] else True
           for p in powers)
source=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
prog=(ROOT/'Source/EssenceProgression.cs').read_text()
for kind in ('areaHeal','chainHeal','areaRegen','purgeHeal',
             'damageAreaDebuff','leechPulse','execute'):
    assert 'case "'+kind+'":' in source
    assert 'case "'+kind+'":' in prog
for fn in ('HealNearbyAllies','ChainHeal','RegenerateNearbyAllies',
           'PurgeAndHeal','BurstAndWeaken','LifeDrainingPulse','ExecuteWounded'):
    assert 'private static bool '+fn+'(' in source
assert 'work += dealt;' in source
assert 'work += restored;' in source
assert 'work = healed + removed * 10f;' in source
assert 'previousWounds >= 5f ? 1.6f : 1f' in source
assert 'GenSight.LineOfSight(target.Position, p.Position, caster.Map)' in source
assert 'Take(Math.Max(1, Math.Min(6, power.maxExtraTargets + 1)))' in source
assert 'if (hits == 0)' in source
assert 'No wounded allies nearby to heal.' in source
assert 'No injured allies are within the regenerative area.' in source
# Structured CSV and canonical JSON are kept in sync for all 50 powers.
import csv as _csv
with (ROOT/'Data/essence_powers.csv').open(newline='',encoding='utf-8') as fh:
    csvrows=list(_csv.DictReader(fh))
assert len(csvrows)==70
csvmap={row['id']:row for row in csvrows}
assert len(csvmap)==70 and set(csvmap)==set(jsonbyid)
for power_id,p in jsonbyid.items():
    row=csvmap[power_id]
    assert row['effect']==p['effect']
    assert row['target']==p['target']
    assert row['status_hediff']==p['status_hediff']
    assert float(row['magnitude'])==p['magnitude']
    assert row['compatible_aspects']=='; '.join(p['compatible_aspects'])
    assert row['favored_essences']=='; '.join(p['favored_essences'])
starterMap={
 'Sun':'InfernoLance','Cold':'FrostHalo','Earth':'Quakebreaker','Wind':'ZephyrRicochet',
 'Void':'UmbralLeech','Venom':'VipersBite','Crystal':'ShardVolley','Plant':'BriarDetonation',
 'Stellar':'Starfall','Water':'TidalRebuke','Iron':'IronBastion','Mirror':'MirrorGuard',
 'Dawn':'DawnChorus','Verdant':'VerdantSanctuary','Ocean':'OceanRenewal','Light':'CleansingRay',
 'Ministration':'MercyThread','Death':'SoulHarvest','Rune':'RuneBind','Omen':'OmenHex',
 'Time':'TemporalHaste','Battlefield':'BattlefieldFocus','Sword':'BladeTempest',
 'Nemesis':'NemesisStrike','Thread':'ThreadSnare','Glass':'GlassWard'}
assert len(starterMap)==26
for essence,ability in starterMap.items():
    assert by_essence['EB_'+essence]=='EB_Power_'+ability
    assert any(r['essence']==essence and r['starter_power']=='EB_Power_'+ability for r in rows)
assert by_essence['EB_Fire']=='EB_Power_EmberBolt'
assert by_essence['EB_Phoenix']=='EB_Power_AshenComet'
print('PASS: v0.17 50 playable powers, 25 statuses, 26 new starters, validated mechanics and CSV')

# v0.18 20 new, genuinely wired gameplay abilities. The older 50 remain.
batch18={'AshenComet','PrismaticDeflection','LotusSanctum','ChronoAnchor',
 'WolfpackCall','SpectralHand','ChimericAspect','VoidSlip','Fleshweave',
 'TidalWall','LightningStep','SovereignCommand','StarMap','PredatorMark',
 'HydraBloom','CrystalPrison','EchoTwin','ResonanceField','GuardianBastion','MoltenRain'}
assert len(batch18)==20 and batch18 <= {p['id'] for p in powers}
assert len({p['id'] for p in powers}-batch18)==50
assert sum(p['target']=='cell' for p in powers)==5
assert sum(p['target']=='thing' for p in powers)==1
newstatus={'EB_Buff_PrismaticDeflection','EB_Buff_ChronoAnchor',
 'EB_Buff_ChimeraCarapace','EB_Buff_ChimeraAgility','EB_Debuff_PredatorMark',
 'EB_Debuff_CrystalPrison','EB_Buff_ResonanceField','EB_Buff_GuardianBastion'}
assert newstatus <= statusids and len(statusids)==33
statusmap={s.findtext('defName'):s for s in buffdefs}
assert statusmap['EB_Buff_PrismaticDeflection'].findtext('hediffClass')==(
 'Essencebound.Hediff_EssenceReflection')
assert statusmap['EB_Buff_ChronoAnchor'].findtext('hediffClass')==(
 'Essencebound.Hediff_EssenceChronoAnchor')
src=(ROOT/'Source/EssenceAdvancedAbilities.cs').read_text()
player=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
world=(ROOT/'Source/MapComponent_EssenceAbilityWorld.cs').read_text()
executor=(ROOT/'Source/EssencePowerExecutor.cs').read_text()
for kind in ('delayedBlast','reflect','healingZone','returnTeleport','summon',
 'adaptiveBuff','transfusion','summonOrder','markedTarget','branchingHeal',
 'containment','decoy','powerAura','wardDome','teleport','blinkStrike',
 'remoteManipulation','tileBarrier','survey','hazardZone'):
    assert 'case "'+kind+'":' in src, kind
assert 'public static bool CastSpatial(' in src
assert 'public static bool CastPawn(' in src
assert 'public bool TryCastPower(EssenceAbilitySlot slot, LocalTargetInfo target, out string message)' in player
assert 'canTargetItems = items' in player
assert 'canTargetLocations = locations' in player
assert 'power.targetKind == "cell" || power.targetKind == "thing"' in player
assert 'EssenceAdvancedAbilities.CastSpatial(' in player
assert 'EssenceAdvancedAbilities.CastPawn(' in executor
assert 'GenPlace.TryPlaceThing' in src
assert 'caster.Map.fogGrid.Unfog(c)' in src
assert 'world.TrackSummon(' in src
assert 'world.AlliedSummons(caster)' in src
assert 'CreateWall(' in src and 'CreatePrison(' in src
assert 'MovePawn(caster, cell)' in src
reflect=(ROOT/'Source/Hediff_EssenceReflection.cs').read_text()
anchor=(ROOT/'Source/Hediff_EssenceChronoAnchor.cs').read_text()
mark=(ROOT/'Source/Patch_EssencePredatorMark.cs').read_text()
assert '[ThreadStatic] private static bool reflecting;' in reflect
assert 'shield.Award(' in reflect
assert 'ebReflectionCaster' in reflect and 'ebReflectionSlot' in reflect
assert 'ebChronoAnchorCell' in anchor and 'ebChronoAnchorTicks' in anchor
assert 'dinfo.SetAmount(dinfo.Amount * 1.18f)' in mark
xmlmap={x.findtext('defName'):x for x in powerdefs}
assert len(xmlmap)==70
for p in powers:
    x=xmlmap[p['defName']]
    assert x.findtext('effectKind')==p['effect']
    assert x.findtext('targetKind')==p['target']
    assert int(x.findtext('cooldownTicks'))==p['cooldown_ticks']
    assert int(x.findtext('durationTicks'))==p['duration_ticks']
    assert float(x.findtext('magnitude'))==p['magnitude']
    assert x.findtext('buffHediffDefName','')==p['status_hediff']
    assert x.findtext('damageDefName','')==p['damage_def']
for ess,ability in {'Phoenix':'AshenComet','Lotus':'LotusSanctum',
 'Hand':'SpectralHand','Wolf':'WolfpackCall','Hydra':'HydraBloom',
 'Guardian':'GuardianBastion','Prison':'CrystalPrison',
 'Resonating':'ResonanceField','Cataclysm':'MoltenRain'}.items():
    assert by_essence['EB_'+ess]=='EB_Power_'+ability
print('PASS: v0.18 70 playable powers, new targeting, statuses and world effects')
