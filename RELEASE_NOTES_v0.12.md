# Essencebound v0.12 — rank subdivisions, damage XP fixes and per-level attribute bonuses

## Damage XP issue
The v0.11 executor allowed allies and enemies as targets but awarded XP only when `HostileTo(caster)` was true. This excluded allied and neutral targets from training. The damage routine now credits actual damage from any pawn target; if a special damage handler reports zero total damage but creates injuries, injury severity delta is used. Zero-effect hits still earn no work.

Casting now reports earned progression and current ability level. When a tier is stalled, feedback reports the lock.

## Ranked levels and progression gates
- Each ability gains 0–9 numbered levels within Iron, Bronze, Silver and Gold before entering the next major tier at level 0.
- Thresholds per sublevel are 4, 12, 36 and 108 respectively (unchanged total work relative to v0.11).
- An essence's numbered level is the minimum level across all of its *required unlocked* ability slots.
- A major-rank promotion requires all required slots to reach that tier.
- Any ability's next-tier training pauses if another currently owned essence has not reached its tier; unowned slots do not block.
- New save field `abilitySublevel` migrates old `rankWork` without resetting learned powers or affinity rolls.

## Smooth attribute bonuses
Each sublevel grants one-tenth of the gap to the next doubled rank bonus. At Iron level N, the affinity bonus is the Iron 0 amount multiplied by `1 + N/10`. Thus Iron 0 Power +4.0%, Iron 1 +4.4%, Iron 9 +7.6%, and Bronze 0 +8.0%. The approach repeats toward Silver (x4), Gold (x8), and Diamond (x16).

The affinity-bonus computation reads the owning essence's *lowest required ability level*, not its highest ability level or the pawn's overall major rank. Existing Speed cooldown reduction cap of 80% is retained.

## Validation
Automated source structure/data checks and C# compilation required. Gameplay validation remains necessary, especially damage work feedback, sublevel migration and cross-essence synchronization.
