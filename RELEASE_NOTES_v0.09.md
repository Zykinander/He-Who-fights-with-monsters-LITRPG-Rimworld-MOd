# Essencebound v0.09 — targeting update

- Pawn-targeted abilities accept any living pawn on the same map, independent of factions.
- Seven powers updated to `<targetKind>pawn</targetKind>`: Ember Bolt, Frost Shard, Storm Spark, Verdant Mend, Guardian Ward, Rallying Surge, and Purifying Touch.
- Updated `Data/essence_powers.json` and XML definitions to match.
- Retained fallback interpretation of old `targetKind=hostile` and `targetKind=ally` as universal pawn targeting in the C# executor.
- Self-cast powers still cast only on the owner, and the untargeted Force Pulse still affects nearby hostile pawns.
- Range, line-of-sight, alive, same-map and cooldown restrictions remain. Self-targeting with a selected-pawn ability is allowed.
- Power gizmo tooltips now say “Any living pawn (ally or enemy)” for targeted abilities.
- Added static regression tests verifying XML/JSON agreement, no faction checks in ValidateTarget, range and LOS checks retained, and targeting cursor availability.
- Changed mod version metadata to v0.09; RimWorld compatibility remains 1.7.

## Expected behaviors / edge cases
- Damage spells can hurt allied pawns; heal and ward spells can benefit enemies, deliberately.
- Targeted healing fails without consuming cooldown if the pawn has no healable injury.
- Targeted cleansing fails without consuming cooldown if the pawn has no relevant Essencebound affliction.
- Targeter only selects spawned living pawns on the same map; powers still require the caster to be alive and on a map.
- No new power slots, ability definitions, or save-schema modifications.

## Validation
GitHub Actions must compile and run structural tests. This update is **not** proof of in-game correctness until user testing confirms targeting both friendly and hostile pawns.
