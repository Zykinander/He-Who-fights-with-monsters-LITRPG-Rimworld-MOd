# Essencebound v0.18 — 70 playable powers

**RimWorld 1.7 | Harmony required | Experimental release, test with backups**

This release adds **20 executable abilities** from our 100-entry roadmap, increasing the playable catalog from 50 to **70**. Thirty additional designs remain planned.

## New techniques

| Ability | Gameplay effect |
|---|---|
| Ashen Comet | Delayed hostile-only impact near a selected enemy |
| Prismatic Deflection | Reduces ranged pawn damage and reflects part back |
| Lotus Sanctum | Stationary area that gradually heals wounded allies |
| Chrono Anchor | Records a location, then returns the caster there when recast |
| Wolfpack Call | Two temporary allied wolf summons |
| Spectral Hand | Pulls a visible loose haulable item closer |
| Chimeric Aspect | Defensive adaptation near hostiles, agility boost otherwise |
| Void Slip | Short-range line-of-sight teleport to open ground |
| Fleshweave | Uses caster fatigue to heal existing injuries |
| Tidal Wall | Places temporary impassable magical barriers |
| Lightning Step | Blink plus a hostile-only area impact |
| Sovereign Command | Commands allied temporary summons to attack an enemy |
| Star Map | Reveals fogged map cells around a selected location |
| Predator Mark | Makes a hostile more vulnerable to same-faction attacks |
| Hydra Bloom | Regeneration applied to several wounded allies |
| Crystal Prison | Temporary surrounding barriers and a slowing debuff |
| Echo Twin | Temporary allied decoy creature |
| Resonance Field | Stationary aura increasing Essencebound damage |
| Guardian Bastion | Stationary defensive field that boosts armor |
| Molten Rain | Persistent area periodically damaging visible hostiles |

### Power mechanics and progression

New abilities can select map cells and loose items, as well as the existing living-pawn targets. Temporary map fields and summoned companions persist relevant state through save/load. Delayed and area attacks award progression from real damage inflicted; healing fields from actual injury severity repaired; mobility and other utility powers from meaningful successful actions.

Some current mechanics are still prototypes: Echo Twin is a temporary animal decoy, summons are vanilla creatures, and temporary barriers use placeholder stone art. Unique animations and monster assets remain future work.

## Compatibility

Existing 50 abilities retain their saved identifiers, slots, rank/sublevel progression, affinity rolls, and cooldowns. Eighteen essence starter mappings have been added or updated for **new absorptions only**. The current 100-power roster consists of **70 playable and 30 planned** abilities.

## Gameplay test priorities

1. Open a fresh test colony with Harmony enabled, and check for errors at startup.
2. Try Void Slip, Lightning Step, and Star Map on map cells; Spectral Hand on loose items.
3. Check the second Chrono Anchor cast returns to the original cell.
4. Save/reload while Lotus Sanctum, Molten Rain, Tidal Wall, or temporary summons are active.
5. Check Prismatic Deflection against ranged attacks without reflecting indefinitely.
6. Summon allied wolves and try Sovereign Command on an enemy.
7. Check Predator Mark benefits allied pawn attacks, and area healing grants XP only for actual recovery.
8. Compare Gold/Diamond lower-rank damage protection with v0.17.
9. Share Player.log if any powers fail to cast, load or display.

Automated tests and C# compilation are not substitutes for in-game verification.

## Next milestones

- **v0.18:** 70 playable
- **v0.19 planned:** 90 playable
- **v0.20 planned:** 100 playable

The tracked roster is in Data/ability_roster_100.csv. All these abilities are original mod-gameplay designs, not verified book-canon power names.
