# Essencebound v0.08 — log-based fixes

Reviewed the November 8, 2026 RimWorld 1.7 Player.log.

## Confirmed Essencebound issue
RimWorld reported `has stackLimit > 1 but also has drawGUIOverlay = false` for all 30 awakening stones, twice per stone (60 error entries). The issue originates from the `EB_AwakeningStoneItemBase` ThingDef's inherited overlay property. Fix: add `<drawGUIOverlay>true</drawGUIOverlay>` to the shared abstract stone base; keep `<stackLimit>10</stackLimit>`.

## Out of scope (per testing request)
12 other log errors concerned missing game/DLC definitions and textures. No modifications were made to those resources.

## Additional updates
- In-game mod name now shows v0.08 instead of stale v0.6.
- Manifest, quick-start and README use the v0.08 development version.
- XML regression tests verify the base item overlay and version metadata.
- Previous v0.07 gameplay improvements (innate abilities and Ember Bolt) remain in the mod.

## Verification status
Offline structural validation should run in GitHub Actions; a compilation and fresh in-game log are still required to confirm the v0.08 change in RimWorld 1.7.
