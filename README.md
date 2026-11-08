# Essencebound v0.10 — character interface & configurable ability slots

**Development version v0.10 | RimWorld 1.7 | Experimental prototype**

## What changed

- The pawn's **Essence profile** gizmo opens a dedicated, scrollable **four-panel character window** instead of a plain text popup.
- Panels show normal essence slots I, II and III, plus the confluence essence, with locked/empty states.
- Each panel lists awakened abilities with an icon, source (innate or awakening stone), description, and remaining cooldown.
- The header shows the pawn's current Iron/Bronze/Silver/Gold/Diamond rank; rank progression is still a future mechanic.
- **Mod Settings > Essencebound**: click the slot control to cycle **2 → 3 → 4 → 5 → 2** available ability slots **per essence**, globally.
- Default remains **3 slots** per essence, or **12** at a full four-essence set. Minimum is **8** total and maximum **20**.
- In each essence, the innate ability uses a slot; awakening stones can fill the other available slots.
- Increasing the setting creates more empty slots on existing essence users. Decreasing it **does not delete powers** previously earned in higher slots: these are paused (not shown/castable) until you raise the limit again.
- This setting is global for the mod, and applies to all pawns/essences; individual essence-by-essence overrides are not yet implemented.

## Known prototype functionality

125 normal essences, 101 confluences, 366 recipe rows (two conflicting triples unresolvable), 30 awakening stones with custom artwork, 12 reusable magic effect types. Pawn-targeting powers accept allies or enemies; self-cast powers remain self-cast. No changes in the save serialization format for ability slots.

## Testing checklist

1. Build with the GitHub Actions PR workflow (C# and XML checks must pass), then install the new test artifact.
2. Select a pawn with an essence and click **Essence profile**. Verify all four panels and its current ability descriptions appear.
3. Set 5 slots in **Options → Mod Settings → Essencebound** and inspect the same pawn again. Verify five slots per essence panel and no duplicate innate powers.
4. Use awakening stones to fill extra slots. Check that the count and slot cap are respected (maximum 5).
5. Reduce the setting to 2; check that ability gizmos for slots 3-5 disappear, but existing powers remain in the save.
6. Restore 5 and check those powers reappear and retain cooldowns after saving/reloading.
7. Verify two ally/enemy targeted abilities, plus a self-cast ability, continue to work.

## Build artifacts
GitHub Actions produces a uniquely named archive using the mod version and workflow run/attempt numbers, for example `Essencebound-v0.10-RW1.7-build9-attempt1.zip`. The installed folder name remains **Essencebound**.

## Limitations
The prototype still has a fixed set of 12 reusable gameplay power effects, no unique animation or projectile visuals, and no functioning XP/rank progression system yet. A successful C# build does not replace testing the window in RimWorld 1.7.
