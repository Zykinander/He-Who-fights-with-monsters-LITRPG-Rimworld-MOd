# Essencebound v0.16 — Unique ability expansion

**RimWorld 1.7 | Test release | Requires Harmony | Back up saves before upgrading**

## Twelve new playable powers (24 total)

This update expands our previous 12 general power templates to **24 distinct abilities**. It adds **nine new effect mechanics** instead of just changing spell names. Every new power is available through the existing Awakening Stone aspect system and uses the same 2–5 ability slot limit per essence.

| Power | Role | What it actually does | Essence theme |
|---|---|---|---|
| Cinder Nova | Area damage | Burning pulse hurts hostile pawns near caster | Volcano |
| Thunder Chain | Chain attack | Hits the selected pawn plus up to two visible nearby hostiles | Thunderbird |
| Blood Siphon | Damage + self-healing | Damages target and heals the caster by a portion of effective damage | Blood |
| Life Bloom | Grouped injury healing | Spends a healing pool across several of one pawn's injuries | Fertile |
| Renewal Seed | Heal over time | Repairs actual injuries once every 120 ticks during its duration | Renewal |
| Grove Shelter | Group ward | Gives nearby nonhostile pawns temporary armor buffs | Tree |
| Purging Wave | Area cleansing | Removes Essencebound debuffs from nearby nonhostile pawns | Pure |
| Gravity Snare | Battlefield control | Slows a hostile pawn; progress follows active restraint | Vortex |
| Corrosive Strike | Armor debuff | Damages target and temporarily lowers its armor | Blight |
| Echo Burst | Splash attack | Damages selected pawn plus nearby hostile pawns at half magnitude | Echo |
| Spirit Beacon | Support buff | Temporarily improves work speed and ranged accuracy | Mystic |
| Moonlit Reprieve | Healing + ward | Repairs an injury and grants a protective buff | Moon |

These abilities are **original mod-gameplay interpretations**, not claims about specific canonical abilities from the books.

## Progression is tied to real results

New attack abilities earn XP from **effective damage dealt**. Life Bloom and Renewal Seed award XP only for **injuries actually healed**. Gravity Snare gains work while it genuinely affects a hostile pawn; Purging Wave only grants work for debuffs removed. Group wards grant periodic training while their benefits remain active. This carries forward the Iron 0–9 and per-essence synchronized rank requirements.

Rank resistance from v0.15 still applies to damage through the shared RimWorld damage pipeline; a Diamond defender continues taking **99% less damage from unranked attackers**.

## How to obtain the new powers

For new characters, 12 selected essences now grant one of these new abilities as their **innate starter** when first absorbed (for example, Volcano → Cinder Nova, Blood → Blood Siphon, Renewal → Renewal Seed). The other 214 essence starter mappings are unchanged.

All 24 powers can also appear as compatible Awakening Stone choices using their associated aspects. An essence favors powers aligned with its theme, but the stone/aspect system still allows unusual combinations.

**Existing saves:** Already awakened powers, cooldowns, affinity rolls and ranks are not rewritten. An essence absorbed *before* this release retains its original innate power. Only newly absorbed essences get the revised starter mapping.

## Testing checklist

1. On a temporary colony, absorb **Volcano**, **Blood**, **Renewal** or **Vortex** on new Essencebound pawns and check the new innate powers appear.
2. Use Thunder Chain against a target surrounded by other hostiles; verify the secondary hits and actual damage XP.
3. Test Life Bloom on a pawn with multiple wounds; the total healing should not exceed its configured budget.
4. Apply Renewal Seed to an injured pawn; watch wounds recover over time, with XP awarded only for healing.
5. Apply Grove Shelter and Spirit Beacon to allies. Confirm the hediff and periodic training persist across a save/reload.
6. Use Purging Wave on allies affected by Chilled, Static, Gravity Snare or Corroded Armor. No afflicted pawn means no cleanse XP.
7. Apply Gravity Snare to a hostile and verify it slows that pawn and grants duration-based work. Try it on an ally; the spell should be rejected.
8. Use Blood Siphon with and without wounds on the caster. Effective damage earns attack XP, and healing is limited to existing injuries.
9. Check that Stone awakenings now select from the wider pool, and that existing v0.15 powers still cast normally.
10. Examine Player.log for new missing-def, targeting, or save/load errors.

## Known limitations

New powers initially reuse existing awakening-stone icon art; unique sprites and animations are future work. Area effects select valid nearby pawns rather than producing visible projectile animations. The first wave favors breadth and mechanical diversity; we can add lore-specific abilities and distinctive animation passes next.

This is an experimental compiled release, not yet tested in-game. Install only one copy of `Mods/Essencebound`, with Harmony enabled first.
