# Essencebound v0.23 — First magical monster wave

The first wave of custom astral monsters replaces vanilla animal placeholders **when an astral tear successfully spawns its assigned custom PawnKind**. Existing unmarked wildlife keeps its inherent species rank. Normal tears that condense directly into an essence crystal remain unchanged.

## New magical monster archetypes

| Assigned rank | Monster | Vanilla body used in this prototype | Signature magic | Associated essence preferences |
| --- | --- | --- | --- | --- |
| Iron | Glassmote Scarab | Megascarab, cyan crystalline tint | Dazzling static condition on a nearby colonist | Glass, Crystal, Lightning |
| Bronze | Emberfang Stalker | Warg, crimson tint | Short-ranged ember pulse | Fire, Wolf, Smoke |
| Silver | Duskweaver | Megaspider, violet tint | Astral web slowing a nearby colonist | Spider, Thread, Dark |
| Gold | Stormhide Colossus | Rhinoceros, storm-blue tint | Storm surge and static condition | Lightning, Earth, Might |
| Diamond | Prismatic Sovereign | Thrumbo, lavender tint | Prismatic pulse plus healing of nearby injured astral manifestations | Light, Star, Mirror |

The tinted animals are **prototype visuals**, not finished original monster sprite assets. Def files and body graphics are in `1.7/Defs/PawnKindDefs/AstralMonsterKinds.xml`. Monsters still have the ordinary animal race's physiology and corpse behavior, but their unique `PawnKindDef` gives them a separate name and appearance tint.

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

## In-game QA

1. Trigger astral tears at low and high density and confirm each of the five tiers uses the appropriate monster name and visible rank.
2. Verify the tinted prototype textures load rather than appearing as missing graphics. In the health tab, the assigned astral rank must override the vanilla body species.
3. Encounter all five: verify one signature power activates periodically for each, without unintended extra effects outside line-of-sight range.
4. Save/reload during a creature's pulse cooldown. Its archetype and cooldown should survive; no duplicate hediffs or repeated rewards.
5. Defeat a custom manifestation and confirm the existing rare essence and more-common awakening-stone rolls, plus Astral Loot.
6. Open the Codex Bestiary; the encountered monster appears as a unique discovered entry. Unknown monsters remain hidden. Reload and confirm discovery persists.
7. Test the fallback on an isolated development copy with a temporarily unavailable custom kind; an ordinary animal should still appear.
8. Run existing 100-power regression checks, as monster behavior must not change player progression.

GitHub Actions checks the XML/source integration and compiles the RimWorld 1.7 DLL. This is not a substitute for in-game QA, particularly artwork path and combat-effect checks.
