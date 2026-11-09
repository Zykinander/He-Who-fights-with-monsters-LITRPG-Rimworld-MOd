# Essencebound v0.16 — playable ability expansion

## New gameplay
- Total playable power definitions: **24**, up from 12.
- New original mod powers: Cinder Nova, Thunder Chain, Blood Siphon, Life Bloom, Renewal Seed, Grove Shelter, Purging Wave, Gravity Snare, Corrosive Strike, Echo Burst, Spirit Beacon and Moonlit Reprieve.
- Add nine distinct effect handlers: chained damage, splash damage, damage-to-healing drain, healing multiple wounds, regeneration over time, friendly area buffs, nearby cleansing, duration-based hostile debuffs and combined heal + ward.
- Add six new timed-status definitions: regeneration, group ward, gravity snare, armor corrosion, spirit beacon and moonlit ward.
- Regeneration persists duration, progress tick, per-pulse healing amount, original caster and source slot for valid training after reload. Existing buffs retain duration, and hostile-control training requires the affected pawn to remain hostile.
- Damage effects train from actual received damage, healing from actual injury severity repaired, cleansing only from removed statuses, and duration powers while beneficial/control statuses persist.
- New target-dependent powers retain range and line-of-sight validation; area follow-up targets require hostility when dealing unintended splash damage.
- Reassign the starter imprint of 12 matching essences to one of the new powers; existing absorbed powers remain unchanged in save games.
- Awakening Stone aspect system can select all 24 powers without modifying stones or the existing slot cap.
- No changes to Gold/Diamond rank resistance or magic-density scheduling.

## Test coverage
- Structural tests check cross-file XML/JSON concordance, unique names, status classes, target kinds and aspect compatibility.
- New tests validate effect handler presence, work sources, persistence and all 12 starter imprint mappings.
- Automated compilation must succeed, then gameplay must be verified in a test colony.

These abilities are mod-original and should not be represented as canonical names/effects from the source novels.
