# Essencebound v0.19 — All 100 planned abilities now implemented

**RimWorld 1.7 | Harmony required | Experimental: use a backup or test save.**

## v0.23 development: Distinct astral monster archetypes

Astral tears now produce five recognizable monster types according to their existing density-based rank roll: **Glassmote Scarab (Iron), Emberfang Stalker (Bronze), Duskweaver (Silver), Stormhide Colossus (Gold), and Prismatic Sovereign (Diamond)**. Each now uses its own simplified, transparent creature art (256px directional textures generated in the mod build), a timed signature magical power, a persisted archetype identifier, and its own discoverable Codex entry. North/south views are temporary perspective adaptations until separately illustrated. Their loot remains rare for essence crystals and more generous for awakening stones; a successfully rolled essence can favor the monster's associated essences. Use **DEV: Astral monsters** from an essence user's gizmos to test each one with developer mode enabled. See [monster bestiary and QA notes](MONSTER_BESTIARY_V023.md).

## v0.22 development: Colony Essencebound Codex

A five-tab discovery encyclopedia now tracks **essences, confluences and discovered recipes, awakening stones, powers, and a creature bestiary**. Discoveries are saved separately for each colony game; hidden entries stay masked until the player finds or unlocks them. Open it from an essence user's **Essence Codex** gizmo, their Essence profile, or the Essencebound mod settings. See [the Codex guide](CODEX_GUIDE.md) for discovery rules, legacy-save backfill, and QA.

## v0.21 development: every unique triple forms a confluence

Three **different** normal essences now always grant a fourth, playable confluence essence. The 364 catalogued combinations are preserved (including deterministic handling of two disputed triples); other combinations choose a deterministic, theme-informed result from the 101 existing confluences. The third-essence absorption menu previews the outcome, and older saves missing a confluence can recover theirs when loaded. Generated syntheses are original mod content, not verified novel recipes. See [confluence rules](CONFLUENCE_RULES.md) for the exact behavior and validation details.


We skipped the intermediate 90-power milestone. The active XML/C# catalog now contains **100 distinct castable essence powers**, compared with 70 in v0.18. Every original entry in `Data/ability_roster_100.csv` is marked **playable**; none remain merely planned. Every power has a valid effect handler, a usable existing icon, aspect-compatible awakening selection and a saved power identifier. This is an **automated-build-tested milestone**, not a guarantee that every mechanic has been exercised in the game.

## Ability categories, essence associations and tags

All **100 playable powers** have a main gameplay category (Attack, Defense, Healing, Mobility, Summoning, Control or Utility), a **primary favored essence**, and **multiple overlapping tags** (for example, Attack + Healing + Drain). See [the tagged ability catalog](ABILITY_CATALOG_BY_ESSENCE_AND_TAGS.md) to browse by category and primary essence.

The same tags appear in the ability's in-game tooltip. These are informational labels only: existing awakening aspect compatibility, other favored essences, power IDs and save files are unchanged.

## Unique art concepts for all 100 abilities

[The complete visual design guide](ABILITY_VISUAL_DESIGN_GUIDE.md) now specifies a **unique icon silhouette, color palette, cast animation, effect animation and sound direction for every one of the 100 powers**. These designs now drive the generated 128×128 game icons and corresponding animated fleck sigils in the GitHub Actions mod ZIP. These are first-pass procedural illustrations, not hand-painted artwork, unique frame-by-frame animations or custom sound files.

## First-pass generated icon assets

The build now renders 100 distinct, transparent 128x128 ability icons and 100 matching transparent spell sigils from the versioned visual designs. The generated PNGs are included in the installable build ZIP, not committed as binaries. These are first-pass procedural game icons, not 100 individually hand-painted illustrations. Spell casting and result VFX use the matching power sigils.

## All thirty new powers

| Ability | Mechanic and progression |
|---|---|
| Oasis Well | Stationary area heals actual ally injuries only while no enemies threaten the well |
| Bone Familiar | Temporarily summons a player-allied armored insect companion that trains while assisting in combat |
| Mistwalk | Timed mobility and dodge boon |
| Mirror Step | Swaps positions between caster and a standing allied pawn; earns travel-based work |
| Web Weaver | Temporary web field slows visible hostile pawns crossing it |
| Sonic Lance | High-armor-penetration sonic attack, training from effective damage |
| Sunward Flare | Harms nearby hostiles and lifts several magical curses from nearby allies |
| Moonshadow | Cast in low light to gain an evasive boon |
| Hunger Field | Moving damaging field around caster; converts some effective damage into healing |
| Shatter Armor | Damage plus a temporary penalty to sharp and blunt armor |
| Harvest Blessing | Short-range support boon improving plant harvest and work effectiveness |
| Weave Ward | Links the caster with an ally, sharing some incoming pawn-instigated damage |
| Rune Beacon | Stationary protective ward area around a selected ground cell |
| Iron Reprisal | Temporary close-range counterstance that retaliates upon being attacked |
| Serpent Coil | Restrains a hostile pawn and disrupts its shooting |
| Dawn Rebirth | Saved conditional healing trigger when caster's health falls below a threshold |
| Adept Convergence | Timed exchange of a small amount of Speed affinity for Power affinity |
| Fey Glamour | Hostile accuracy and work disruption through a timed curse |
| Nimbus Guard | Mobile armor and evasiveness ward on a nonhostile pawn |
| Blade Orbit | Orbiting strikes centered on the moving caster |
| Gravity Well | Pulls hostiles toward a chosen center and applies slowing essence |
| Corrupting Seed | Persists on a hostile and repeatedly deals real damage while weakening armor |
| Talisman Seal | Stores one defensive trigger to absorb part of the next pawn attack |
| Pact of Unity | Boosts work speed and dodging for allies nearby |
| Phoenix Ashes | A saved one-time healing boon triggered by a serious injury crisis |
| Worldstep | Start a timed channel toward an explored unoccupied destination, then teleport |
| Feral Claw | Melee-range attacks that increase in force as successful combo hits accumulate |
| Ward Echo | Periodically removes hostile Essencebound curses and can return them to their original caster |
| Sands of Stillness | Places a persistent hostile-only slowing field |
| Tempest Flock | Summons several temporary friendly birds that can draw enemy attention |

The new powers reuse some existing visual assets and familiar vanilla creatures; custom VFX, unique creature art and canonical lore verification are **not** part of this milestone.

## Existing mechanics retained

The v0.19 implementation preserves all 70 previous power IDs and saves; existing awakened powers and levels are not rewritten. Thirty matching essences grant the new abilities as **starter imprints only on future absorption**; awakening stones can also select them through their compatible aspects.

Rank synchronization and Iron/Bronze/Silver/Gold/Diamond progression remain in place. In particular, the rank defense curve is unchanged: **Gold takes 80% less damage from unranked attackers, and Diamond takes 99% less** from those attackers. All attack effects still use RimWorld's pawn damage pipeline for rank resistance.

## In-game acceptance checklist

1. Use a backed-up colony, put Harmony ahead of Essencebound, and check `Player.log` for XML and Harmony exceptions.
2. Try **Mirror Step** with an ally and **Worldstep** on clear, explored map cells; confirm range, failure handling and safe destination rules.
3. Cast **Moonshadow** in brightly lit and dark locations; only dim conditions should allow casting.
4. Test **Oasis Well** on wounded allies, first out of combat, then near enemies; XP should come from actual healing.
5. Try **Bone Familiar** and **Tempest Flock** with **Sovereign Command**; summons should disappear on expiry and after saving/reloading.
6. Cast **Web Weaver**, **Sands of Stillness** and **Gravity Well** on hostiles near walls; confirm slowing/pulling and no movement into occupied or blocked cells.
7. Test **Talisman Seal**, **Iron Reprisal**, **Weave Ward** and **Ward Echo** against pawn attacks and control spells. Verify no recursive defensive damage loop.
8. Compare successive **Feral Claw** attacks and verify a stopped combo resets; test **Corrupting Seed** training from effective periodic damage.
9. Confirm **Dawn Rebirth** and **Phoenix Ashes** activate only under their conditional thresholds, with actual injury recovery and no free XP.
10. Save/reload during active timed spells, fields, familiars and triggers. Verify existing characters still retain all previously learned powers and ranks.
11. Confirm lower-rank damage mitigation on the new powers, especially Gold and Diamond defenders.

**Automated QA** verifies 100 unique IDs and active XML defs, 53 status defs, the awakening/catalog mappings and a RimWorld 1.7 C# compilation. It does not substitute for playtesting every ability.

## Milestone status

| Version | Playable powers |
|---|---:|
| v0.16 | 24 |
| v0.17 | 50 |
| v0.18 | 70 |
| **v0.19** | **100** |

No v0.20 release is needed to satisfy the requested minimum count. Future updates can focus on in-game bug fixes, distinct animations, improved balancing and making different essence combinations feel more unique.
