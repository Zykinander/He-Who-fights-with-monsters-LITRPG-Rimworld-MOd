# Essencebound Codex — v0.22 development

The colony has a **save-backed discovery journal**. Knowledge is shared across player-owned maps, persists in the save, and is not tied to a single colonist.

## Opening the Codex

- Select a colonist who has absorbed an essence and click the **Essence Codex** gizmo.
- Or open that colonist's **Essence profile** and click **Open Codex**.
- Before any colonist gains an essence, use **Options > Mod settings > Essencebound > Open Essencebound Codex** while a game is loaded.

## Five tabs

| Tab | Contents | How to discover |
|---|---|---|
| **Essences** | All 125 normal essence types, descriptions, attributes and innate starters | See a crystal on a visible home-map tile or absorb one |
| **Confluences** | 101 confluence outputs, plus journal entries listing each discovered triple and result | Complete a three-different-essence set |
| **Stones** | 30 stone types, rarity, possible awakening aspects, favored essences (undiscovered names masked) | See an item on a visible home map or awaken a power |
| **Abilities** | The 100 powers, unlocked descriptions, categories, tags and cooldowns | Gain an innate power or use a stone |
| **Bestiary** | Species ranks, essence/stone chances, and observed astral ranks | Encounter a creature or its remains on a visible home map |

Unknown entries are represented only as a hidden-entry count marked ???, rather than exposing secret names. Search filters discovered entries, not the entire definition database. Known recipes show **Catalogued recipe**, **Mod-generated synthesis**, or **Disputed catalog recipe**. Distinct combinations producing the same confluence are kept as separate recipes.

## Discovery and saves

The journal is stored through a RimWorld GameComponent and Scribe_Collections, not global settings. Discoveries happen immediately on successful absorption or awakening and through a regular 600-tick scan of colonists and unfogged items and creatures on player home maps. Seeing a stone or crystal does not consume it.

For existing saves, the Codex backfills from current colonists' essences, known recipes, unlocked abilities and used stones, plus visible items and animals. Knowledge of already consumed items and recipes possessed only by deceased or missing colonists cannot always be recovered retroactively. Newly discovered entries persist across normal saves.

The Codex doesn't change damage, creature loot probabilities, awakening behavior, or confluence outcomes.

## In-game QA

1. Open from the gizmo, profile, or mod settings.
2. Verify unknown names stay masked and search does not reveal them.
3. Encounter two copies of one essence crystal; only one Codex entry should unlock.
4. Absorb an unlisted three-essence combination; check the saved confluence recipe and generated label.
5. Use a stone to unlock a power; verify Stone and Ability tabs update, then save/reload.
6. Encounter a squirrel, warg, thrumbo and astral manifestation; verify the bestiary rank and loot odds.
7. Load an older colony save, verify backfill; load a different save and verify knowledge remains separate.

Automated structural checks and RimWorld 1.7 C# compilation run in GitHub Actions. Full in-game validation is still required.
