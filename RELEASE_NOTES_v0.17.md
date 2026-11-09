# Essencebound v0.17 — first major step toward 100 playable powers

- Expand active catalog from 24 to **50** distinct playable ability definitions.
- Design and register **100 total named abilities** in `Data/ability_roster_100.csv`: 50 playable now and 50 clearly labelled as planned (20 targeted for v0.18, 20 for v0.19, 10 for v0.20).
- Add seven effect families: `areaHeal`, `chainHeal`, `areaRegen`, `purgeHeal`, `damageAreaDebuff`, `leechPulse`, `execute`.
- Add 11 new Essencebound timed buff/debuff definitions (25 total).
- Reassign 26 more themed essence starter abilities for **new absorptions**, while retaining previous powers, save IDs, cooldowns, ranks, and awakened slots.
- Update cleansing systems to recognize all new debuffs, so existing cleansing abilities are not left behind.
- Keep universal rank damage mitigation and existing 2–5 awakening-slot restrictions intact.
- Regenerate `Data/essence_powers.csv` from the canonical 50-power JSON catalog, replacing the stale 12-row prototype.
- Add XML/JSON/CSV and roadmap integrity tests, unique-ID assertions, status and aspect checks, and coverage for each new effect handler.

## QA
Automated tests and RimWorld 1.7 compilation required. In-game validation needed for group healing, regeneration save/load, duration-based debuffs, multi-target line of sight, and compatibility with existing saves.

## Merge dependency
This draft branches from `essencebound-v0.16-ability-expansion`. Merge v0.16 PR #11 **before** this v0.17 PR; change the v0.17 PR base to `main` after PR #11 is merged.
