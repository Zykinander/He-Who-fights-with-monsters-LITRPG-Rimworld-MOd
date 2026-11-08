# Essencebound v0.7 — starter powers and Ember Bolt fix

Source-only update; compile with the existing GitHub Actions workflow before installing.

- Fix Ember Bolt's missing damage definition (`Burn` → `Flame`) and report actual damage dealt or resisted.
- Add 226 provisional starter-power mappings across 125 normal and 101 confluence essences, using the existing 12 effect types.
- One innate power automatically occupies a normal ability slot when an essence is absorbed or a confluence forms.
- Preserve legacy awakened slots; grant a missing starter only in an empty slot without duplicating power defs.
- Awakening stones still fill remaining slots. Three abilities per essence; twelve total maximum.
- `Data/essence_starter_abilities.csv` documents all mappings. They are original mod gameplay choices, not canon.

## Test checklist
1. On a fresh test save absorb Fire: Ember Bolt should appear automatically in slot 1.
2. Fire awakening stones should fill remaining empty slots without adding duplicate powers.
3. Cast Ember Bolt on a hostile pawn in range and line of sight; compare feedback against its Health tab.
4. Resolve a confluence: it should start with an innate power too.
5. Back up an existing save and check that its prior stone power remains present after loading.

Known limitations: no flying projectile visual yet, original essence-confluence recipe conflicts remain unresolved, and gameplay code requires an actual GitHub build and in-game test.
