# Essencebound — 100 Unique Ability Visual Designs

**Status: design concepts, not rendered sprites.** This is the art-production blueprint for all 100 v0.19 ability definitions. The existing mod still reuses awakening-stone icons. No PNG art or particle systems are claimed to exist yet.

## Unified art direction

- **Icon target:** transparent 128 × 128 PNG; 64 × 64 and 32 × 32 downscale previews. One dominant silhouette and at most two supporting marks; no text, no tiny faces, no detailed backgrounds.
- **Visual identity:** each ability has its own central shape; related essences may share a color family, but two ability icons must remain distinguishable in grayscale.
- **Colors:** primary is the main glow; highlight is the brightest readable spark; shadow supplies a dark outline for light and dark UI.
- **Motion:** build cast cue, hit or sustained-effect cue independently, with simple additive sprites/meshes and modest particle counts appropriate for RimWorld zoom.
- **Telegraphing:** delayed blasts and damaging fields must mark their affected ground before dealing damage; healing and friendly buffs should read differently from hostile debuffs.
- **Accessibility:** danger cannot be indicated by red alone; use angular spiked silhouettes for hostile effects and circles/leaves/shields for positive effects. Prefer consistent outline and contrast.
- **Performance:** short casts should minimize persistent particles; zones and summons should reuse sprite assets rather than creating independent animated effects every tick.
- **Assets are not wired yet:** future image exports belong under `1.7/Textures/Essencebound/AbilityIcons/<PowerId>.png`; only then change the corresponding XML `iconPath`.
- **Original designs:** color, motifs and audio cues are art direction, not verified canon from the novels. Sound direction is a production note, not a supplied audio file.

## Complete ability visual guide

### Attack (31)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 1 | Ember Bolt | Fire | `#F47738` · `#FFE0A0` | A flaming arrowhead enclosing one coal | A bright orange rune sparks in the caster's palm | A narrow ember streak ends in one compact fire burst |
| 2 | Frost Shard | Ice | `#67D5ED` · `#F0FBFF` | A jagged six-point icicle splitting a snowflake | Three icy facets crystallize above the hand | The shard strikes with a frostflake snap and a thin slowing ring |
| 3 | Storm Spark | Lightning | `#A498FF` · `#FFF0A5` | A forked lightning bolt inside an open circle | Violet sparks gather between three floating nodes | A short zigzag flashes and a static halo clings to the target |
| 12 | Force Pulse | Might | `#D9A578` · `#F7E4AD` | A clenched fist inside three concentric rings | Energy compresses into a small ochre sphere at chest level | One outward circular shockwave crosses nearby hostile tiles |
| 13 | Cinder Nova | Volcano | `#FF6538` · `#FEC46C` | An erupting crater surrounded by a burning sunring | A rotating molten ring builds around the caster | A fiery circular wave expands outward with scattered falling embers |
| 14 | Thunder Chain | Lightning | `#A498FF` · `#FFF0A5` | Three connected lightning nodes forming an angled fork | Three electric beads orbit the casting hand | Lightning branches from the first target toward two nearby hostiles |
| 15 | Blood Siphon | Blood | `#CE4C79` · `#F6B3A5` | Two inward crescents joined by a red droplet | A crimson tether extends toward the target | A red stream returns to the caster and fades into healed wound marks |
| 21 | Corrosive Strike | Blight | `#98C95E` · `#ECF097` | A sword tip dissolving into acidic droplets | Yellow-green liquid light coats the caster's focus | A corrosive splatter leaves cracked armor outlines on the target |
| 22 | Echo Burst | Echo | `#9BA6ED` · `#F3E8FF` | A broken sound ring striking a small central orb | Two violet rings contract toward the chosen target | A main impact produces smaller concentric shock rings around nearby hostiles |
| 25 | Inferno Lance | Fire | `#F47738` · `#FFE0A0` | A needle-thin spear with a flame-shaped spearhead | A sharp golden spear forms horizontally before the hand | A fast straight orange lance leaves a long fading heat streak |
| 26 | Frost Halo | Ice | `#67D5ED` · `#F0FBFF` | A frosted circular crown with four hanging icicles | Cold mist compresses around the caster's feet | A blue-white ring bursts outward with ice dust on enemy tiles |
| 27 | Quakebreaker | Earth | `#AAA49A` · `#F4E4C5` | A hammer fracturing a concentric stone disc | Square stones hover and shake around the casting hand | A main ground crack radiates short secondary cracks toward nearby hostiles |
| 28 | Zephyr Ricochet | Wind | `#84D5BD` · `#FEF8DE` | Three curved wind blades joined by tiny bounce arrows | Three small mint blades spin beside the hand | A blade darts between targets in angled ricochets |
| 29 | Umbral Leech | Dark | `#7169A8` · `#DDC8FF` | A black crescent funnel drawing in a purple droplet | Shadow wisps narrow into a thread at the hand | Violet energy siphons backward from target to caster as small wound marks fade |
| 30 | Viper's Bite | Venom | `#87BE57` · `#E2F2A0` | A coiled viper head bearing two glowing fangs | A green fang outline snaps over the hand | A short fang flash leaves slow-moving green toxin wisps |
| 31 | Shard Volley | Crystal | `#6ED3DB` · `#F6FFFF` | Four diamond shards arranged like a fan of arrows | Prisms assemble from clear fragments around the wrist | Crystalline darts strike in branching bright cyan streaks |
| 32 | Briar Detonation | Plant | `#57B877` · `#D9FFA4` | An exploding thorned seed pod with four outward spikes | A prickled seed brightens above the chosen pawn | Briar vines burst outward and wrap affected enemies briefly |
| 33 | Starfall | Star | `#B1A9F1` · `#FFF0B7` | A falling four-point star with a long comet-tail slash | A tiny star glyph flashes overhead | A vertical violet-gold beam lands and sends out star-dust ripples |
| 34 | Tidal Rebuke | Water | `#449FCA` · `#A1E8F5` | A curling tidal crest inside a shield-shaped wave | Water coils around the caster's knees | A foamy blue arc surges outward across nearby hostile tiles |
| 42 | Soul Harvest | Death | `#8577A3` · `#E6D0E8` | A dark sickle surrounding a returning light seed | A purple circle blooms at the caster's feet | Wisps rise from nearby hostiles and converge on the caster as healing flecks |
| 47 | Blade Tempest | Sword | `#A4B2CB` · `#FFE9AE` | Four swords spinning around an empty central circle | Sword silhouettes orbit the caster for half a turn | A fast outward ring of blade streaks strikes nearby hostiles |
| 48 | Nemesis Strike | Nemesis | `#CE4C79` · `#F6B3A5` | A downward blade passing through a broken target seal | A small red target glyph marks the injured foe | The finisher leaves a sharp descending crimson-white impact slash |
| 51 | Ashen Comet | Phoenix | `#FF6538` · `#FEC46C` | A black-red comet with a phoenix-feather tail | A thin ember ring marks the target's ground position | A delayed blazing comet hits the marked location with a short fireburst |
| 70 | Molten Rain | Volcano | `#FF6538` · `#FEC46C` | Three dripping meteors above a cracked ground tile | A glowing hot zone outline appears on the ground | Repeated molten streaks fall into the marked area |
| 76 | Sonic Lance | Song | `#A4B9E7` · `#FFE4B8` | A narrow tuning fork spear striking a circular note | A pale blue tuning fork oscillates by the caster | A focused straight sound-wave lance lands with tight concentric rings |
| 77 | Sunward Flare | Sun | `#F9BE74` · `#FFF0B7` | A rising sun split by a clean white spear of light | The caster lifts a small golden sun halo | A sharp radial flare damages hostiles and burns away dark curse sigils from allies |
| 79 | Hunger Field | Hunger | `#8577A3` · `#E6D0E8` | A hollow mouth-shaped vortex with inward arrows | Dark ribbons spiral inward at the caster's feet | A moving violet ring draws thin life wisps from nearby hostiles |
| 80 | Shatter Armor | Hammer | `#9AA6B8` · `#FFE2A3` | A hammer smashing an armored breastplate into three pieces | A steel-white crack line forms on the target | An impact fractures a shield outline and leaves chipped armor flecks |
| 90 | Blade Orbit | Sword | `#A4B2CB` · `#FFE9AE` | Three blade tips orbiting an empty crescent center | Three slender sword silhouettes rise around the caster | Moving orbit blades leave repeatable short strike arcs against close hostiles |
| 92 | Corrupting Seed | Corrupt | `#8D6FA8` · `#DAF09A` | A black seed splitting a cracked armored leaf | A violet seed lodges over the target | Slow pulses shed sickly light and thin armor-crack flecks |
| 97 | Feral Claw | Claw | `#CE4C79` · `#F6B3A5` | Four claw slashes arranged as ascending steps | A reddish claw aura charges over the caster's hands | Each successive close hit draws another brighter claw streak |

### Defense (18)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 5 | Guardian Ward | Shield | `#69BACF` · `#FFDCAB` | A tall shield with a tiny central sun | A shield outline traces itself around the hand | A translucent single-pawn shield shell pulses once |
| 6 | Stone Skin | Earth | `#AAA49A` · `#F4E4C5` | A human silhouette covered in hexagonal stone plates | Dust motes spiral inward onto the caster | Chunky rock plates click into place and a heavy footprint ripple fades |
| 8 | Mist Veil | Dark | `#7169A8` · `#DDC8FF` | A half-hidden eye behind three curled fog strands | Shadow vapor rises from the shoulders | A translucent edge haze shimmers around the moving pawn |
| 18 | Grove Shelter | Tree | `#57B877` · `#D9FFA4` | A broad tree canopy forming an overhead shield | Root-shaped lines spread outward from the caster | Individual leafy protective shells appear on nearby allies |
| 35 | Iron Bastion | Iron | `#9AA6B8` · `#FFE2A3` | An iron tower gate crossed by two heavy plates | Iron rivet points light up across the caster's shoulders | Thick segmented armor panels lock into position |
| 36 | Mirror Guard | Mirror | `#B3D5E6` · `#F6E9FF` | A faceted upright mirror displaying a shifted silhouette | Silver-blue facets rotate above the caster | Brief duplicate outline slips sideways as a mirrored armor gleam appears |
| 50 | Glass Ward | Glass | `#AFE0E8` · `#FFFFFF` | A glass prism shield with a tiny healing droplet | A transparent crystal forms above the target | A wound gleams closed beneath faceted protective panels |
| 52 | Prismatic Deflection | Refracting | `#6ED3DB` · `#F6FFFF` | A triangular prism reflecting one incoming arrow | A rotating rainbow-edged prism appears by the caster | An incoming ranged hit splinters into a returning beam |
| 57 | Chimeric Aspect | Chimera | `#98C95E` · `#ECF097` | A split chimera mask with plated and feathered halves | The central mask rotates to select a side | Near threats evoke bronze armor plates; safety evokes swift green streaks |
| 60 | Tidal Wall | Ocean | `#449FCA` · `#A1E8F5` | Five rising water tiles forming a crenelated line | A blue line sketches itself over the selected cells | Temporary translucent water-brick barriers rise in a row |
| 69 | Guardian Bastion | Guardian | `#69BACF` · `#FFDCAB` | A vaulted dome with a central guardian diamond | A golden diamond rotates above the caster | A stationary translucent shield dome overlays nearby allies |
| 78 | Moonshadow | Moon | `#97A9F0` · `#F6EDFF` | A dark crescent moon hiding a single silver eye | Dim silver particles gather under the caster's hoodline | A faint crescent afterimage appears only in low light |
| 82 | Weave Ward | Weave | `#937ABD` · `#F5D9E4` | Two shield knots joined by a taut silver thread | A thread passes between caster and ally | A slim silver tether flashes as damage is shared |
| 83 | Rune Beacon | Rune | `#75A5D0` · `#FFE8B4` | A triangular stone rune standing inside a shield circle | A rune is stamped onto the selected open tile | A stationary circular ward glyph pulses under nearby allies |
| 84 | Iron Reprisal | Iron | `#9AA6B8` · `#FFE2A3` | A shield backed by an outward-pointing spear | An iron guard plate locks across the caster's arm | On a close hit, a steel counter-spark flashes back at the attacker |
| 89 | Nimbus Guard | Sky | `#87CFE9` · `#FCF5E1` | A cloud shield with one curved wind feather | A blue-cloud ribbon wraps around the selected pawn | The protective cloud tracks the moving pawn with subtle sparks |
| 93 | Talisman Seal | Talisman | `#D0AA76` · `#B4E6DB` | A hanging scroll seal protecting a small shield | A paper-like rune folds around the caster | The seal flares once on the next attack then breaks into gold paper flecks |
| 98 | Ward Echo | Boundary | `#69BACF` · `#FFDCAB` | A shield reflecting a bent curse arrow outward | A pale mirror ring settles around the caster | Hostile curse wisps reverse into outward-traveling blue sigils |

### Healing (16)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 4 | Verdant Mend | Life | `#57B877` · `#D9FFA4` | A split leaf stitched by a glowing thread | A leaf-shaped ribbon circles the caster's fingertips | Soft green stitches close visible wound marks on the target |
| 11 | Purifying Touch | Pure | `#FFF1BF` · `#83E5D0` | An open palm holding a four-point white star | A cream-white sigil unfolds over the palm | A dark status glyph unravels into clear teal flecks |
| 16 | Life Bloom | Life | `#57B877` · `#D9FFA4` | A flower whose petals are small healing crosses | A closed green bud opens over the target | Separate petals glide to different injury spots and dissolve |
| 17 | Renewal Seed | Renewal | `#57B877` · `#D9FFA4` | A small seed with a spiral sprout at its center | A glowing seed descends onto the target | The sprout pulses gently with each delayed healing tick |
| 19 | Purging Wave | Pure | `#FFF1BF` · `#83E5D0` | A breaking chain inside a circular water ripple | A broad pale ring gathers at the caster's feet | A clear wave strips dark debuff flecks from nearby allies |
| 24 | Moonlit Reprieve | Moon | `#97A9F0` · `#F6EDFF` | A crescent moon cradling a small shield and drop | A pale lunar arc crosses the selected pawn | One wound glows shut and a thin moon-shaped shield appears |
| 38 | Verdant Sanctuary | Verdant | `#57B877` · `#D9FFA4` | An open four-leaf canopy over a healing cross | Four leaves unfold around the caster | Green healing threads branch to several nearby wounded allies |
| 39 | Ocean Renewal | Ocean | `#449FCA` · `#A1E8F5` | A wave cradle carrying four small raindrops | Small blue droplets orbit the caster | Each wounded ally receives a slow pulsing wave-ring |
| 40 | Cleansing Ray | Light | `#FFF1BF` · `#83E5D0` | A narrow white ray splitting a broken dark rune | A white line sharpens from the caster's hand | The beam removes dark glyphs and closes a faint injury mark |
| 41 | Mercy Thread | Ministration | `#F9BE74` · `#FFF0B7` | Three linked gold needles threaded with one light ribbon | A golden filament winds around the first target | Healing light stitches from the main target to two nearby allies |
| 53 | Lotus Sanctum | Lotus | `#6CC5B0` · `#F5E7A4` | A lotus blossom enclosing a pool and tiny cross | A pale lotus opens on the ground | Transparent petals rise in a stationary zone and healing pulses ripple outward |
| 59 | Fleshweave | Flesh | `#CE4C79` · `#F6B3A5` | An open palm sewing a branching pink thread | A crimson stitch ribbon stretches from caster to target | Threads mend wounds while the caster's glow visibly dims |
| 65 | Hydra Bloom | Hydra | `#57B877` · `#D9FFA4` | Three sprouting heads rising from one healing stem | Three green buds glow on the caster's hand | Branching regenerative vines curl around multiple wounded allies |
| 71 | Oasis Well | Oasis | `#6CC5B0` · `#F5E7A4` | A small well with lotus leaves and a bright water drop | A turquoise circle traces a stone-well silhouette on open ground | A stationary spring shimmer heals injured allies in slow ripples |
| 86 | Dawn Rebirth | Dawn | `#F9BE74` · `#FFF0B7` | A sunbird rising from a small healing cross | A golden sunrise sigil rests over the caster | When triggered, warm rays pour into wound marks as a quick restorative flare |
| 95 | Phoenix Ashes | Firebird | `#FF6538` · `#FEC46C` | A phoenix feather curling around a healing ember | A single glowing feather lands on the selected ally | When triggered, a gentle ash plume turns into a bright restorative spark |

### Mobility (8)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 7 | Swift Step | Swift | `#76D7C8` · `#F5FFF0` | Two opposing arrowhead boots crossing a wind ribbon | Thin green speed lines collect at the feet | Twin parallel aftertrails follow a short rapid movement |
| 45 | Temporal Haste | Time | `#72CDB9` · `#FCE1A4` | An hourglass crossed by forward chevrons | The hourglass grains reverse and accelerate | Thin clock-tick trails follow the caster's movement |
| 54 | Chrono Anchor | Time | `#72CDB9` · `#FCE1A4` | A clock dial tied to a fixed map pin | A glowing pin stamps below the caster | A later recast folds the pawn into a clock ring and restores the pinned position |
| 58 | Void Slip | Void | `#7169A8` · `#DDC8FF` | A hollow doorway cut through a black crescent | A dark ring briefly compresses at the caster | The pawn vanishes into a pinhole and reappears from a second ring |
| 61 | Lightning Step | Lightning | `#A498FF` · `#FFF0A5` | A zigzag boot striking a circular lightning impact | Electricity gathers around the caster's boots | A brief blink ends with a radial lightning shock on landing |
| 73 | Mistwalk | Cloud | `#9FCDDF` · `#F6FAF6` | A hood-shaped cloud with a fading footpath | Cloud wisps pool around the caster's feet | Thin mist trails soften the pawn outline while the movement and evasion buff lasts |
| 74 | Mirror Step | Mirror | `#B3D5E6` · `#F6E9FF` | Two figures connected through an hourglass-shaped mirror | Matching mirror glyphs flash under two allies | Both figures fracture into reflections and trade positions |
| 96 | Worldstep | Dimension | `#72CDB9` · `#FCE1A4` | A distant doorway framed by a winding star-road | A long spiral clock pathway traces toward the target cell | After channeling, the caster folds into a tunnel of light at the destination |

### Summoning (5)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 55 | Wolfpack Call | Wolf | `#DDD0BC` · `#AFE6CE` | Two wolf heads emerging from one spiral portal | Two spectral pawprints glow near the caster | Temporary wolves emerge from spiraling green-white light |
| 62 | Sovereign Command | Sovereign | `#EFCB77` · `#FFF5D2` | A crown above three directional banners | A gold command rune flares over the caster | Matching tiny crown pips pulse above summoned allies as they receive the order |
| 67 | Echo Twin | Doppelganger | `#9BA6ED` · `#F3E8FF` | Two offset matching masks with one fading edge | A lavender mirrored outline peels away from the caster | A ghostlike decoy silhouette appears with a ripple before settling into its temporary stand-in |
| 72 | Bone Familiar | Bone | `#DDD0BC` · `#AFE6CE` | A small bone-shelled beetle inside a summoning ring | Ivory segmented markings rotate near the caster | A compact summoned insect emerges in pale ghostdust |
| 100 | Tempest Flock | Bird | `#91C4DF` · `#FBE6BC` | Three bird wings crossing a storm spiral | Three pale storm feathers circle the caster | A small flock emerges in windswept light and then fights or distracts nearby foes |

### Control (11)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 20 | Gravity Snare | Vortex | `#75A5D0` · `#FFE8B4` | A weighted sphere caught in four inward arrows | Four blue geometric anchors lock around the target | A compact gravity grid drags downward around the hostile's steps |
| 43 | Rune Bind | Rune | `#75A5D0` · `#FFE8B4` | A square rune stamped over two crossed chains | A bright square seal rotates above the hand | The rune locks around the hostile's ankles as grid lines |
| 44 | Omen Hex | Omen | `#A66BB5` · `#ECD5EC` | A cracked purple eye with a falling star pupil | A shadow-purple sigil unrolls toward the pawn | A floating eye blinks once above the hostile before dissolving into static |
| 49 | Thread Snare | Thread | `#937ABD` · `#F5D9E4` | A single hooked needle drawing a looped cord | Silver strands twist between the caster's fingers | A taut knot snaps around the hostile's legs |
| 64 | Predator Mark | Predatory | `#CE4C79` · `#F6B3A5` | A clawed aiming sigil enclosing a red eye | A crimson triangle briefly aligns over the hostile | A small target emblem tracks the marked pawn |
| 66 | Crystal Prison | Crystal | `#6ED3DB` · `#F6FFFF` | A pawn silhouette inside four giant gemstone bars | Four faceted corner posts blink around a selected foe | A small crystal cage rises and a blue binding grid shimmers inside |
| 75 | Web Weaver | Spider | `#937ABD` · `#F5D9E4` | A central spider knot inside a seven-strand web | A purple web grid draws over a targeted area | Web strands fade in and pulse at slowed enemy feet |
| 85 | Serpent Coil | Serpent | `#87BE57` · `#E2F2A0` | A coiled serpent encircling a boot and crosshair | A green serpentine loop forms around the targeted pawn | The loop tightens around legs while a green aiming sigil distorts |
| 88 | Fey Glamour | Fey | `#AF9BDD` · `#F0D7EA` | A smiling fae mask with three offset eye trails | Pink-violet motes swirl around the hostile's head | False afterimages and wobbling aim lines confuse the enemy |
| 91 | Gravity Well | Vortex | `#75A5D0` · `#FFE8B4` | A dark center orb with four spiraling inward arrows | A circular grid appears over selected ground | Nearby enemies slide inward as lines curve toward the center |
| 99 | Sands of Stillness | Sand | `#DCBB8A` · `#F8E6BE` | A stopped hourglass half buried in rippling dunes | Golden sand forms a level circular boundary | Gentle sand bands thicken around hostile feet within the zone |

### Utility (11)

| # | Ability | Primary essence | Main / highlight | Icon silhouette | Casting cue | Resulting effect |
|---:|---|---|---|---|---|---|
| 9 | Keen Sight | Eye | `#699AF1` · `#FFF3BF` | A round iris with four compass ticks | A blue aperture focuses in front of the caster | One clean crosshair glint blinks over the affected pawn |
| 10 | Rallying Surge | Might | `#D9A578` · `#F7E4AD` | A raised banner topped by two speed chevrons | Amber light sweeps from the caster toward the target | Amber lines flare on the pawn's arms and feet then settle into a warm aura |
| 23 | Spirit Beacon | Mystic | `#699AF1` · `#FFF3BF` | A tall beacon with an eight-ray eye at its peak | A blue pinprick of light rises above the target | A narrow steady pillar highlights the pawn's hands and aim |
| 37 | Dawn Chorus | Dawn | `#F9BE74` · `#FFF0B7` | A rising sun made of five musical notes | Warm orange-gold notes float upward from the caster | Synchronized light rings gently appear under nearby allies |
| 46 | Battlefield Focus | Battlefield | `#699AF1` · `#FFF3BF` | A tactical reticle framing a crossed blade and arrow | An angular aiming grid focuses over the caster | A clean reticle glint pulses at the eyes and weapon |
| 56 | Spectral Hand | Hand | `#699AF1` · `#FFF3BF` | A translucent extended hand lifting a small cube | A ghostly hand outline floats beside the caster | The targeted loose item slides back on a faint tether |
| 63 | Star Map | Star | `#B1A9F1` · `#FFF0B7` | A folded map with a glowing eight-point star | An orbit of map-grid dots forms over the caster | Fog peels back in a soft outward star-grid sweep around the chosen cell |
| 68 | Resonance Field | Resonating | `#A4B9E7` · `#FFE4B8` | Three waveforms crossing a bright central crystal | An indigo sound ring forms beneath the caster | A stationary circular lattice pulses when allied essence attacks occur |
| 81 | Harvest Blessing | Harvest | `#A4C26B` · `#F2D68A` | A sheaf of golden wheat behind a small work hand | Gold grain motes rise around the caster | A wheat-green buff ring appears near allied growers |
| 87 | Adept Convergence | Adept | `#699AF1` · `#FFF3BF` | Two attribute arrows trading positions through a central prism | One blue and one gold rune rotate around the caster | The Power-side rune brightens while the Speed-side dims |
| 94 | Pact of Unity | Unity | `#EFCB77` · `#FFF5D2` | Four hands connected in a bright central knot | Four golden nodes form around the caster | Linked circles pulse briefly below nearby allied pawns |

## Production checklist

- [ ] Render 100 transparent unique icon PNGs following the silhouettes above.
- [ ] Check each at 32 × 32 size in bright and dark UI.
- [ ] Add casting cue and impact/persistent effect to every actual handler.
- [ ] Add hostile-zone danger markers and summon/teleport entry-exit cues.
- [ ] Test light/dark, color accessibility and particle performance in RimWorld 1.7.
- [ ] After the art exists, update XML iconPath and add missing-file validation before packaging.

**No gameplay data changed in this PR.** These are visual plans only; rank mitigation, cast behavior, cooldowns, awakening compatibility and saves remain as before.

