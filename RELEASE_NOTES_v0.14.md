# Essencebound v0.14 — colony-driven ambient magic density

- Replace random-per-map magic density with a **deterministic, live strength-based** value that reflects colony wealth, population, combat competence and the current rank/sublevel of Essencebound essences.
- Strength weights: 9% starter background, 23% wealth, 14% population, 12% combat, 42% essence strength. Each normalized contribution is capped individually.
- Newly absorbed essences add modest strength; training through Iron 0–9 to Diamond adds more. Multiple essence users contribute.
- Preserve the existing Low/Normal/High density setting as a multiplier (0.65×, 1×, 1.45×), independent of the strength calculation.
- Apply calculated density to the next manifestation-event roll and to newly opened tears. Existing open tears hold their saved opening density and remain unaffected.
- Preserve the next scheduled event-check tick in existing map saves. The obsolete v0.13 `ebAmbientDensity` value is ignored, not rerolled.
- Add a live density and strength-factor display in the mod settings window while on a map.
- Keep current creature placeholders, Astral Loot, confluence logic and numbered-rank progression unchanged.
- Add source and numeric monotonicity tests to protect against reversing density progression.

## Validation
GitHub Actions structural tests and C# compilation must pass. In-game RimWorld 1.7 tests still required for density display, intensity snapshots, event rate and save compatibility.
