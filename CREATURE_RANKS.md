# Inherent creature ranks (v0.20 development)

Every non-humanlike pawn (animals and other non-humanlike creatures) now has an **inherent rank by species**. Ordinary humanlike pawns remain unranked until they absorb essences. Inherent ranks affect the same incoming-damage reduction rules used by Essencebound fighters.

## Default categories

| Rank | Example creatures |
| --- | --- |
| Unranked | Squirrel, rat, hare, chicken, cat |
| Iron | Boomrat, raccoon, fox, alpaca, megascarab |
| Bronze | Wolf, warg, cougar, muffalo, horse, spelopede |
| Silver | Bears, gorilla, megaspider |
| Gold | Elephant, rhinoceros, megasloth |
| Diamond | Thrumbo |

The complete explicit table is in `Source/EssenceCreatureRanks.cs`. For other species, the code uses their `PawnKindDef.combatPower` (or body size if that statistic is unavailable) to choose a rank. This makes newly added modded creatures rank automatically, while still permitting a direct override.

## Mod compatibility

Third-party creature mods can put an `Essencebound.EssenceCreatureRankExtension` on the creature **ThingDef**:

```xml
<modExtensions>
  <li Class="Essencebound.EssenceCreatureRankExtension">
    <rankIndex>2</rankIndex>
  </li>
</modExtensions>
```

The range is -1 = Unranked, 0 = Iron, 1 = Bronze, 2 = Silver, 3 = Gold, 4 = Diamond.

**Priority**: astral-manifestation rank (saved on each manifested creature) > explicit species extension > built-in species mapping > combat-power/body-size fallback. Manifestation ranks stay density-driven. Rank inheritance does not make ordinary wildlife drop awakening stones; loot behavior can be introduced in a separate change.

## Game testing needed

- Compare an unranked colonist attacking a squirrel, warg, elephant, and thrumbo. Damage to Diamond thrumbos should use the existing 99% reduction against unranked attackers.
- Confirm two creatures of the same species receive the same inherent rank.
- Confirm astral manifestations still use their saved generated rank regardless of species.
- Test a custom modded creature with and without an extension.
- Check save/load; innate ranks are derived from species so no new per-pawn save data is needed.

This is a source-level implementation, not evidence that it has been playtested in RimWorld.
