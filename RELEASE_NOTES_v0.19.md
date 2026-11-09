# Essencebound v0.19 — All 100 powers milestone

**Thirty additional playable mechanics, directly from 70 to 100.**

- Register the final 30 abilities with unique stable power IDs, XML definitions, valid awakening aspects and starter assignments.
- Mark all 100 tracked designs as `playable` in `Data/ability_roster_100.csv`. Remove the former v0.20 target and set `current_playable=100`, `remaining_to_implement=0`.
- Add 20 supporting timed/triggered hediffs (53 total), a saveable reaction system and a saveable periodic corruption effect.
- Implement new spawnable familiar and flock summons, linked defense, reactive counterstrike, one-hit emergency protection, conditional healing, attack combos and curse reflection.
- Extend saved map fields for hostile draining, moving blade orbits, out-of-combat sanctuaries, gravity pulling, slowing webs and stillness, and completed channeled teleports.
- Add spatial/pawn casting routes for all 30 effects. Ability progression continues to depend on actual damage, recovery, active control or meaningful utility.
- Preserve all 70 existing power IDs, Essencebound ranks, Gold/Diamond damage resistance and save data.
- Expand regression tests for 100 XML/JSON/CSV entries, all effect handlers, 53 statuses and no unimplemented roster records.

## Caution

Automated checks verify the C# compilation and structural data, but this milestone is **not fully playtested**. Temporary creatures still use vanilla visual placeholders. Reused awakening stone sprites are not unique ability art.

## PR dependency

This is a stacked PR based on v0.18 PR #13. Merge v0.18 first, then change v0.19's PR base to `main` before its eventual merge. Both should remain drafts while runtime testing is incomplete.
