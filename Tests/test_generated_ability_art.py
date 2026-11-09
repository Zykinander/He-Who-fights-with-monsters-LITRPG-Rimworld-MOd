"""Validate generated 100 transparent power icons and spell sigils."""
import hashlib
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
powers=json.loads((ROOT/"Data/essence_powers.json").read_text())["powers"]
art=json.loads((ROOT/"Data/ability_visual_designs.json").read_text())["abilities"]
names={p["id"] for p in powers}
assert len(names)==100 and names=={e["id"] for e in art}
xml=ET.parse(ROOT/"1.7/Defs/PowerDefs/EssencePowers.xml")
defs=xml.getroot().findall("Essencebound.EssencePowerDef")
flecks=ET.parse(ROOT/"1.7/Defs/FleckDefs/EssenceAbilitySigils.xml")
fdefs=flecks.getroot().findall("FleckDef")
assert len(defs)==len(fdefs)==100
for cat in ["AbilityIcons","AbilitySigils"]:
    directory=ROOT/"1.7"/"Textures"/"Essencebound"/cat
    assert {x.stem for x in directory.glob("*.png")}==names
    hashes=set()
    for path in sorted(directory.glob("*.png")):
        with Image.open(path) as image:
            assert image.size==(128,128) and image.mode=="RGBA"
            lo,hi=image.getchannel("A").getextrema()
            assert lo==0 and hi>190, (path,lo,hi)
        digest=hashlib.sha256(path.read_bytes()).hexdigest()
        assert digest not in hashes,"Duplicate icon pixels: "+path.name
        hashes.add(digest)
for node in defs:
    id=node.findtext("defName").removeprefix("EB_Power_")
    assert node.findtext("iconPath")=="Essencebound/AbilityIcons/"+id
for node in fdefs:
    id=node.findtext("defName").removeprefix("EB_AbilitySigil_")
    assert id in names
    assert node.attrib["ParentName"]=="FleckBase"
    assert node.findtext("graphicData/texPath")=="Essencebound/AbilitySigils/"+id
meta=json.loads((ROOT/"manifest.json").read_text())["visual_design_plan"]
assert meta["generated_icon_assets_in_packaged_build"]==100
assert meta["generated_cast_sigil_assets_in_packaged_build"]==100
print("PASS: 100 unique transparent 128x128 icons and matching sigils")
