"""Generate RimWorld Graphic_Multi PNGs from original simplified monster art.

Run from any directory: python Tools/generate_monster_textures.py
Requires Pillow. North/south are provisional perspective adaptations rather
than separately illustrated views; replacing them later needs no XML changes.
"""
from pathlib import Path
from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "ArtSource" / "Monsters"
OUTPUT = ROOT / "1.7" / "Textures" / "Essencebound" / "Monsters"
CREATURES = ("Glassmote", "Emberfang", "Duskweaver", "Stormhide", "PrismaticSovereign")
SIZE = 256

def save_texture(image, path):
    if image.size != (SIZE, SIZE):
        raise ValueError(f"Unexpected texture size: {path}: {image.size}")
    if image.getchannel("A").getextrema() == (255, 255):
        raise ValueError(f"Missing transparency: {path}")
    image.save(path, format="PNG", optimize=True)

def generation_views(source):
    east = source.resize((SIZE, SIZE), Image.Resampling.LANCZOS)
    west = ImageOps.mirror(east)

    def narrow(factor, mirror=False):
        artwork = ImageOps.mirror(east) if mirror else east
        width = round(SIZE * factor)
        artwork = artwork.resize((width, SIZE), Image.Resampling.LANCZOS)
        result = Image.new("RGBA", (SIZE, SIZE))
        result.alpha_composite(artwork, ((SIZE - width) // 2, 0))
        return result

    return {"east": east, "west": west,
            "south": narrow(0.88), "north": narrow(0.80, mirror=True)}

def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for name in CREATURES:
        path = SOURCE / f"{name}.png"
        if not path.is_file():
            raise FileNotFoundError(f"Missing original monster artwork: {path}")
        with Image.open(path) as source_file:
            source = source_file.convert("RGBA")
        if min(source.size) < 96:
            raise ValueError(f"Source sprite is too small: {path}")
        for direction, image in generation_views(source).items():
            save_texture(image, OUTPUT / f"{name}_{direction}.png")
        print(f"Generated {name}: 4 x {SIZE}px transparent PNGs")

if __name__ == "__main__":
    main()
