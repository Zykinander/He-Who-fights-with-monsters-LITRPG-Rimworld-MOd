# Essencebound v0.6 — first playable power library (source prototype)

**RimWorld 1.7 · 30 awakening stones · 12 reusable powers · 3 normal essences + 1 predetermined confluence · 3 slots each**

The 30 stones now select a playable power **and** a thematic imprint when absorbed. The initial set is **original mod gameplay design**, not a claim about abilities from the books. An essence has only three ability slots (maximum twelve if its confluence is resolved). No duplicate power is assigned *within* the same essence.

## First 12 powers

| Power | Target | Effect | Cooldown |
|---|---|---|---|
| Ember Bolt | Hostile | Heat spell, 6 damage | 7.5 s |
| Frost Shard | Hostile | 4 damage and movement slow | 9 s |
| Storm Spark | Hostile | 5 damage and short static debuff | 9.5 s |
| Verdant Mend | Ally | Heal 6 points of a nonpermanent injury | 15 s |
| Guardian Ward | Ally | Temporary armor bonus | 14 s |
| Stone Skin | Self | Higher armor, slightly less movement | 15 s |
| Swift Step | Self | Temporary movement speed bonus | 12 s |
| Mist Veil | Self | Temporary melee evasion bonus (not invisibility) | 15 s |
| Keen Sight | Self | Temporary accuracy bonus | 15 s |
| Rallying Surge | Ally | Temporary speed and work-speed bonus | 15 s |
| Purifying Touch | Ally | Remove a custom chill/static status | 13 s |
| Force Pulse | Self | Small radius effect against nearby hostile pawns | 12 s |

**Preview limits:** Instantly applies effects through pawn gizmos; no animation, ranged projectile, cast-time, AI use, mana resource, rank scaling, crafted stones, custom spell VFX, or truly distinct canonical spells. Ranged effects require line of sight and range. Friendly powers require friendly pawns; hostile spells only target hostiles. Cooldowns persist in saves. Buff timers save with the target pawn.

## Install vs build

This archive contains **source and XML, NOT a ready-to-play DLL**. RimWorld does not compile C# source when loading a mod. You must compile with the 1.7 references. See `BUILD.md`. **No RimWorld 1.7 compile or in-game run was possible in this environment.** Keep this experimental mod out of important saves.

To try the XML-only artwork/items without compiling, use the separate v0.6 XML catalog package instead of the C# package; the two packages define the same stones and must not be loaded simultaneously.

### Compatibility notes

- v0.3–v0.5 awakening imprints are matched deterministically to an appropriate new power on save load. Existing essence/stone/aspect history is preserved; old cooldowns default to zero. If no compatible power exists, the imprint remains unresolved rather than being erased.
- The community confluence database still contains 366 rows; 2 conflicting triples remain flagged and are not auto-resolved.
- The database and game power definitions are separated. Changing the 12 ability templates never needs changing 366 recipes.
- Rarity/source-status fields for stones remain unchanged; most gameplay abilities are original designs rather than confirmed canonical powers.

For definitions see `Data/essence_powers.json`, `Data/essence_powers.csv`, `1.7/Defs/PowerDefs`, and `1.7/Defs/HediffDefs/EssencePowerEffects.xml`.

## Manual smoke-test checklist

1. Compile with .NET 10 and RimWorld 1.7 references; check zero XML/C# errors on startup.
2. In dev mode, spawn a crystal. Select adjacent colonist and right-click to absorb Fire essence.
3. Spawn Fire/Shield/Focus stones. Right-click a stone to awaken Fire. Confirm the new power gizmo appears.
4. On a test map with both allied and hostile pawns, try a targeted power. Check that friend/foe restrictions, range and line of sight work.
5. Test a positive buff and verify its temporary stat change fades.
6. Test a hostile Frost Shard, then Purifying Touch on a friendly pawn who has a custom debuff from a test source.
7. Save and reload during a cooldown and a timed buff. Confirm both persist and eventually expire.
8. Awaken three powers within one essence, attempt a fourth, and verify it is rejected.
9. Absorb a known three-essence set, ensure one confluence appears, then verify it has three additional slots.
10. Load an old v0.5 test save; confirm existing imprints are backfilled into active powers.

If the game reports any build or load errors, share the game's log and we can fix exact 1.7 API differences.
