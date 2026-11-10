"""Check source art identity, transparency, and all in-game directional textures."""
from pathlib import Path
from hashlib import sha1
import xml.etree.ElementTree as ET

from PIL import Image, ImageChops, ImageOps

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "ArtSource/Monsters"
OUTPUT = ROOT / "1.7/Textures/Essencebound/Monsters"
SCRIPT = (ROOT / "Tools/generate_monster_textures.py").read_text()
WORKFLOW = (ROOT / ".github/workflows/build-essencebound.yml").read_text()

# Git blob SHA verifies that no binary PNG bytes were truncated during upload.
EXPECTED = {
    "Glassmote": "b887cd9c110b5b5cae5c3ff62a2b41f184361475",
    "Emberfang": "6ec818abf17829fa3f3d8c31eb694c2e8c00e0b9",
    "Duskweaver": "12e46f0de80d125d48d5082dd922a8416e9025e4",
    "Stormhide": "18cff6d11de235d2ad7bd6741885ac7d11c322d8",
    "PrismaticSovereign": "ac5ce0556f8b246c1be0b06c70ba425929b6870d",
}
assert "python Tools/generate_monster_textures.py" in WORKFLOW
assert "python Tests/test_monster_art.py" in WORKFLOW
assert "ImageOps.mirror" in SCRIPT
assert 'SIZE = 256' in SCRIPT

for name, digest in EXPECTED.items():
    path = SOURCE / f"{name}.png"
    assert path.is_file(), f"Missing source art for {name}"
    data = path.read_bytes()
    blob = b"blob " + str(len(data)).encode("ascii") + b"\0" + data
    assert sha1(blob).hexdigest() == digest, f"Source PNG changed or was truncated: {name}"
    with Image.open(path) as img:
        assert img.size[0] >= 96 and img.size[1] >= 96
        alpha = img.convert("RGBA").getchannel("A")
        assert alpha.getextrema()[0] == 0
        assert alpha.getextrema()[1] > 150
    generated = {}
    for direction in ("north", "south", "east", "west"):
        out_path = OUTPUT / f"{name}_{direction}.png"
        assert out_path.is_file(), f"Missing {name} {direction}"
        with Image.open(out_path) as img:
            assert img.format == "PNG"
            assert img.size == (256, 256)
            generated[direction] = img.convert("RGBA").copy()
        assert generated[direction].getchannel("A").getextrema()[0] == 0
        assert generated[direction].getchannel("A").getextrema()[1] > 150
    assert ImageChops.difference(generated["west"],
        ImageOps.mirror(generated["east"])).getbbox() is None, name

xml = ET.parse(ROOT / "1.7/Defs/PawnKindDefs/AstralMonsterKinds.xml").getroot()
for pawn in xml.findall("PawnKindDef"):
    name = pawn.findtext("defName").removeprefix("EB_Monster_")
    assert name in EXPECTED
    for life in pawn.findall("lifeStages/li"):
        data = life.find("bodyGraphicData")
        assert data.findtext("texPath") == f"Essencebound/Monsters/{name}"
        assert data.findtext("graphicClass") == "Graphic_Multi"
        assert data.find("color") is None

print(f"PASS: {len(EXPECTED)} original source sprites, 20 transparent 256px directions, "
      "verified Git blob checksums, exact XML paths, and mirrored west views.")
