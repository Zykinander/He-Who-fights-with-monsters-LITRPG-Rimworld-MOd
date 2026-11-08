# Essencebound — Awakening Stone Design Sheet v0.5

**Target:** RimWorld 1.7 · **Stone count:** 30 · **Status:** provisional design + XML/C# source only (uncompiled, untested).

This document is a design sheet, not a list of canon abilities. **The three ability slots per essence and twelve total per pawn remain unchanged.**

## Provenance

- `Listed / known rarity` = name and stated rarity appear in the community wiki; not independently book-verified.
- `Listed / tier assigned` = name appears, but wiki says rarity unknown; the tier below is **our mod choice**.
- `Custom / unverified` = name does not appear in the cited stone roster; created or retained for our mod, not claimed as canon.

Stones influence **ability families** rather than guaranteeing a particular ability. Higher rarity should mean scarcity and thematic specificity, not automatic greater combat power.

## Full roster

### Common (10)

| Stone · theme | Description | Imprint tags (3 examples) | Preferred ability types | Status |
|---|---|---|---|---|
| **Fire** · Ember / heat | A warm ember crystal that flickers when held near an awakened essence. | Assault, Aura, Control | Ranged flame spells; heat zones; burning retaliation | Listed / known rarity |
| **Shield** · Protection | A faceted blue stone with a faint outline of a sheltering barrier. | Ward, Counter, Recovery | Temporary barriers; damage reduction; protective countereffects | Listed / known rarity |
| **Magus** · Spellcasting | A violet prism threaded with fine runic filaments. | Spell, Ritual, Control | Direct spells; ritual utility; spell amplification | Listed / known rarity |
| **Plant** · Growth / vitality | A moss-green pebble with branching veins beneath its surface. | Growth, Recovery, Familiar | Regeneration; root control; botanical companions | Listed / known rarity |
| **Water** · Flow / restoration | A glassy blue stone whose inner currents never settle. | Recovery, Control, Ward | Healing flow; water shaping; protective currents | Listed / tier assigned |
| **Wind** · Air / motion | A pale crystal that hums with the suggestion of moving air. | Mobility, Reach, Surge | Movement bursts; knockback gusts; extended reach | Listed / tier assigned |
| **Stone** · Earth / endurance | A rough slate gem with a heavy, unyielding core. | Ward, Counter, Growth | Stone skin; cover creation; ground control | Custom / unverified |
| **Fang** · Predator / precision | An ivory-colored shard with a dark line through its point. | Assault, Familiar, Precision | Piercing attacks; predator senses; animal companions | Custom / unverified |
| **Light** · Illumination / support | A milky crystal carrying a steady lantern-like glow. | Ward, Recovery, Exploration | Illumination; cleansing; ally protection | Custom / unverified |
| **Shadow** · Concealment | A smoke-black stone that seems to dim the space around it. | Veil, Mobility, Control | Stealth; shadow relocation; concealment fields | Custom / unverified |

### Uncommon (8)

| Stone · theme | Description | Imprint tags (3 examples) | Preferred ability types | Status |
|---|---|---|---|---|
| **Focus** · Clarity / mastery | A clear turquoise gem with a perfectly still center. | Precision, Control, Spell | Accuracy; concentrated casting; maintained effects | Listed / known rarity |
| **Reach** · Distance / projection | A sky-blue crystal whose markings stretch toward its tip. | Reach, Mobility, Precision | Long-range abilities; extended targeting; quick repositioning | Listed / known rarity |
| **Preparation** · Planning / setup | A layered amber stone that looks like a folded map. | Preparation, Ward, Control | Traps; advance wards; tactical setup buffs | Listed / known rarity |
| **Surge** · Burst / momentum | A bright orange shard that pulses in irregular waves. | Surge, Assault, Mobility | Short speed boosts; sudden force; burst casting | Listed / known rarity |
| **Hunt** · Tracking / pursuit | A green-brown stone marked with a faint tracking spiral. | Exploration, Precision, Familiar | Tracking; target marking; companion aid | Listed / tier assigned |
| **Grace** · Agility / balance | A smooth silver-blue stone that feels almost weightless. | Mobility, Recovery, Counter | Dodge buffs; safe repositioning; graceful counterattacks | Custom / unverified |
| **Blood** · Vitality / exchange | A dark crimson gem with slow-moving threads at its heart. | Recovery, Assault, Control | Life transfer; sustained recovery; weakening afflictions | Custom / unverified |
| **Mist** · Obscurity / defense | A cloudy opal whose interior is difficult to focus on. | Veil, Ward, Control | Smoke screens; area denial; evasion fields | Custom / unverified |

### Rare (6)

| Stone · theme | Description | Imprint tags (3 examples) | Preferred ability types | Status |
|---|---|---|---|---|
| **Adventure** · Exploration / adaptability | A sea-green stone that catches glints like far-off horizons. | Adventure, Exploration, Mobility | Exploration utility; emergency traversal; situational adaptation | Listed / known rarity |
| **Judgement** · Verdict / discernment | A pale gold stone whose inner markings resemble scales. | Judgement, Ward, Control | Target marking; condition-based counters; protective verdicts | Listed / known rarity |
| **Chain** · Binding / coordination | A linked metallic crystal that softly chimes when turned. | Control, Counter, Reach | Binding fields; tethering; linked ally effects | Custom / unverified |
| **Mirror** · Reflection / deception | A silvery gem that reflects shapes a heartbeat late. | Counter, Veil, Ritual | Reflected damage; decoys; counterspells | Custom / unverified |
| **Ash** · Aftermath / renewal | A grey-black crystal with tiny rose-red embers beneath the dust. | Aura, Control, Recovery | Lingering ash clouds; endurance; recovery after hardship | Custom / unverified |
| **Tide** · Rhythm / currents | A blue-green stone patterned by overlapping circles. | Control, Surge, Recovery | Wave control; ebb-and-flow movement; restorative pulses | Custom / unverified |

### Epic (4)

| Stone · theme | Description | Imprint tags (3 examples) | Preferred ability types | Status |
|---|---|---|---|---|
| **Stars** · Astral / veiling | A midnight stone scattered with moving points of white light. | Stars, Veil, Spell | Celestial concealment; astral effects; unusual magic | Listed / known rarity |
| **Tempest** · Storm / upheaval | An indigo crystal with bright threads that flash through its core. | Surge, Aura, Reach | Storm fields; chain effects; high-area disruption | Custom / unverified |
| **Sovereign** · Command / dominion | A regal amethyst stone ringed by a tiny gold halo. | Ward, Aura, Control | Leadership auras; command marks; area protection | Custom / unverified |
| **Eclipse** · Twilight / duality | A two-tone gem split between warm gold and dark violet. | Veil, Spell, Counter | Light-shadow mixing; counter-magic; stealth wards | Custom / unverified |

### Legendary (2)

| Stone · theme | Description | Imprint tags (3 examples) | Preferred ability types | Status |
|---|---|---|---|---|
| **Reaper** · Reaper / finality | An enigmatic purple-black stone cold to the touch. | Reaper, Control, Counter | Offensive utility; control afflictions; anti-healing pressure | Listed / known rarity |
| **Phoenix** · Rekindling / rebirth | A glowing gold-red stone whose internal flame never goes out. | Recovery, Aura, Familiar | Recovery from defeat; protective flame; phoenix-like familiar | Custom / unverified |

## Implementation contract

1. A pawn may absorb up to **3 normal essences** and receive at most **1 confluence** from a predetermined triple.
2. A pawn may awaken **3 slots per essence**, for **12 total**. No awakening overwrites an existing imprint.
3. Each stone is consumed on successful use and assigns one currently unused imprint tag to a selected essence.
4. `favored_essences_mod_design` is a **bias** list, not an eligibility gate; incompatible or exhausted choices should fail without consuming the stone.
5. `preferred_ability_types_mod_design` is descriptive metadata for later ability pools. It is **not** a list of implemented powers.
6. Preserve the two unresolved confluence conflicts rather than guessing an outcome.
7. v0.5 extends the v0.3 source definitions without making claims of playable powers or a successful RimWorld build.

## Artwork

- The original 12 stone icons from v0.3 are preserved.
- All 18 added stones now have unique faceted 128×128 transparent PNG artwork and editable SVG source.

## Sources and caveats

- Community roster for stone names/rarities: https://he-who-fights-with-monsters.fandom.com/wiki/Awakening_Stones
- Example-specific lore detail: https://he-who-fights-with-monsters.fandom.com/wiki/Awakening_Stone_of_the_Magus
- Example-specific lore detail: https://he-who-fights-with-monsters.fandom.com/wiki/Awakening_Stone_of_the_Reaper
- All stone descriptions and gameplay suggestions here are original mod design text, not passages from the novels.
