"""Offline v0.5 structural checks. This cannot validate C# compilation or in-game execution."""
from pathlib import Path
import json
import collections
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
d = json.loads((ROOT/'Data/confluence_combinations.json').read_text())
rows = d['combinations']
assert len(rows) == 366
normal_names = {e for r in rows for e in r['essences']}
confluence_names = {r['confluence'] for r in rows}
assert len(normal_names) == 125 and len(confluence_names) == 101
counts = collections.defaultdict(set)
for r in rows:
    assert len(r['essences']) == 3 and len(set(r['essences'])) == 3
    assert r['essences'] == sorted(r['essences'],key=str.casefold)
    counts[tuple(r['essences'])].add(r['confluence'])
conflicted = {triple:outs for triple,outs in counts.items() if len(outs)>1}
assert len(conflicted) == 2
assert all(r['conflict'] == (tuple(r['essences']) in conflicted) for r in rows)

xmlpaths = list((ROOT/'1.7/Defs').rglob('*.xml'))
assert len(xmlpaths) == 19, xmlpaths
for p in xmlpaths:
    ET.parse(p)

def definitions(path, tag=None):
    els = list(ET.parse(ROOT/path).getroot())
    return [e for e in els if tag is None or e.tag==tag]

items = definitions('1.7/Defs/ThingDefs/EssenceCrystals.xml')
assert len([e for e in items if e.attrib.get('ParentName')=='EB_EssenceItemBase'])==125
confluence = definitions('1.7/Defs/ConfluenceDefs/ConfluenceRecipes.xml')
assert len(confluence)==366

stones_data = json.loads((ROOT/'Data/awakening_stones.json').read_text())['stones']
assert len(stones_data)==30
assert collections.Counter(s['rarity'] for s in stones_data)=={
    'Common':10,'Uncommon':8,'Rare':6,'Epic':4,'Legendary':2}

stones_xml = definitions('1.7/Defs/AwakeningDefs/AwakeningStones.xml','Essencebound.AwakeningStoneDef')
aspects_xml = definitions('1.7/Defs/AwakeningDefs/AwakeningAspects.xml','Essencebound.AwakeningAspectDef')
stone_items = definitions('1.7/Defs/ThingDefs/AwakeningStones.xml')
assert len(stones_xml)==30 and len(aspects_xml)==21
assert len([e for e in stone_items if e.attrib.get('ParentName')=='EB_AwakeningStoneItemBase']) == 30
# v0.08: RimWorld 1.7 requires stackable items to show their stack-count overlay.
stone_base=next(e for e in stone_items if e.attrib.get('Name')=='EB_AwakeningStoneItemBase')
assert stone_base.findtext('stackLimit')=='10'
assert stone_base.findtext('drawGUIOverlay')=='true'
assert all(e.findtext('drawGUIOverlay') != 'false' for e in stone_items if e.attrib.get('ParentName')=='EB_AwakeningStoneItemBase')


def child_text(el,k):
    c=el.find(k)
    assert c is not None, (el.tag,k)
    return c.text

def children(el,k):
    node=el.find(k)
    assert node is not None,(el.tag,k)
    return [n.text for n in node]

all_aspects={child_text(e,'defName') for e in aspects_xml}
all_essences={'EB_'+name for name in normal_names} | {'EB_'+name for name in confluence_names}
for st in stones_xml:
    focus=children(st,'possibleAspects')
    favorites=children(st,'favoredEssences')
    assert len(focus)==3 and len(set(focus))==3
    assert all(x in all_aspects for x in focus)
    assert all(x in all_essences for x in favorites)
    assert child_text(st,'sourceStatus') in {'community_listed_known_rarity','community_listed_rarity_unknown','mod_original_unverified'}

base_names=[]
for p in xmlpaths:
    for element in ET.parse(p).getroot():
        if element.find('defName') is not None:
            base_names.append(element.find('defName').text)
assert len(base_names)==len(set(base_names)),[x for x,c in collections.Counter(base_names).items() if c>1]

texdir=ROOT/'1.7/Textures/Essencebound/AwakeningStones'
for st in stones_data:
    assert (ROOT/'1.7/Textures'/ (st['texture_path']+'.png')).is_file()
    assert st['item_defName'] in {child_text(e,'defName') for e in stone_items if e.attrib.get('ParentName')}
    assert st['stone_defName'] in {child_text(e,'defName') for e in stones_xml}

# Static source safeguards, also require actual RimWorld play-testing.
source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
slot=(ROOT/'Source/EssenceAbilitySlot.cs').read_text()
stone_comp=(ROOT/'Source/CompAwakeningStoneItem.cs').read_text()
assert 'i < SlotsPerEssence' in source # dynamic 2-5 slots per essence
assert 'slots.Count != SlotsPerEssence' in source
assert 's.IsAwakened' in source and 'slot.awakeningStoneDefName = stone.defName' in source
assert 'slot.awakeningAspectDefName = chosen.aspect.defName' in source
assert 'Scribe_Values.Look(ref awakeningStoneDefName' in slot
assert 'Scribe_Values.Look(ref awakeningAspectDefName' in slot
assert 'parent.Destroy( DestroyMode.Vanish' not in stone_comp
assert 'parent.Destroy(DestroyMode.Vanish)' in stone_comp
assert 'current.TryAwaken(stone, capturedName' in stone_comp
print('PASS: 366 recipes, 125 normal essences, 101 confluences; 2 unresolved conflicts')
print('PASS: 30 stones, 21 aspects, 30 unique stone textures; XML source refs and rarities consistent')
print('PASS: source includes 2-5 slot cap, persistent imprints, adjacency consume interaction')
print('NOT CHECKED: C# build, runtime loading, game UX, in-game effects')

# All stone sprites should have a dedicated unique file, correct dimensions and transparency.
from PIL import Image
texture_paths = [s['texture_path'] for s in stones_data]
assert len(texture_paths) == len(set(texture_paths)) == 30
for st in stones_data:
    assert st['art_status'] == 'original_png'
    img_path=ROOT/'1.7/Textures'/ (st['texture_path']+'.png')
    with Image.open(img_path) as image:
        assert image.size == (128,128), (st['name'],image.size)
        assert image.mode == 'RGBA', (st['name'],image.mode)
        assert image.getpixel((0,0))[3] == 0
print('PASS: all 30 icons unique path, 128px RGBA, transparent corners')

# Release metadata must use the development-version scheme.
metadata=ET.parse(ROOT/'About/About.xml').getroot()
assert 'v0.19' in metadata.findtext('name')
assert json.loads((ROOT/'manifest.json').read_text())['version']=='0.19'
print('PASS: v0.13 metadata and inherited stack-count overlays')

# Regression: each GitHub Actions run must have a unique versioned artifact name.
workflow=(ROOT/'.github/workflows/build-essencebound.yml').read_text()
assert 'id: build_name' in workflow
assert 'manifest.json' in workflow
assert '${GITHUB_RUN_NUMBER}' in workflow
assert '${GITHUB_RUN_ATTEMPT}' in workflow
assert 'name: ${{ steps.build_name.outputs.name }}' in workflow
assert 'path: ${{ steps.build_name.outputs.name }}-mod.zip' in workflow
assert 'cd build && zip -qr "../${{ steps.build_name.outputs.name }}-mod.zip" Essencebound' in workflow
print('PASS: versioned GitHub Actions test-build archive names')

# v0.10: four essence panels with configurable per-essence slot count.
setting_source=(ROOT/'Source/EssenceboundMod.cs').read_text()
dialog_source=(ROOT/'Source/Dialog_EssenceProfile.cs').read_text()
assert 'GetSettings<EssenceboundSettings>()' in setting_source
assert 'Scribe_Values.Look(ref abilitySlotsPerEssence' in setting_source
assert 'Math.Max(2, Math.Min(5' in setting_source
assert 'public override string SettingsCategory()' in setting_source
assert 'public override void DoSettingsWindowContents(Rect inRect)' in setting_source
assert 'i < SlotsPerEssence' in source
assert 's.index < SlotsPerEssence' in source
assert 'slot.index >= SlotsPerEssence' in source
assert 'var slots = GetActiveSlotsFor(essenceName)' in source
assert 'new Dialog_EssenceProfile(this)' in source
assert 'NORMAL ESSENCE I' in dialog_source and 'NORMAL ESSENCE II' in dialog_source
assert 'NORMAL ESSENCE III' in dialog_source and 'CONFLUENCE ESSENCE' in dialog_source
assert 'for (int index = 0; index < cap; index++)' in dialog_source
assert all(rank in dialog_source for rank in ['Iron','Bronze','Silver','Gold','Diamond'])
assert 'state.PausedPowersCount' in dialog_source
manifest=json.loads((ROOT/'manifest.json').read_text())
assert manifest['counts']['min_awakening_slots_per_essence']==2
assert manifest['counts']['max_awakening_slots_per_essence']==5
assert manifest['counts']['max_awakening_slots']==20
print('PASS: v0.10 configurable ability slots 2-5 and four-panel profile')

# v0.11 — every essence has positive attribute weights matching editable CSV.
import csv
attr_rows=list(csv.DictReader((ROOT/'Data/essence_attribute_weights.csv').open(encoding='utf-8')))
assert len(attr_rows)==226
csv_by_name={row['essence']:row for row in attr_rows}
assert len(csv_by_name)==226 and set(csv_by_name)==all_essences
for source_path in ('NormalEssences','ConfluenceEssences'):
    defs=definitions('1.7/Defs/EssenceDefs/'+source_path+'.xml','Essencebound.EssenceDef')
    for essence in defs:
        key=child_text(essence,'defName')
        row=csv_by_name[key]
        for attribute in ['power','speed','spirit','recovery']:
            xmlval=float(child_text(essence,attribute+'Weight'))
            assert xmlval>0 and xmlval==float(row[attribute+'_weight'])
assert [csv_by_name['EB_Fire'][x+'_weight'] for x in ['power','speed','spirit','recovery']]==['6','1','5','1']
assert [csv_by_name['EB_Balance'][x+'_weight'] for x in ['power','speed','spirit','recovery']]==['3','3','3','3']
progression_source=(ROOT/'Source/EssenceProgression.cs').read_text()
rank_source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
slot_source=(ROOT/'Source/EssenceAbilitySlot.cs').read_text()
assert 'Rand.Value * (power + speed + spirit + recovery)' in progression_source
assert 'Scribe_Values.Look(ref attribute' in progression_source
assert 'Scribe_Collections.Look(ref affinities, "essenceAffinities", LookMode.Deep)' in rank_source
assert 'Scribe_Collections.Look(ref essenceRanks, "essenceRanks", LookMode.Deep)' in rank_source
assert 'Scribe_Values.Look(ref abilityRankIndex' in slot_source
assert 'Scribe_Values.Look(ref rankWork' in slot_source
assert 'requiredSlotCount = Math.Max(record.requiredSlotCount, SlotsPerEssence)' in rank_source
assert 'required.Any(s => !s.IsAwakened || s.abilityRankIndex <= record.rankIndex)' in rank_source
assert 'TryAdvanceEssenceRank(essenceName)' in rank_source
assert [40,120,360,1080] == [int(x) for x in __import__('re').findall(r'([0-9]+)f',progression_source.split('private static readonly float[] Thresholds = {',1)[1].split('}',1)[0])]
print('PASS: 226 weighted random essence affinities and save-safe progression gates')

# v0.11 rank bonuses are exponential rather than flat additions.
bonus_source=(ROOT/'Source/EssenceProgression.cs').read_text()
rank_source=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
assert 'return 1 << clamped;' in bonus_source
assert 'Math.Max(0, Math.Min(4, rankIndex))' in bonus_source
assert 'EssenceProgression.RankBonusMultiplier(tier, level)' in rank_source
assert 'totalBonus += EssenceProgression.RankBonusMultiplier(tier, level)' in rank_source
assert 'EssenceProgression.MaxAttributeBonus(attribute)' in rank_source
assert 'attribute == "Speed" ? 0.80f : 4.00f' in bonus_source
metadata=json.loads((ROOT/'manifest.json').read_text())
assert metadata['progression']['bonus_rank_multipliers']==[1,2,4,8,16]
assert len(metadata['progression']['attribute_bonus_caps'])==4
# Numerical examples of the exact rank multipliers and per-rank base rates.
bases={'Power':0.04,'Speed':0.025,'Spirit':0.05,'Recovery':0.06}
for attribute,base in bases.items():
    values=[base*(1<<rank) for rank in range(5)]
    assert all(abs(values[i+1]-2*values[i])<1e-8 for i in range(4))
assert abs(bases['Recovery']*16-0.96)<1e-8
assert abs(bases['Power']*16-0.64)<1e-8
print('PASS: exponential x1/x2/x4/x8/x16 affinity bonuses, including Diamond Recovery')

# v0.13: 0-9 sublevels, cross-essence tier gating and smooth per-level bonuses.
sublevel_src=(ROOT/'Source/EssenceAbilitySlot.cs').read_text()
progress_src=(ROOT/'Source/EssenceProgression.cs').read_text()
rank_src=(ROOT/'Source/Hediff_EssenceUser.cs').read_text()
profile_src=(ROOT/'Source/Dialog_EssenceProfile.cs').read_text()
assert 'Scribe_Values.Look(ref abilitySublevel, "abilitySublevel", -1)' in sublevel_src
assert 'LoadSaveMode.PostLoadInit' in sublevel_src
assert 'rankWork - abilitySublevel * step' in sublevel_src
assert 'return NextRankWork(currentRank) / 10f;' in progress_src
assert 'public static string RankWithLevel(int majorRank, int sublevel)' in progress_src
assert 'public static float RankBonusMultiplier(int rankIndex, int sublevel)' in progress_src
assert 'major * (1f + Math.Max(0, Math.Min(9, sublevel)) / 10f)' in progress_src
assert 'GetEssenceSublevel(string essenceName)' in rank_src
assert 'GetEssenceRankDisplay(string essenceName)' in rank_src
assert 'required.Min(s => s.abilityRankIndex > record.rankIndex ? 9 : s.abilitySublevel)' in rank_src
assert 'IsTierTrainingStalled(int abilityTier)' in rank_src
assert 'OwnedEssenceNames.Any(name' in rank_src
assert 'slot.abilitySublevel < 9' in rank_src and 'slot.abilityRankIndex++' in rank_src
assert 'state.GetEssenceRankDisplay(essenceName)' in profile_src
assert 'WAITING FOR OTHER ESSENCES' in profile_src
progress_meta=json.loads((ROOT/'manifest.json').read_text())['progression']
assert progress_meta['sublevels_per_major_rank']==10
assert [(x/10) for x in [40,120,360,1080]]==[4,12,36,108]
for tier_base in [1,2,4,8]:
    assert abs(tier_base*(1+1/10)-tier_base*1.1)<1e-8
    assert tier_base*(1+9/10)<tier_base*2
assert abs(0.04*1.1-0.044)<1e-8
assert abs(0.04*1.9-0.076)<1e-8
assert abs(0.04*2.0-0.08)<1e-8
print('PASS: v0.13 rank sublevels, synchronized tiers, smooth attribute bonuses')

# v0.13: spatial magic density and astral tears are real saveable game objects.
world=(ROOT/'Source/EssenceWorld.cs').read_text(encoding='utf-8')
tear=(ROOT/'Source/Thing_AstralTear.cs').read_text(encoding='utf-8')
marker=(ROOT/'Source/Hediff_AstralManifestation.cs').read_text(encoding='utf-8')
settings=(ROOT/'Source/EssenceboundMod.cs').read_text(encoding='utf-8')
defs=definitions('1.7/Defs/ThingDefs/AstralTears.xml')
marks=definitions('1.7/Defs/HediffDefs/AstralManifestations.xml')
assert len(defs)==1 and len(marks)==1
assert child_text(defs[0],'defName')=='EB_AstralTear'
assert child_text(defs[0],'tickerType')=='Normal'
assert child_text(defs[0],'thingClass')=='Essencebound.Thing_AstralTear'
assert child_text(marks[0],'defName')=='EB_AstralManifestation'
assert child_text(marks[0],'hediffClass')=='Essencebound.Hediff_AstralManifestation'
assert (ROOT/'1.7/Textures/Essencebound/AwakeningStones/StoneMagus.png').exists()
assert 'public sealed class MapComponent_EssenceWorld : MapComponent' in world
assert 'public override void MapComponentTick()' in world
assert 'ebNextManifestationCheck' in world and 'ebAmbientDensity' in world
assert 'private float ambientDensity' not in world
assert 'map.IsPlayerHome' in world and 'FreeColonistsSpawned.Count == 0' in world
assert 'EssenceboundMod.MagicDensityMultiplier' in (ROOT/'Source/EssenceDensityCalculator.cs').read_text()
assert '0.15f + 0.65f * intensity' in world
assert 'Rand.Value < chance' in world
assert 'monster.mindState.mentalStateHandler.TryStartMentalState' in world
assert 'public static bool SpawnEssence(' in world
assert 'Essencebound.Thing_AstralTear' in (ROOT/'1.7/Defs/ThingDefs/AstralTears.xml').read_text()
assert 'ticksUntilResolution' in tear and 'Scribe_Values.Look' in tear
assert 'Scribe_Values.Look(ref magicDensity' in marker
for setting in ('manifestationsEnabled','manifestationMonstersEnabled',
                'manifestationFrequency','densityPreset'):
    assert setting in settings
assert json.loads((ROOT/'manifest.json').read_text())['version']=='0.19'
print('PASS: v0.13 saveable magic density, tears, monster tiering and XML')

# v0.14: monster danger follows actual colony progression, not random base magic.
import math
world_source=(ROOT/'Source/EssenceWorld.cs').read_text()
density_source=(ROOT/'Source/EssenceDensityCalculator.cs').read_text()
settings_source=(ROOT/'Source/EssenceboundMod.cs').read_text()
assert 'EssenceDensityCalculator.ForMap(map)' in world_source
assert 'public float MagicDensity => DensityReport.FinalDensity' in world_source
assert 'tear.Initialize(densityAtOpening)' in world_source
assert 'float chance = Math.Min(0.8f' in world_source
assert '0.15f + 0.65f * intensity' in world_source
assert 'private float ambientDensity' not in world_source
assert 'Rand.Value' not in density_source
assert 'map.wealthWatcher?.WealthTotal' in density_source
assert 'map.mapPawns.FreeColonistsSpawned' in density_source
assert 'SkillDefOf.Shooting' in density_source and 'SkillDefOf.Melee' in density_source
assert 'Hediff_EssenceUser.ForPawn(pawn)' in density_source
assert 'user.GetEssenceRankIndex(essence)' in density_source
assert 'user.GetEssenceSublevel(essence)' in density_source
assert '0.23f * wealthFactor' in density_source
assert '0.14f * populationFactor' in density_source
assert '0.12f * combatFactor' in density_source
assert '0.42f * essenceFactor' in density_source
assert 'report.Value.FinalDensity' in settings_source
assert 'report.Value.EssenceFactor' in settings_source

def density_for(wealth, people, combat, magic, multiplier=1):
    clamp=lambda v:max(0,min(1,v))
    w=clamp(math.sqrt(max(0,wealth)/200000))
    p=clamp(math.sqrt(max(0,people)/12))
    c=clamp(combat/20)
    e=clamp(magic/18)
    base=(0.09+0.23*w+0.14*p+0.12*c+0.42*e) if people else 0.05
    return max(0.05,min(1,base*max(0,multiplier)))

starter=density_for(10000,2,4,0.5)
midgame=density_for(70000,5,9,5)
late=density_for(250000,12,17,18)
assert 0.05 < starter < midgame < late <= 1
assert density_for(70000,5,9,9)>density_for(70000,5,9,5)
assert density_for(70000,5,9,5,1.45)>midgame
assert density_for(70000,5,9,5,0.65)<midgame
assert density_for(70000,5,9,5)==density_for(70000,5,9,5)
assert density_for(0,0,0,0)>=0.05
assert density_for(10**9,100,20,100)<=1
manifest=json.loads((ROOT/'manifest.json').read_text())
assert manifest['version']=='0.19'
assert manifest['world_integration']['density_weights']['essence']==0.42
print('PASS: v0.14 deterministic strength-based density scales up with colony strength')

# v0.15: all manifestation tiers appear visibly and save correctly.
stage_defs=definitions('1.7/Defs/HediffDefs/AstralManifestations.xml','HediffDef')
assert len(stage_defs)==1
stage=stage_defs[0]
assert child_text(stage,'maxSeverity')=='5'
rank_stages=stage.find('stages')
assert rank_stages is not None and len(rank_stages)==5
assert [x.findtext('label') for x in rank_stages] == [
    'Iron astral manifestation','Bronze astral manifestation',
    'Silver astral manifestation','Gold astral manifestation',
    'Diamond astral manifestation'
]
assert [x.findtext('minSeverity') for x in rank_stages][1:] == ['2','3','4','5']
monster_src=(ROOT/'Source/EssenceMonsterRanks.cs').read_text()
world=(ROOT/'Source/EssenceWorld.cs').read_text()
mark=(ROOT/'Source/Hediff_AstralManifestation.cs').read_text()
assert 'public static int[] WeightsForDensity(float density)' in monster_src
assert 'public static int Roll(float density)' in monster_src
for expected in ('95, 5, 0, 0, 0','58, 39, 3, 0, 0',
                 '15, 55, 27, 3, 0','0, 15, 65, 19, 1','0, 0, 20, 75, 5'):
    assert expected in monster_src
assert 'EssenceMonsterRanks.Roll(intensity)' in world
assert 'marker.rankIndex = monsterRank' in world
assert 'marker.Severity = monsterRank + 1f' in world
assert 'EssenceMonsterRanks.GroupSize(monsterRank)' in world
assert 'ebMonsterRankIndex' in mark
assert 'Scribe_Values.Look(ref magicDensity' in mark
meta=json.loads((ROOT/'manifest.json').read_text())
assert meta['version']=='0.19'
assert meta['monster_progression']['ranks']==['Iron','Bronze','Silver','Gold','Diamond']
assert all(sum(weights)==100 for weights in meta['monster_progression']['rank_weights_by_magic_density'].values())
assert meta['requires_harmony']
modxml=ET.parse(ROOT/'About/About.xml').getroot()
assert modxml.findtext('modDependencies/li/packageId') == 'brrainz.harmony'
print('PASS: v0.15 5 ranked manifestation tiers, persistent rank label, Harmony requirement')

# v0.17 — milestone tracks 100 unique ability designs; 50 are executable now.
import csv as _roster_csv
with (ROOT/'Data/ability_roster_100.csv').open(encoding='utf-8',newline='') as _file:
    _roster=list(_roster_csv.DictReader(_file))
assert len(_roster)==100
assert len({r['id'] for r in _roster})==100
assert sum(r['status']=='playable' for r in _roster)==100
assert sum(r['status']=='planned' for r in _roster)==0
assert set(r['id'] for r in _roster if r['status']=='playable')=={
    p['id'] for p in json.loads((ROOT/'Data/essence_powers.json').read_text())['powers']}
assert all(r['primary_essence'] in {
    e.findtext('defName').removeprefix('EB_')
    for n in ['NormalEssences','ConfluenceEssences']
    for e in definitions('1.7/Defs/EssenceDefs/'+n+'.xml')} for r in _roster)
assert {r['target_milestone'] for r in _roster}=={'v0.17','v0.18','v0.19'}
assert sum(r['target_milestone']=='v0.18' for r in _roster)==20
assert sum(r['target_milestone']=='v0.19' for r in _roster)==30
assert sum(r['target_milestone']=='v0.20' for r in _roster)==0
assert all(r['mechanic'] and r['source_status']=='original_mod_gameplay_design' for r in _roster)
assert json.loads((ROOT/'manifest.json').read_text())['ability_plan']['minimum_playable_target']==100
print('PASS: v0.17 100 unique themed designs, 50 playable and 50 future milestones')

# v0.18: saveable advanced battlefield effects and 70/100 milestone.
barriers=definitions('1.7/Defs/ThingDefs/EssenceBarriers.xml','ThingDef')
assert len(barriers)==1
assert child_text(barriers[0],'defName')=='EB_TemporaryEssenceBarrier'
assert child_text(barriers[0],'passability')=='Impassable'
assert child_text(barriers[0],'tickerType')=='Normal'
world=(ROOT/'Source/MapComponent_EssenceAbilityWorld.cs').read_text()
assert 'class MapComponent_EssenceAbilityWorld : MapComponent' in world
assert 'Scribe_Collections.Look(ref effects, "ebAbilityWorldEffects", LookMode.Deep)' in world
assert 'public void TrackSummon(' in world
assert 'public List<Pawn> AlliedSummons(' in world
for kind in ('delayedBlast','hazardZone','healingZone','powerAura','wardDome'):
    assert 'case "'+kind+'":' in world
assert 'EssencePowerExecutor.FieldDamage' in world
assert 'EssencePowerExecutor.FieldHealing' in world
assert 'EssencePowerExecutor.FieldBuff' in world
m=json.loads((ROOT/'manifest.json').read_text())
assert m['version']=='0.19'
assert m['counts']['playable_power_defs']==100
assert m['counts']['timed_power_effect_defs']==53
assert m['ability_plan']['minimum_playable_target']==100
assert m['ability_plan']['current_playable']==100
assert m['ability_plan']['remaining_to_implement']==0
assert sum(r['status']=='playable' and r['target_milestone']=='v0.18' for r in _roster)==20
assert sum(r['status']=='planned' and r['target_milestone']=='v0.18' for r in _roster)==0
print('PASS: v0.18 70/100 ability roster, field saves and barrier XML')

# v0.19 — the user-requested direct jump from 70 to 100.
assert len(_roster)==100
assert all(r['status']=='playable' for r in _roster)
assert not any(r['target_milestone']=='v0.20' for r in _roster)
assert sum(r['target_milestone']=='v0.19' for r in _roster)==30
assert len({r['id'] for r in _roster})==100
meta=json.loads((ROOT/'manifest.json').read_text())
assert meta['ability_plan']['current_playable']==100
assert meta['ability_plan']['remaining_to_implement']==0
assert meta['ability_plan']['this_version_added']==30
assert meta['version']=='0.19'
print('PASS: v0.19 direct jump to 100 playable abilities and zero unimplemented roster items')


# Taxonomy metadata only — no changes to rank, aspect or ability limits.
import csv as _tag_csv
with (ROOT/'Data/ability_roster_100.csv').open(encoding='utf-8',newline='') as fp:
    tagged_roster=list(_tag_csv.DictReader(fp))
taxonomy_meta=json.loads((ROOT/'manifest.json').read_text())['ability_taxonomy']
assert len(tagged_roster)==100
assert taxonomy_meta['abilities_tagged']==100
assert sum(taxonomy_meta['primary_category_counts'].values())==100
assert set(taxonomy_meta['allowed_primary_categories'])=={
    'Attack','Defense','Healing','Mobility','Summoning','Control','Utility'
}
powers_by_id={p['id']:p for p in json.loads((ROOT/'Data/essence_powers.json').read_text())['powers']}
for row in tagged_roster:
    p=powers_by_id[row['id']]
    assert row['primary_essence']==p['primary_essence']
    assert row['primary_category']==p['primary_category']
    assert row['tags']=='; '.join(p['tags'])
    assert row['other_favored_essences']=='; '.join(p['favored_essences'][1:])
assert (ROOT/'ABILITY_CATALOG_BY_ESSENCE_AND_TAGS.md').exists()
print('PASS: 100-ability essence/category index and taxonomy manifest totals')
