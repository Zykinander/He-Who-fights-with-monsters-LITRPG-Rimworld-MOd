# Essencebound v0.11 — earned ability ranks and weighted attributes

**RimWorld 1.7 | Early development release v0.11 | Test saves recommended**

## New progression system

Each unlocked power has its own persisted tier and measured work toward the next tier: **Iron → Bronze → Silver → Gold → Diamond**. Progress comes from the *actual outcome*, not merely pressing the cast button.

| Power effect | Earned progression |
|---|---|
| Single-target damage / damage + debuff | Actual damage applied to a hostile pawn |
| Area pulse | Total actual damage applied to nearby hostiles |
| Healing | Actual injury severity healed |
| Support buffs | 1 work per 60 game ticks the status remains active on a living spawned pawn |
| Cleansing | 10 work per actual removed Essencebound chill/static affliction |

The initial tier requirements are 40 work to Bronze, 120 more to Silver, 360 more to Gold, and 1080 more to Diamond. These are mod-original balancing defaults.

**An essence ranks up only when all its required ability slots are unlocked and every one has reached the next rank.** Essences progress separately. The pawn's overall rank is the *lowest* of its owned essence ranks. Unresolved confluences are not counted until they form.

### Dynamic 2–5 ability slots

In **Options → Mod Settings → Essencebound**, choose 2–5 slots per essence (default 3). Each essence remembers the **highest number of slots ever required since progression was introduced**. Raising the slot cap increases the required count; lowering it only pauses higher-index powers and does **not** make the essence rank up early. Increase the cap again to resume those requirements.

## Weighted, random essence attribute affinities

When a pawn gains a normal or confluence essence, it receives one **random** affinity. The same Fire essence can produce Power on one pawn and Spirit or even Recovery on another. The roll happens only once and is **saved in the pawn's hediff**. Older saves acquire one affinity per existing essence when first loaded with v0.11.

Every essence has positive weights for all four attributes; the distribution is themed, not fixed. The 226 editable distributions are in `Data/essence_attribute_weights.csv` and embedded in the corresponding essence XML defs. These are mod-original choices and aren't canonical lore claims.

| Affinity | Benefit for each rank tier of an essence with that affinity |
|---|---|
| Power | +4% essence ability damage |
| Speed | -2.5% essence ability cooldown |
| Spirit | +5% positive essence buff duration |
| Recovery | +6% essence healing strength |

Iron counts as one tier, Bronze as two, and Diamond as five. Bonuses from multiple owned essences of the same attribute stack (capped at +80% combined). These bonuses currently affect Essencebound powers, **not all vanilla pawn stats**; a later pass can integrate melee stats, movement speed, psychic resources, or natural injury recovery.

A few illustrative base weights, in Power / Speed / Spirit / Recovery order:
- **Fire:** 6 / 1 / 5 / 1 (Power and Spirit favored)
- **Life:** 1 / 1 / 3 / 8 (Recovery favored)
- **Swift:** 1 / 8 / 2 / 1 (Speed favored)
- **Might:** 8 / 1 / 2 / 2 (Power favored)
- **Balance:** 3 / 3 / 3 / 3 (equal chances)

## Character profile

The four-panel profile displays each essence's randomly rolled attribute and individual rank. Ability rows show rank, effective-work progress and cooldowns. The header shows current combined Essencebound bonuses.

## In-game test checklist

1. On a temporary colony, absorb the **same essence on two different pawns**; compare their attribute roll (results can match by chance).
2. Save/reload, verify affinity selections remain identical.
3. Cast Ember Bolt on enemies and confirm its damage-based progress increases. Friendly fire still works, but grants **no damage XP**.
4. Heal wounded pawns using Verdant Mend and confirm progress matches the amount actually restored.
5. Cast a ward/speed buff and verify support training appears over time, not instantly on cast.
6. With two active slots, level one ability to Bronze while the other remains Iron; the essence rank should remain Iron.
7. Level the second ability to Bronze and confirm the essence advances. If another essence remains Iron, overall pawn rank remains Iron.
8. Change slot limit 2 → 5 → 2; check extra slots cannot be skipped for essence promotion and no previously earned power is lost.
9. Save/reload after partial XP and rank ups to confirm persistence.

## Versioned builds

GitHub Actions automatically names the build artifact using the version and run number, for example `Essencebound-v0.11-RW1.7-build10-attempt1.zip`; the installed mod directory stays `Essencebound`.

## Known limitations

Progression and weighted selection are v0.11 prototype mechanics that require successful C# compilation and in-game validation. The rank/attribute system is not claimed as a canon implementation, and the 12 reusable powers, two disputed confluence recipes, and placeholder spell effects remain. 
