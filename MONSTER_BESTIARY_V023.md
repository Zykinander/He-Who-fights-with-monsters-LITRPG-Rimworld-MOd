# Essencebound v0.23 — First magical monster wave

The first wave of custom astral monsters replaces vanilla animal placeholders **when an astral tear successfully spawns its assigned custom PawnKind**. Existing unmarked wildlife keeps its inherent species rank. Normal tears that condense directly into an essence crystal remain unchanged.

## New magical monster archetypes

| Assigned rank | Monster | Underlying physiology (art is original) | Signature magic | Associated essence preferences |
| --- | --- | --- | --- | --- |
| Iron | Glassmote Scarab | Megascarab — cyan crystalline scarab sprite | Dazzling static condition on a nearby colonist | Glass, Crystal, Lightning |
| Bronze | Emberfang Stalker | Warg — charcoal-and-ember stalker sprite | Short-ranged ember pulse | Fire, Wolf, Smoke |
| Silver | Duskweaver | Megaspider — indigo astral spider sprite | Astral web slowing a nearby colonist | Spider, Thread, Dark |
| Gold | Stormhide Colossus | Rhinoceros — storm-plated colossus sprite | Storm surge and static condition | Lightning, Earth, Might |
| Diamond | Prismatic Sovereign | Thrumbo — ivory prismatic sovereign sprite | Prismatic pulse plus healing of nearby injured astral manifestations | Light, Star, Mirror |

The creatures now use **five original simplified, transparent monster illustrations**, not recolored vanilla sprites. The source images are in `ArtSource/Monsters/`; `Tools/generate_monster_textures.py` produces 20 directional 256×256 textures in `1.7/Textures/Essencebound/Monsters/` during the GitHub Actions build. The `PawnKindDef.lifeStages.bodyGraphicData` `texPath` values point to `Essencebound/Monsters/<Name>` with `Graphic_Multi`. Colors are not tinted again. East artwork is original, west is mirrored; north and south are **provisional perspective adaptations**, not custom front/back paintings yet. Vanilla species retain their own textures. The magical creatures still share the relevant vanilla physiology and corpse behavior.

## Spawn and rank rules

- A tear uses the original density-dependent five-tier rank roll.
- Each rank currently picks one new archetype. It cannot change rank through species inheritance: the saved `Hediff_AstralManifestation.rankIndex` overrides the underlying animal race rank.
- Monsters retain manhunter behavior and the existing tear group sizes.
- Their astral manifestation hediff saves the archetype ID and power cooldown; the selected monster's Health tab tooltip names its special power.
- Monsters from **older saves** have no archetype ID, retain their previous rank, and do not spontaneously acquire a new ability.
- If the new PawnKindDef is absent or pawn generation fails, generation falls back to vanilla kinds to keep astral tears functional.
- Pulse targets are nearby, visible, still-active player colonists. Each creature uses a separate interval; the Diamond sovereign can heal nearby injured manifestations.

## Loot and the Codex

The normal drop chances are unchanged from previous versions:

| Rank | Essence crystal | Awakening stone |
| --- | ---: | ---: |
| Iron | 1% | 8% |
| Bronze | 2% | 16% |
| Silver | 4% | 26% |
| Gold | 7% | 40% |
| Diamond | 12% | 60% |

Like other manifestations, these monsters receive the existing 20% relative drop bonus and allow one reduced additional Astral Loot attempt. If an essence crystal roll succeeds, there is a **45% chance to choose a themed essence** from the monster's three favored essences rather than from the full normal essence catalog. This only changes the type of the successful drop, not its rarity or quantity. Themed drops don't guarantee a specific essence or a confluence recipe.

The colony Codex Bestiary now records first encounters with each magical monster under its **own name**, as well as the generic base species. It records discovered archetypes and astral ranks in the save and shows a signature power, rank, drop odds, and favorite essences whose names have been discovered.

## Developer quick-test menu

Enable RimWorld developer mode, select a colonist with an Essencebound essence, and use the **DEV: Astral monsters** command. The menu lists all five archetypes with their assigned ranks. Choose one to spawn it on a standable, visible tile 5–10 cells away, without altering colony magic density. Spawned test monsters still have their normal magical archetype, saved rank, loot, abilities, and Codex discovery behavior. This menu is only visible in developer mode.

## In-game QA

1. Trigger astral tears at low and high density and confirm each of the five tiers uses the appropriate monster name and visible rank.
2. Verify the original transparent textures appear at normal gameplay zoom in all four directions, without pink missing-graphic squares or an unwanted second color tint. The north/south views are temporary adaptations. In the health tab, the astral rank must override the vanilla species.
3. Encounter all five: verify one signature power activates periodically for each, without unintended extra effects outside line-of-sight range.
4. Save/reload during a creature's pulse cooldown. Its archetype and cooldown should survive; no duplicate hediffs or repeated rewards.
5. Defeat a custom manifestation and confirm the existing rare essence and more-common awakening-stone rolls, plus Astral Loot.
6. Open the Codex Bestiary; the encountered monster appears as a unique discovered entry. Unknown monsters remain hidden. Reload and confirm discovery persists.
7. Test the fallback on an isolated development copy with a temporarily unavailable custom kind; an ordinary animal should still appear.
8. Run existing 100-power regression checks, as monster behavior must not change player progression.

GitHub Actions verifies the original source image checksums, generates all 20 directional PNGs, checks their alpha channels and paths, and compiles the RimWorld 1.7 DLL. This is not a substitute for in-game QA, particularly artwork path and combat-effect checks.
