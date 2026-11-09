# Essencebound v0.13 — Astral tears, magic density and manifestation loot

**Development version v0.13 | RimWorld 1.7 | Experimental, use test saves**

## The new gameplay loop

A map now has a **saved ambient magic density** (some biomes influence the initial roll). Every in-game day, player-home maps with colonists may develop an astral tear. A tear stays visible for about 15 game seconds before it resolves:

1. **Magical condensation:** The tear collapses into a randomly selected normal essence crystal, which an eligible pawn can absorb.
2. **Monster manifestation:** The tear releases one or more hostile magical monsters. The stronger the local magic density, the greater the chance and danger.

In this first version, manifested monsters use vanilla RimWorld creatures as **visual placeholders**, not yet custom-made HWFWM magical monster sprites or abilities.

| Density | Current creature placeholder | Approximate behavior |
|---|---|---|
| Low | Megascarab | One manhunter |
| Medium | Warg | One manhunter |
| High (78%+) | Thrumbo | Two manhunters |

Monster chance grows from about 15% at zero intensity to 80% at maximum intensity. The density values are random per map and modified by Mod Settings, not by real-world geography.

## Astral Loot power

Each Essencebound pawn temporarily has a **universal Astral Loot** gizmo. Select the ability to open a menu listing eligible **marked monster corpses within 10 cells and line of sight**. Select one to extract its essence. Alternatively choose **Target a corpse on the map** and click its tile; this works even if the game treats the corpse as a cell instead of an item. The power consumes the remains and produces one random normal essence crystal, with a 60-second game cooldown. Ordinary animals cannot be looted by this power, nor can a manifested monster be looted twice.

**Loot troubleshooting:** If no remains appear in the menu, move the pawn closer, check line of sight and confirm the target came from a tear rather than a normally spawned animal. A failed map-target attempt now provides a specific reason. Only a fully dead pawn with a spawned corpse can be extracted.

This is a **prototype utility power**, not yet an awakened ability occupying one of the 2–5 slots. Future releases can introduce distinct looting powers from essences or awakening stones with their own rarity and effects.

## Mod Settings

In **Options → Mod Settings → Essencebound**:

- **Astral tears**: enabled/disabled.
- **Monsters from tears**: enabled/disabled; when off, every tear crystallizes an essence.
- **Manifestation frequency**: off, rare, normal, frequent.
- **Map magic density**: low, normal or high multiplier.

Automatic tears roll once per in-game day and may take multiple days to occur, even at high frequency. Only one tear is allowed to remain open on a map at a time.

## Testing

With **Development Mode** enabled, select a colonist who has absorbed an essence. Use **DEV: Astral tear** to immediately open a test tear near the pawn; this bypasses the event-frequency roll, not the density-based outcome.

1. Confirm the tear appears on the map, shows a density percentage, and persists for roughly 900 ticks.
2. Verify it leaves a crystal or releases a creature. Use the density and monster toggles to test both cases.
3. Defeat a manifested monster in a disposable colony, select an essence user, activate **Astral Loot**, and choose its remains from the nearby list. Also test the **Target a corpse on the map** fallback.
4. Verify an essence crystal appears, the corpse disappears, and the 60-second cooldown starts.
5. Confirm targeting an ordinary corpse cannot yield an essence.
6. Save/reload during an open tear and verify its timer and density persist.
7. Inspect the error log for issues and share any exceptions.

**Warning:** High-density encounters intentionally can summon dangerous creatures. Test with development mode and a backup rather than an important colony.

## Deferred work

Astral tears currently use an enlarged existing magical-stone sprite as a temporary portal marker; custom portal animation, purpose-built monsters, unique drops/rarity weighting, quest integration, specialized looting powers and magic-density environmental dynamics belong in later versions. All existing rank, affinity and configurable slot mechanics remain.

GitHub Actions names each compiled test download with its version and build number; the installed folder stays `Essencebound`.
