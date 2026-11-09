"""Ensure exactly 100 complete visual concept specifications match the mod's power catalog."""
import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
source=json.loads((ROOT/"Data/essence_powers.json").read_text(encoding="utf-8"))
visual=json.loads((ROOT/"Data/ability_visual_designs.json").read_text(encoding="utf-8"))
manifest=json.loads((ROOT/"manifest.json").read_text(encoding="utf-8"))
assert visual["ability_count"] == 100
assert visual["status"] == "procedural_first_pass_generated_in_build"
assert len(visual["abilities"])==100
lookup={p["id"]:p for p in source["powers"]}
assert len(lookup)==100
ids=[d["id"] for d in visual["abilities"]]
assert len(set(ids))==100 and set(ids)==set(lookup)
shapes=[d["icon_silhouette"] for d in visual["abilities"]]
assert len(set(shapes))==100, "Every icon concept must be visually distinct"
for d in visual["abilities"]:
    p=lookup[d["id"]]
    assert d["name"]==p["name"]
    assert d["primary_category"]==p["primary_category"]
    assert d["primary_essence"]==p["primary_essence"]
    assert d["tags"]==p["tags"]
    assert d["existing_effect_kind"]==p["effect"]
    assert d["design_status"]=="procedural_first_pass"
    assert d["icon_asset_status"]=="generated_in_mod_zip"
    assert d["sigil_asset_path"]=="Essencebound/AbilitySigils/"+d["id"]
    assert d["audio_asset_status"]=="not_implemented"
    assert d["asset_path"]=="Essencebound/AbilityIcons/"+d["id"]
    for c in ("primary","highlight","shadow"):
        value=d["palette"][c]
        assert len(value)==7 and value[0]=="#" and all(
            ch in "0123456789abcdefABCDEF" for ch in value[1:])
    for field in ("icon_silhouette","cast_vfx","result_vfx","sound_direction"):
        assert len(d[field])>=13, (d["id"],field)
    assert len(d["cast_vfx"])>=26 and len(d["result_vfx"])>=33
assert manifest["visual_design_plan"]["ability_concepts"]==100
assert manifest["visual_design_plan"]["finished_unique_icon_assets"]==100
assert (ROOT/"ABILITY_VISUAL_DESIGN_GUIDE.md").exists()
print("PASS: exactly 100 unique visual design concepts; correct IDs, palette, cast, impact and status")
