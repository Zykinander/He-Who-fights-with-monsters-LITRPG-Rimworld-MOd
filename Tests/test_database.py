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
assert len(xmlpaths) == 13, xmlpaths
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
assert 'i < 3' in source # only three slots per essence
assert 'slots.Count != 3' in source
assert 's.IsAwakened' in source and 'slot.awakeningStoneDefName = stone.defName' in source
assert 'slot.awakeningAspectDefName = chosen.aspect.defName' in source
assert 'Scribe_Values.Look(ref awakeningStoneDefName' in slot
assert 'Scribe_Values.Look(ref awakeningAspectDefName' in slot
assert 'parent.Destroy( DestroyMode.Vanish' not in stone_comp
assert 'parent.Destroy(DestroyMode.Vanish)' in stone_comp
assert 'current.TryAwaken(stone, capturedName' in stone_comp
print('PASS: 366 recipes, 125 normal essences, 101 confluences; 2 unresolved conflicts')
print('PASS: 30 stones, 21 aspects, 30 unique stone textures; XML source refs and rarities consistent')
print('PASS: source includes 3-slot cap, persistent imprints, adjacency consume interaction')
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
