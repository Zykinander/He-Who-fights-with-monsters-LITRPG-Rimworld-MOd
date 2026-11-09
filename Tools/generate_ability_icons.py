#!/usr/bin/env python3
"""Generate 100 original, transparent 128x128 RPG-style power icons.

Run from anywhere: python Tools/generate_ability_icons.py
Uses the versioned visual brief. Exports PNGs in the mod texture folder.
This script intentionally makes clean, readable PROCEDURAL illustrations,
not hand-painted icons nor frame-by-frame spell animations.
"""
import hashlib
import json
import math
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "Data" / "ability_visual_designs.json"
OUT = ROOT / "1.7" / "Textures" / "Essencebound" / "AbilityIcons"
SCALE = 4
SIZE = 128 * SCALE


def point(x, y):
    return (int(round(x * SCALE)), int(round(y * SCALE)))


def color(hex_code, alpha=255):
    return tuple(bytes.fromhex(hex_code.lstrip("#"))) + (alpha,)


def mid(a, b, t):
    return tuple(round(a[i] * (1 - t) + b[i] * t) for i in range(3)) + (255,)


def line(d, points, fill, width=3, joint="curve"):
    d.line([point(*p) for p in points], fill=fill, width=max(1, round(width * SCALE)), joint=joint)


def ellipse(d, bounds, fill=None, outline=None, width=2):
    x0, y0, x1, y1 = bounds
    d.ellipse((*point(x0, y0), *point(x1, y1)), fill=fill, outline=outline,
              width=max(1, round(width * SCALE)))


def polygon(d, points, fill, outline=None, width=2):
    pts = [point(*p) for p in points]
    d.polygon(pts, fill=fill)
    if outline:
        line(d, points + [points[0]], outline, width)


def arc(d, bounds, start, end, fill, width=3):
    x0, y0, x1, y1 = bounds
    d.arc((*point(x0, y0), *point(x1, y1)), start, end, fill=fill,
          width=max(1, round(width * SCALE)))


def petal(d, cx, cy, a, length, spread, tint, outline):
    ux, uy = math.cos(a), math.sin(a)
    nx, ny = -uy, ux
    root = (cx + ux * 7, cy + uy * 7)
    tip = (cx + ux * length, cy + uy * length)
    polygon(d, [root, (cx + ux * length * .59 + nx * spread,
                        cy + uy * length * .59 + ny * spread),
                tip, (cx + ux * length * .59 - nx * spread,
                      cy + uy * length * .59 - ny * spread)],
            tint, outline, 1.25)


def crystal(d, x, y, height, width, primary, light, shadow):
    polygon(d, [(x, y-height/2), (x+width/2, y-height/5),
                (x+width/3, y+height/2), (x-width/3, y+height/2),
                (x-width/2, y-height/5)], shadow, primary, 2)
    polygon(d, [(x, y-height/2), (x+width/2, y-height/5),
                (x, y+height/4)], primary)
    line(d, [(x, y-height/2), (x, y+height/3)], light, 1.6)


def star(d, cx, cy, outer, inner, count, col, edge, rotation=-math.pi/2):
    vertices=[]
    for i in range(count*2):
        a=rotation+i*math.pi/count
        r=outer if i%2==0 else inner
        vertices.append((cx+r*math.cos(a),cy+r*math.sin(a)))
    polygon(d, vertices, col, edge, 1.5)


def shield(d, primary, light, shadow):
    polygon(d, [(64,28),(91,39),(88,72),(64,97),(40,72),(37,39)],
            shadow, primary,3.5)
    polygon(d, [(64,35),(84,43),(82,69),(64,86),(46,69),(44,43)],
            primary)
    line(d,[(64,36),(64,84)],light,2.7)
    line(d,[(49,52),(79,52)],light,2.7)


def leaf(d, primary, light, shadow, rotation=0):
    # Midrib and opposing curved leaf edges.
    polygon(d, [(39,88),(43,58),(62,37),(91,29),(89,62),(68,84)],
            shadow,primary,3)
    line(d,[(40,88),(56,69),(73,50),(89,31)],light,3)
    for x,y,dx,dy in [(57,68,2,17),(69,56,16,3),(79,44,7,10)]:
        line(d,[(x,y),(x+dx,y+dy)],light,1.6)


def flame(d, primary, light, shadow):
    polygon(d,[(64,23),(77,47),(71,53),(86,41),(92,68),
               (82,91),(62,100),(43,87),(38,68),(50,49),(53,64)],
            primary,light,3)
    polygon(d,[(63,57),(76,72),(70,91),(55,91),(52,77)],
            light)
    line(d,[(44,83),(38,72),(43,58)],shadow,2)


def bolt(d, primary, light, shadow):
    polygon(d,[(70,24),(50,58),(64,56),(45,96),(81,58),
               (65,60),(86,27)],primary,light,3.5)
    line(d,[(78,23),(86,27),(85,39)],shadow,2)


def blade(d, primary, light, shadow):
    polygon(d,[(90,25),(79,60),(65,75),(57,67),(70,51)],
            light,primary,2.5)
    polygon(d,[(73,51),(89,25),(78,59),(64,74)],primary)
    line(d,[(46,77),(65,59)],shadow,7)
    line(d,[(39,69),(52,82)],light,5)
    ellipse(d,(32,80,44,92),fill=shadow,outline=light,width=2.4)


def moon(d, primary, light, shadow):
    ellipse(d,(38,25,94,92),primary,light,3)
    ellipse(d,(52,18,105,77),shadow,None)
    star(d,84,74,10,4,4,light,primary)


def sun(d, primary, light, shadow):
    ellipse(d,(44,44,84,84),primary,light,3)
    for i in range(12):
        a=i*math.pi/6
        r0=29;r1=38 if i%2 else 43
        line(d,[(64+math.cos(a)*r0,64+math.sin(a)*r0),
                (64+math.cos(a)*r1,64+math.sin(a)*r1)],light,3 if i%2==0 else 2)


def time(d, primary, light, shadow):
    polygon(d,[(42,25),(86,25),(78,42),(69,55),(69,69),
               (82,87),(86,99),(42,99),(47,84),(58,70),
               (58,56),(47,43)],shadow,primary,3)
    line(d,[(45,28),(83,28)],light,3)
    line(d,[(48,95),(81,95)],light,3)
    polygon(d,[(55,43),(73,43),(65,56)],primary)
    polygon(d,[(64,74),(77,89),(51,89)],primary)
    line(d,[(65,54),(64,75)],light,1.5)


def eye(d, primary, light, shadow):
    polygon(d,[(24,64),(45,45),(64,39),(84,46),(104,64),
               (84,82),(64,88),(44,83)],shadow,primary,3)
    ellipse(d,(49,48,79,79),primary,light,3)
    ellipse(d,(58,55,71,72),shadow)
    ellipse(d,(60,57,64,61),light)


def hand(d, primary, light, shadow):
    polygon(d,[(47,96),(41,66),(35,49),(40,45),(50,58),
               (50,31),(57,29),(61,56),(63,24),(71,24),
               (72,55),(76,29),(83,31),(82,60),
               (90,43),(96,48),(88,70),(78,94)],
            shadow,primary,3)
    line(d,[(55,71),(71,71),(78,62)],light,2.4)


def chain(d, primary, light, shadow):
    for a,b in [((30,32,65,64),0),((61,61,97,96),1)]:
        arc(d,a,38,320,primary,7)
        arc(d,a,38,320,light,2)


def wolf(d, primary, light, shadow):
    polygon(d,[(27,87),(39,29),(52,43),(65,40),(79,27),
               (101,87),(83,99),(65,89),(47,99)],
            shadow,primary,3)
    polygon(d,[(43,57),(64,49),(86,58),(78,84),(64,92),(50,84)],
            primary)
    ellipse(d,(48,61,57,70),light)
    ellipse(d,(72,61,81,70),light)
    polygon(d,[(60,75),(68,75),(64,81)],shadow)


def bird(d, primary, light, shadow):
    polygon(d,[(28,62),(43,41),(58,52),(64,39),(70,53),
               (85,41),(101,62),(81,64),(66,82),(64,96),
               (58,82),(46,70)],primary,light,3)
    polygon(d,[(56,68),(64,61),(71,68),(65,78)],shadow)
    polygon(d,[(62,55),(76,53),(66,61)],light)


def snake(d, primary, light, shadow):
    arc(d,(35,33,97,103),35,300,primary,8)
    arc(d,(39,38,91,99),26,289,light,2)
    polygon(d,[(37,48),(48,34),(63,46),(51,57)],primary,light,3)
    ellipse(d,(45,43,49,47),shadow)
    line(d,[(40,50),(32,55),(30,60)],light,2)


def spider(d, primary, light, shadow):
    ellipse(d,(49,53,79,88),shadow,primary,4)
    ellipse(d,(55,39,74,62),primary,light,2)
    for side in (-1,1):
        for j in range(4):
            y=51+j*9
            line(d,[(64+side*12,y),(64+side*(24+j*2),y-13+j*6),
                    (64+side*(30+j*3),y-8+j*7)],primary,3.5)
    ellipse(d,(59,44,63,48),light)
    ellipse(d,(67,44,71,48),light)


def portal(d, primary, light, shadow):
    ellipse(d,(30,27,98,102),shadow,primary,5)
    ellipse(d,(41,37,87,91),primary,light,3)
    ellipse(d,(51,47,77,82),shadow)
    star(d,64,63,16,6,5,light,primary)


def cloud(d, primary, light, shadow):
    ellipse(d,(27,56,60,87),primary,light,2)
    ellipse(d,(42,37,78,78),primary,light,2)
    ellipse(d,(66,52,100,87),primary,light,2)
    polygon(d,[(39,70),(89,70),(85,88),(43,88)],primary)
    for x in (46,64,82):
        line(d,[(x,93),(x-5,103)],light,2.5)


def wave(d, primary, light, shadow):
    polygon(d,[(27,77),(43,56),(56,52),(67,62),(75,43),
               (94,43),(102,61),(89,83),(66,92),(44,90)],
            primary,light,3)
    arc(d,(35,51,90,89),195,343,shadow,5)
    line(d,[(40,82),(65,85),(89,74)],light,2)


def flower(d, primary, light, shadow):
    for i in range(7):
        petal(d,64,64,i*math.tau/7,37,11,primary,light)
    ellipse(d,(53,53,75,75),shadow,light,3)
    star(d,64,64,10,5,5,light,primary)


def map_icon(d, primary, light, shadow):
    polygon(d,[(27,35),(47,28),(71,37),(100,29),
               (100,86),(76,95),(52,85),(27,93)],
            shadow,primary,3)
    line(d,[(49,30),(52,85)],light,2)
    line(d,[(73,38),(76,94)],light,2)
    star(d,75,58,16,6,4,light,primary)


def hammer(d, primary, light, shadow):
    polygon(d,[(42,30),(79,30),(87,39),(87,57),
               (73,57),(73,48),(49,48),(49,57),(34,57),(34,38)],
            primary,light,3)
    polygon(d,[(58,50),(69,50),(71,104),(58,104)],shadow,light,2)


def claw(d, primary, light, shadow):
    for i in range(4):
        x=30+i*18
        polygon(d,[(x-3,83),(x+4,32+i*2),(x+12,37+i*2),(x+7,87)],
                primary,light,2)


def rune(d, primary, light, shadow):
    polygon(d,[(64,25),(92,43),(91,79),(64,102),(36,79),(35,43)],
            shadow,primary,3)
    line(d,[(64,32),(64,93)],light,2.5)
    line(d,[(43,46),(85,82)],light,2)
    line(d,[(85,45),(43,83)],light,2)
    ellipse(d,(55,55,73,73),shadow,light,2)


def mask(d, primary, light, shadow):
    polygon(d,[(35,37),(64,27),(92,37),(86,83),(64,99),(42,83)],
            shadow,primary,3)
    polygon(d,[(42,59),(55,54),(60,66),(48,67)],light)
    polygon(d,[(68,66),(74,54),(87,59),(80,67)],light)
    arc(d,(50,68,79,87),10,170,light,3)


def wings(d, primary, light, shadow):
    for side in (-1,1):
        polygon(d,[(64,66),(64+side*11,46),(64+side*39,34),
                   (64+side*27,60),(64+side*40,67),
                   (64+side*16,84)],
                primary,light,3)
        line(d,[(64,66),(64+side*26,51)],light,2)
    star(d,64,65,11,5,4,shadow,light)


def pick_motif(entry):
    hint=(entry["icon_silhouette"]+" "+entry["id"]).lower()
    priority=[
        (("wolf","hunting hound"),"wolf"),(("serpent","viper","snake"),"snake"),
        (("spider","web"),"spider"),(("bird","flock","feather","wing","phoenix"),"bird"),
        (("talisman","scroll"),"rune"),(("map","compass"),"map"),
        (("hand","palm"),"hand"),(("claw","fang"),"claw"),
        (("sword","blade","sickle","spear"),"blade"),(("hammer","fist"),"hammer"),
        (("hourglass","clock","time"),"time"),(("moon","crescent"),"moon"),
        (("sun","dawn","flare"),"sun"),(("iris","eye"),"eye"),
        (("lotus","flower","blossom","bloom"),"flower"),
        (("leaf","seed","tree","branch","wheat","sprout","vine","briar"),"leaf"),
        (("crystal","gem","prism","icicle","shard","glass"),"crystal"),
        (("lightning","thunder","electric","bolt"),"bolt"),
        (("flame","fire","ember","lava","ash","comet"),"flame"),
        (("wave","tidal","water","ocean","well","ripple"),"wave"),
        (("mist","cloud","fog","vapor","nimbus"),"cloud"),
        (("shield","ward","armor","bastion","gate","barrier","fortress","prison"),"shield"),
        (("mirror","mask","echo","twin","glamour"),"mask"),
        (("thread","chain","linked","weave","knot","tether"),"chain"),
        (("portal","doorway","worldstep","teleport","slip"),"portal"),
        (("star","celestial","stellar"),"star"),
        (("rune","seal","glyph","sigil","mark","beacon"),"rune"),
    ]
    for keys,motif in priority:
        if any(k in hint for k in keys):
            return motif
    return {"Attack":"star","Defense":"shield","Healing":"flower",
            "Mobility":"portal","Summoning":"wings","Control":"rune",
            "Utility":"eye"}[entry["primary_category"]]


def draw_motif(d, motif, primary, light, shadow, seed):
    if motif=="shield": shield(d,primary,light,shadow)
    elif motif=="leaf": leaf(d,primary,light,shadow)
    elif motif=="flame": flame(d,primary,light,shadow)
    elif motif=="bolt": bolt(d,primary,light,shadow)
    elif motif=="blade": blade(d,primary,light,shadow)
    elif motif=="moon": moon(d,primary,light,shadow)
    elif motif=="sun": sun(d,primary,light,shadow)
    elif motif=="time": time(d,primary,light,shadow)
    elif motif=="eye": eye(d,primary,light,shadow)
    elif motif=="hand": hand(d,primary,light,shadow)
    elif motif=="chain": chain(d,primary,light,shadow)
    elif motif=="wolf": wolf(d,primary,light,shadow)
    elif motif=="bird": bird(d,primary,light,shadow)
    elif motif=="snake": snake(d,primary,light,shadow)
    elif motif=="spider": spider(d,primary,light,shadow)
    elif motif=="portal": portal(d,primary,light,shadow)
    elif motif=="cloud": cloud(d,primary,light,shadow)
    elif motif=="wave": wave(d,primary,light,shadow)
    elif motif=="flower": flower(d,primary,light,shadow)
    elif motif=="map": map_icon(d,primary,light,shadow)
    elif motif=="hammer": hammer(d,primary,light,shadow)
    elif motif=="claw": claw(d,primary,light,shadow)
    elif motif=="rune": rune(d,primary,light,shadow)
    elif motif=="mask": mask(d,primary,light,shadow)
    elif motif=="wings": wings(d,primary,light,shadow)
    else:
        count=5+seed%3
        star(d,64,63,37,16,count,primary,light)
        ellipse(d,(56,56,72,72),shadow,light,2)
    # Distinct, deterministic secondary mark supplies extra differentiation
    # between powers sharing the same motif.
    accent=seed%5
    if accent==0:
        star(d,94,31,11,4,4,light,primary)
    elif accent==1:
        ellipse(d,(26,31,43,48),shadow,light,2)
        line(d,[(30,40),(39,40)],light,1.8)
    elif accent==2:
        line(d,[(25,28),(37,32),(25,36)],light,2.2)
    elif accent==3:
        ellipse(d,(89,85,105,101),primary,light,1.8)
    else:
        line(d,[(29,96),(41,89),(47,97)],light,2)


def render(entry):
    digest=hashlib.sha256(entry["id"].encode("utf8")).digest()
    seed=int.from_bytes(digest[:4],"big")
    primary=color(entry["palette"]["primary"])
    highlight=color(entry["palette"]["highlight"])
    shadow=color(entry["palette"]["shadow"])
    body=Image.new("RGBA",(SIZE,SIZE),(0,0,0,0))
    bd=ImageDraw.Draw(body,"RGBA")
    # A dark, partially transparent medallion retains legibility in either UI theme.
    ellipse(bd,(12,12,116,116),fill=(12,16,32,213),outline=(*shadow[:3],225),width=3)
    ellipse(bd,(17,17,111,111),outline=(*primary[:3],185),width=2)
    # Fine scale rings and position markers establish a consistent aesthetic.
    offset=seed%18
    arc(bd,(18,18,110,110),16+offset,100+offset,(*highlight[:3],190),2.3)
    arc(bd,(18,18,110,110),190+offset,250+offset,(*primary[:3],200),2.1)
    for j in range(4):
        a=(j*math.pi/2)+(seed%17)*math.pi/180
        x=64+51*math.cos(a);y=64+51*math.sin(a)
        ellipse(bd,(x-2.3,y-2.3,x+2.3,y+2.3),(*highlight[:3],185))
    motif=pick_motif(entry)
    # Colored bloom behind the icon silhouette.
    halo=Image.new("RGBA",(SIZE,SIZE),(0,0,0,0))
    hd=ImageDraw.Draw(halo,"RGBA")
    ellipse(hd,(37,34,91,91),(*primary[:3],100))
    halo=halo.filter(ImageFilter.GaussianBlur(9*SCALE))
    body=Image.alpha_composite(body,halo)
    subject=Image.new("RGBA",(SIZE,SIZE),(0,0,0,0))
    draw_motif(ImageDraw.Draw(subject,"RGBA"),motif,primary,highlight,shadow,seed)
    glow=subject.filter(ImageFilter.GaussianBlur(3*SCALE))
    glow.putalpha(glow.getchannel("A").point(lambda v: int(v*0.46)))
    body=Image.alpha_composite(body,glow)
    body=Image.alpha_composite(body,subject)
    # The tiny number of highlights on the ring depends on this exact power id.
    accents=ImageDraw.Draw(body,"RGBA")
    for j in range(2+(seed%3)):
        a=(seed%360 + j*(80+seed%21))*math.pi/180
        radius=44+(j%2)*6
        x=64+math.cos(a)*radius;y=64+math.sin(a)*radius
        ellipse(accents,(x-1.8,y-1.8,x+1.8,y+1.8),(*highlight[:3],210))
    img=body.resize((128,128),Image.Resampling.LANCZOS)
    assert img.mode=="RGBA"
    return img


def main():
    visual=json.loads(DATA.read_text(encoding="utf-8"))
    entries=visual["abilities"]
    assert len(entries)==100 and len({e["id"] for e in entries})==100
    OUT.mkdir(parents=True,exist_ok=True)
    hashes=set()
    for e in entries:
        target=OUT/(e["id"]+".png")
        im=render(e)
        im.save(target,optimize=True)
        assert target.is_file() and target.stat().st_size>1500, target
        digest=hashlib.sha256(target.read_bytes()).hexdigest()
        assert digest not in hashes, "Duplicate icon art"
        hashes.add(digest)
    # Generate no unreferenced images: stale exported files are removed.
    expected={e["id"]+".png" for e in entries}
    for p in OUT.glob("*.png"):
        if p.name not in expected:
            p.unlink()
    print(f"PASS: generated {len(hashes)} distinct transparent 128x128 icons in {OUT.relative_to(ROOT)}")


if __name__=="__main__":
    main()
