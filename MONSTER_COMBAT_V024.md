# Essencebound v0.24 — Magical Monster Combat

This milestone gives the five original astral monster types distinct **magical combat tactics**, warning visuals and fair counterplay. Their ordinary movement/melee AI still comes from RimWorld's manhunter animal behavior. We have not replaced all vanilla job-giver movement AI.

## Tactical profiles

| Monster | Rank | Targeting and power | Area | Warning | Spell interval |
| --- | --- | --- | ---: | ---: | ---: |
| Glassmote Scarab | Iron | Marks the closest visible colonist and disrupts colonists near the marked cell with essence static | 2 cells | 75 ticks | 480 ticks |
| Emberfang Stalker | Bronze | Prioritizes a wounded visible colonist and explodes a compact ember blast at their **original** position | 1.6 cells | 65 ticks | 390 ticks |
| Duskweaver | Silver | Targets the farthest visible colonist, applying a longer-lasting web slowdown to anyone caught inside | 2.3 cells | 110 ticks | 360 ticks |
| Stormhide Colossus | Gold | Charges a broad blunt-damage and static shockwave around **its own position at cast start** | 4 cells | 105 ticks | 330 ticks |
| Prismatic Sovereign | Diamond | Sends a prismatic impact at a nearby colonist while healing the worst nearby astral wound and protecting up to three nearby astral allies | 2 cells (attack), 7 cells (support) | 135 ticks | 300 ticks |

**The strike lands at the warned cell, not where its selected target has moved**. The first warning puff ring appears as the monster begins the cast; colored puffs recur during the wind-up, with a larger burst on impact. The five colors are cyan, ember orange, violet, storm blue and prismatic lavender. These are inexpensive vanilla flecks, not bespoke animated spell sprites.

- **Line of sight:** The monster must have line of sight when it starts its spell. At impact, any affected player colonist must also have line of sight from the marked impact cell.
- **No guaranteed hit:** If the colonist leaves the marked area, the spell misses them. An attack can catch multiple colonists standing together.
- **No wildlife damage:** These spell bursts only affect active player colonists, not pets, neutral animals or other astral monsters. The sovereign's beneficial effects apply only to creatures carrying the astral manifestation marker.
- **Damage rules:** Damage still passes the actual monster pawn as `DamageInfo` instigator, preserving the rank-resistance system.
- **Statuses:** The implementation reuses the existing essence static, webbed and guardian ward hediffs, with temporary durations. Creature spells cannot generate colonist ability-training work.
- **Support logic:** A Sovereign can charge a support-only pulse without a nearby colonist if a marked astral ally has a nonpermanent injury.

## Persistence and interruptions

An astral monster saves the selected impact cell, pulse cooldown and charge completion tick on its `Hediff_AstralManifestation`. On save/load, a pending strike maintains its fixed target rather than picking a new one; an overdue charge is given a fresh warning on load. Being downed cancels any in-progress charged attack. Monsters from older saves without an archetype ID remain unchanged.

## Bestiary information

The Codex Bestiary now lists discovered magical monsters' tactics, attack ranges, impact radius, warning periods and cooldowns alongside their existing rank and loot chances. Unknown monster entries stay hidden.

## In-game QA

Use **DEV: Astral monsters** from an Essencebound colonist while developer mode is enabled.

1. Spawn each type and confirm its unique sprite, name, rank hediff, and appropriate colored warning ring.
2. Move the target colonist **out of the ring** during the warning; it should avoid that spell. Remain inside to verify impact/status.
3. Try two colonists together to verify area effects; ensure allied animals, ordinary wildlife, and astral monsters are not accidentally struck.
4. Inspect statuses: Glassmote/Stormhide static, Duskweaver webbed, Sovereign guardian ward; verify the ward expires naturally.
5. Wound an astral ally near a Prismatic Sovereign and observe the healing pulse, including a support-only pulse when no colonist is in casting range.
6. Save and reload during a spell's warning period. No duplicated casts or retargeting. Down the creature during a warning; after recovering it must begin a new charge.
7. Check a lower-rank attacker against a higher-rank monster. The existing rank mitigation should still work, and monster damage should use the same normal damage pipeline.
8. Check the monster's Codex details and ensure essence / stone loot probabilities remain unchanged.

GitHub Actions validates code-level invariants in `Tests/test_magical_monster_combat.py` and compiles the 1.7 DLL. A passed build does **not** establish in-game behavior or balance.
