# Inherent creature ranks (v0.20 development)

Every non-humanlike pawn (animals and other non-humanlike creatures) now has an **inherent rank by species**. Ordinary humanlike pawns remain unranked until they absorb essences. Inherent ranks affect the same incoming-damage reduction rules used by Essencebound fighters.

## Viewing creature ranks in-game

**Select a living creature on the map.** Its normal bottom-left inspection panel now shows `Creature rank: Unranked / Iron / Bronze / Silver / Gold / Diamond`. Creatures spawned from astral tears instead show `Astral rank: <rank>`, which reflects the rank assigned by the tear even if the underlying vanilla creature type normally has a different rank. Both labels use the same rank resolver as damage calculations and loot drops, so the displayed value matches gameplay. The line appears alongside vanilla inspection text; it does not require opening the Health tab or developer mode.

## Health tab rank status

Open a living creature's **Health** tab to see the permanent, non-harmful `Creature rank: <rank>` hediff, using the same computed rank as the bottom-left inspection line. It will show `Astral rank: <rank>` on creatures that were spawned by an astral tear, which may differ from the inherent rank of the base species. A separate astral manifestation marker can also appear for these monsters.

The rank status has no damage, pain, buffs, or health effects; all damage and loot calculations still use `EssenceRankCombat.RankOf`. It is automatically attached once when a creature spawns **or when existing map creatures are loaded from a saved game**. Reloads and respawns must not create duplicate hediff entries.

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

**Priority**: astral-manifestation rank (saved on each manifested creature) > explicit species extension > built-in species mapping > combat-power/body-size fallback. Manifestation ranks stay density-driven.

## Random essence and awakening-stone loot

Ranked creatures roll **two independent drops** once when they die on a map and leave a corpse. Both are optional: one creature can drop an essence, an awakening stone, both, or neither.

| Rank | Essence drop | Awakening stone drop | Available stone rarities |
| --- | ---: | ---: | --- |
| Unranked | 0.1% | 1% | Common |
| Iron | 1% | 8% | Common, Uncommon |
| Bronze | 2% | 16% | Common, Uncommon, Rare |
| Silver | 4% | 26% | Common through Epic |
| Gold | 7% | 40% | Common through Legendary |
| Diamond | 12% | 60% | Common through Legendary (strongest Epic/Legendary weighting) |

- Essence crystals are intentionally **5–10 times less likely** than awakening stones at every rank. Each successful roll produces one random **normal** essence or one random awakening stone of a rank-eligible rarity.
- The 30 awakening stones are chosen from matching XML items, with rank-weighted rarity and random stone type.
- Astral manifestations use their saved manifestation rank and get a 20% relative boost to both drop probabilities (capped at 95% essence / 80% stone).
- The existing **Astral Loot** gizmo offers a separate bonus extraction from an astral corpse at 60% of that rank's boosted normal chance. It consumes the corpse and starts its cooldown even if nothing is recovered. This prevents repeat attempts on the same corpse.
- Death drops are rolled once, when `Pawn.Kill` successfully creates a spawned corpse. Hauling or reloading the corpse does not roll more drops. Creatures killed without a corpse or outside a map yield no automatic drop.
- The old guaranteed essence from manually looted astral remains has been replaced with these probabilities. Standalone astral tears that resolve directly into a crystal are unchanged.


## Game testing needed

- Kill several same-species creatures in a test colony; drops should vary from one corpse to another (including none), without spawning on save/reload.
- Use both essence and stone rewards; verify the items are the real existing `ThingDef` items that pawns can absorb/awaken with.
- Repeated Iron/Thrumbo tests: the observed aggregate rates should approach the tables; ensure no Legendary stone comes from Iron creatures.
- Kill a manifested creature, confirm the direct drops and then use Astral Loot; no rerolls after its corpse is consumed.
- Test a corpse destroyed without spawning, and a dead creature arriving on a map; these should not trigger unexpected extra rewards.

- Select a squirrel, warg, elephant and thrumbo; the bottom-left inspection panel **and Health tab hediff** should show Unranked, Bronze, Gold and Diamond, respectively.
- Select a spawned astral monster whose generated rank differs from its vanilla species rank; its panel must show the astral rank, not the species rank.
- Compare an unranked colonist attacking a squirrel, warg, elephant, and thrumbo. Damage to Diamond thrumbos should use the existing 99% reduction against unranked attackers.
- Confirm two creatures of the same species receive the same inherent rank.
- Confirm astral manifestations still use their saved generated rank regardless of species.
- Test a custom modded creature with and without an extension.
- Load a previous save with existing wildlife; each living animal gains exactly one rank hediff. Save/reload again and confirm no duplicate rank hediffs.

This is a source-level implementation, not evidence that it has been playtested in RimWorld.
