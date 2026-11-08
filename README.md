# Essencebound v0.09 — universal pawn-targeting powers

**Development release v0.09 | RimWorld compatibility: 1.7 | Early prototype**

## New in v0.09

Any **pawn-targeted** essence power can be aimed at **any living pawn**: colonist, allied, neutral, or hostile. This includes accidentally or intentionally harming an ally or helping an enemy. There is no automatic friend-or-foe protection for targeted spells.

Targeted powers: Ember Bolt, Frost Shard, Storm Spark, Verdant Mend, Guardian Ward, Rallying Surge, and Purifying Touch.

The five **self-cast** powers remain unchanged: Stone Skin, Swift Step, Mist Veil, Keen Sight, and Force Pulse. Force Pulse still automatically affects nearby **hostile pawns**, not selected targets.

Range, line of sight, cooldowns, empty-target rejection and same-map checks remain. Targeting a pawn with no healable injuries or no relevant removable debuff fails safely without spending cooldown.

## Current prototype content

- 125 normal essences, 101 confluence essences; 366 fixed-confluence recipe rows (two conflicting triples intentionally unresolved).
- Three normal essences + one confluence; 3 power slots each = 12 maximum.
- Absorbing a normal essence and gaining a confluence both grant an innate starter ability.
- 30 awakening stones with distinct artwork and 12 reusable ability effect templates.

## Versioned test downloads

GitHub Actions automatically names **every new build** using the development version, RimWorld target, workflow run number, and rerun attempt.

- Example downloaded file: `Essencebound-v0.09-RW1.7-build7-attempt1.zip`
- Inside: `Essencebound-v0.09-RW1.7-build7-attempt1-mod.zip`
- After extraction, the folder installed into RimWorld is still called `Essencebound`.

Starting another build (or rerunning a job) produces a new name. Re-downloading an old build does not. This prevents accidentally testing the wrong ZIP while keeping the mod's package ID stable.

## Install and test

After a successful GitHub Actions build on `main`, download `Essencebound-1.7-compiled`, extract the enclosed mod, and replace the existing `Mods/Essencebound/` folder. Use a disposable test colony and back up any existing saves first.

Test with two colonists and an enemy on the same map:
1. Cast **Ember Bolt** on an enemy, then an allied pawn (expect damage to both).
2. Cast **Verdant Mend** on an injured ally, then an injured enemy (expect healing to both).
3. Cast **Guardian Ward** on an ally, then an enemy (expect its timed protective effect to apply to both).
4. Select a distant pawn outside ability range, or one behind blocked line of sight (expect a rejection and no cooldown).
5. Check that **Stone Skin** still casts instantly on its owner, without a targeting cursor.
6. Check that **Force Pulse** still affects nearby hostiles only.
7. Save/reload and confirm unlocked essence powers remain intact.

## Known limitations
This is a source-defined and compiled prototype, not yet fully verified with v0.09 in-game tests. Spells still lack projectile VFX; the pawn essence profile is a temporary diagnostic popup. Friendly fire and enemy healing are **intended** under these new targeting rules. All existing stone rarity, confluence and source-provenance caveats remain.

The mod's development version is **v0.09**; the supported RimWorld game version remains **1.7**.
