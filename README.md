# Essencebound v0.08 — RimWorld 1.7 early prototype

**Development version v0.08 | Target game RimWorld 1.7 | Experimental / test saves only**

## Current features
- 125 normal essence types and 101 confluence essence types.
- 366 recipe rows; two conflicting confluence triples remain deliberately unresolved.
- Three normal essence slots + one confluence, with three ability slots per essence (12 maximum).
- Absorbing an essence grants one **innate starter power** automatically; awakening stones unlock up to two more.
- 30 awakening stones with individual item artwork; 12 reusable prototype gameplay effects.
- Pawns retain unlocked abilities and cooldown information through saving.

## v0.08 fixes
- Fixed a RimWorld 1.7 configuration warning affecting **all 30 awakening stones**: `stackLimit > 1` together with `drawGUIOverlay = false`. The shared abstract stone item now enables `drawGUIOverlay`, preserving stack sizes of 10 while displaying their quantity.
- Updated the in-game mod name from the outdated v0.6 label to **v0.08**.
- Added a regression check for the stack overlay rule and synchronized development version metadata.
- This release intentionally does **not** modify unrelated base-game, DLC, or third-party errors observed in `Player.log`.

## Build and install
GitHub Actions on `main` uses .NET 10, runs structural tests, compiles `Essencebound.Core.dll`, and publishes `Essencebound-1.7-compiled` as an artifact. Source branches and pull requests do not automatically publish an installable DLL; merge after review, then download the successful workflow artifact.

1. Download the latest successful GitHub Actions artifact from `main`.
2. Extract `Essencebound_1.7_COMPILED.zip`, then install the enclosed `Essencebound` folder into RimWorld's `Mods` directory.
3. Remove/replace the older installation first to avoid conflicting definitions.
4. Test on a throwaway colony and inspect the startup log and pawn essence profile.

## Known limitations
- Not all game effects have been tested after the v0.07 gameplay fixes. A successful compilation is not proof of in-game correctness.
- The profile interface is a basic diagnostic dialog; a visual redesign is planned.
- Spells lack custom projectile visual effects and rank scaling.
- Automatic starter mappings are *mod gameplay defaults*, not claimed canon abilities.
- Other games/DLC mods or local game files may still produce unrelated errors.

## Versioning
Early releases use `v0.01`, `v0.02`, … `v0.08`, `v0.09`, `v0.10`, and so on. `v1.0` is reserved for a more feature-complete, stable release. RimWorld `1.7` is the target game compatibility, not our development milestone.
