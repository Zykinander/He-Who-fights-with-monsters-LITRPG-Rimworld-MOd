# Essencebound v0.17 — halfway to 100 playable abilities

**RimWorld 1.7 | Requires Harmony | Test build; back up saves**

## The 100-ability target

We now have **50 playable abilities**, up from 24 in v0.16. Another **50 named, themed designs** are tracked in [Data/ability_roster_100.csv](Data/ability_roster_100.csv), for a total of **100 distinct planned ability identities**. The remaining 50 are **designs only**, not yet functional game abilities.

The roster records each ability's identity, target essence, role, intended mechanic and target milestone. It preserves the goal without inflating the current playable count or promising the designs are already implemented.

| Stage | New playable powers planned | Cumulative playable target |
|---|---:|---:|
| v0.17 | 26 | **50** |
| v0.18 | 20 | 70 |
| v0.19 | 20 | 90 |
| v0.20 | 10 | **100** |

## The 26 new playable abilities

| Ability | Example favored essence | Actual implementation |
|---|---|---|
| Inferno Lance | Fire / Sun | Longer-ranged focused flame damage |
| Frost Halo | Ice / Cold | Circular hostile-only impact |
| Quakebreaker | Earth | Main strike + visible nearby hostile splash |
| Zephyr Ricochet | Wind | Chain attack with up to three secondary targets |
| Umbral Leech | Dark / Void | Damage converted into existing injury healing |
| Viper's Bite | Venom | Damage plus slowing / work-disrupting venom |
| Shard Volley | Crystal | Shard chain attacks |
| Briar Detonation | Plant | Burst damage + area entangling debuff |
| Starfall | Star / Stellar | Focused celestial impact + splash |
| Tidal Rebuke | Water | Short-range hostile shockwave |
| Iron Bastion | Iron | Heavier armor with a modest speed cost |
| Mirror Guard | Mirror | Evasive buff and light armor |
| Dawn Chorus | Dawn | Group speed and work-support aura |
| Verdant Sanctuary | Verdant | Area healing for up to four actually injured allies |
| Ocean Renewal | Ocean | Gradual healing for up to four injured allies |
| Cleansing Ray | Light | Single-target healing plus affliction removal |
| Mercy Thread | Ministration | Chain healing across a selected pawn and two nearby allies |
| Soul Harvest | Death | Hostile-only draining pulse that restores injuries |
| Rune Bind | Rune | Hostile control debuff earning duration progress |
| Omen Hex | Omen | Hostile accuracy / work disruption over time |
| Temporal Haste | Time | Accelerates the caster's movement and work |
| Battlefield Focus | Battlefield | Improves ranged accuracy and dodging |
| Blade Tempest | Sword | Close-range hostile-only damage ring |
| Nemesis Strike | Nemesis | Increased damage if the target has significant wounds |
| Thread Snare | Thread | Impact plus slowing snare |
| Glass Ward | Glass | Healing followed by protective armor |

**Seven new effect families** support group healing, chain healing, group regeneration, cleansing with healing, area damage with status effects, life-draining area pulses, and injury-aware finishing strikes. Their XP tracking follows the actual work performed. Buff and control effects continue granting work during meaningful active duration.

## Access and compatibility

Every new power has compatible Awakening Stone aspects and favored essence themes, allowing the existing 30 awakening stones to select them. Twenty-six selected essences have new innate starter mappings when first absorbed. Previously awakened or innate powers in old saves are **not replaced**.

The existing Iron 0–9 rank progression, rank synchronization, random Power/Speed/Spirit/Recovery attributes, 2–5 slots per essence, Gold/Diamond resistance, and astral manifestations remain.

Power icons currently reuse existing awakening-stone artwork. This milestone prioritizes gameplay and reliable power definitions over unique sprite animations.

## Gameplay test checklist

1. On a disposable colony, try newly absorbing Sun, Earth, Dawn, Verdant, Ocean, Nemesis, Rune and Glass. Confirm each receives its matching new starter.
2. Cast **Verdant Sanctuary** with multiple injured allies. Verify total progress equals effective healing and that casting near healthy allies fails without spending cooldown.
3. Cast **Ocean Renewal** with multiple injured allies. Verify regeneration ticks and resulting XP after saving and reloading.
4. Cast **Cleansing Ray** on a pawn with Venom Haze, Rune Bind or an older Essencebound debuff. Verify cleansing and healing work.
5. Cast **Briar Detonation** on a pack of hostiles, and test that blocked secondary targets do not take area damage.
6. Cast **Soul Harvest** with wounded caster, then with healthy caster. It may earn damage XP with no restorative effect, but never creates new wounds.
7. Apply **Rune Bind** or **Omen Hex** to a hostile. Confirm the status and progress while active; casting on an ally should be rejected.
8. Confirm previously learned v0.15/v0.16 powers, attribute rolls and ranks are unchanged after load.
9. Test Gold and Diamond rank mitigation on new powers (all damage flows through RimWorld's pawn damage pipeline).
10. Check Player.log for missing XML def names, statuses, or runtime exceptions and share any new errors.

## Roster integrity

The repository runs tests ensuring all 100 identities are unique, all 50 playable entries exist in the active XML/JSON catalog, and the 50 future entries are clearly labelled as planned. It also verifies all new abilities' status definitions, aspects, starter mappings and work-handling code.

**Not a claim of canon accuracy.** Names, mechanics and future ability ideas are original mod-development designs inspired by the setting.

A separate v0.17 draft is based on the current **v0.16 development branch**, so PR #11 should be merged first, then the v0.17 PR, to preserve all 24 existing v0.16 powers.
