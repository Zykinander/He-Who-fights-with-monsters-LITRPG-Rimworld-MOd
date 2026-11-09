# Essencebound v0.15 — Ranked magical monsters and rank-based damage resistance

**RimWorld 1.7 experimental build | Back up saves before testing | Requires Harmony mod**

## Monsters now receive magical ranks

Every astral tear snapshots the colony's **current magic density** when it opens, using the v0.14 formula (wealth, population, combat skill and essence ranks). If it releases a monster, that monster rolls one **major magical rank** based on the snapshot:

| Magic density | Iron | Bronze | Silver | Gold | Diamond |
|---|---:|---:|---:|---:|---:|
| 5–24% | 95% | 5% | 0% | 0% | 0% |
| 25–49% | 58% | 39% | 3% | 0% | 0% |
| 50–74% | 15% | 55% | 27% | 3% | 0% |
| 75–94% | 0% | 15% | 65% | 19% | 1% |
| 95–100% | 0% | 0% | 20% | 75% | 5% |

These are mod-original balance weights, chosen to avoid an early Diamond encounter while allowing occasional rare higher ranks.

Monsters still use vanilla **visual placeholders** (Iron: Megascarab, Bronze: Warg, Silver/Gold/Diamond: Thrumbo). This version focuses on **rank logic and combat**, not creature art. Gold and Diamond manifestations spawn in pairs; lower ranks spawn alone. The opening alert identifies each monster's rank, and its health tab shows a rank-labelled astral manifestation hediff.

### Higher ranked manifestations hit harder

| Monster rank | Damage multiplier for the monster's attacks |
|---|---:|
| Iron | 1.00× |
| Bronze | 1.15× |
| Silver | 1.35× |
| Gold | 1.60× |
| Diamond | 2.00× |

Ranked monsters retain their rank when they die, so the same corpse remains eligible for the existing **Astral Loot** power.

## Universal rank difference protection

**Any pawn with an Essencebound major rank**, including colonists and astral manifestations, takes less damage from **lower-ranked pawns**. This applies to RimWorld pawn-instigated attacks and Essencebound ability damage, not only custom spells.

### Intensified rank-resistance balance update

| Defender rank | Damage taken from unranked attacker | Reduction |
|---|---:|---:|
| Iron | 80% | 20% |
| Bronze | 60% | 40% |
| Silver | 40% | 60% |
| Gold | **20%** | **80%** |
| Diamond | **1%** | **99%** |

These are the exact damage multipliers against an **unranked attacker**. For attacks between ranked pawns, damage is the ratio of the defender's baseline to the attacker's baseline. Examples: Iron attacking Bronze deals 75% damage; Silver attacking Gold deals 50%; Gold attacking Diamond deals only 5%; and Iron attacking Diamond deals 1.25%. **An attacker at the same or a higher rank gets no rank-based penalty.**

This rule applies to all pawn-instigated hits through the existing Harmony damage hook. A colonist's defensive rank is their **lowest-ranked owned essence**, matching the overall rank in the profile. Ordinary armor and other health mechanics still apply after the rank-based damage adjustment. Diamond's extreme protection is deliberate and may require further gameplay tuning.

Damage from fires, weather and other hazards without a pawn instigator remains unchanged. Healing and recovery are not treated as attacks.

### Harmony requirement

RimWorld 1.7 needs the **Harmony** mod (package ID `brrainz.harmony`) enabled before Essencebound. The v0.15 mod uses Harmony 3 to patch RimWorld's shared `Pawn.PreApplyDamage` method. This avoids applying resistance only to our 12 prototype powers. **Do not ship a second copy of Harmony's DLL** alongside Essencebound.

## Testing checklist

1. Enable Harmony first, load a temporary test colony, and confirm v0.15 appears in the mod list.
2. Generate multiple tears at Low/Normal/High magic density and observe the rank in each spawn alert and the monster's Health tab.
3. Check that an unranked colonist hits an Iron monster for ~80% normal damage, a Gold monster for ~20%, and a Diamond monster for ~1%, before ordinary armor calculations.
4. Check an Iron essence user fighting a Bronze monster for ~75% damage and a Bronze user fighting an Iron monster for normal damage.
5. Reverse the matchup: Bronze defenders should resist Iron attackers; Iron defenders should not resist Bronze attackers.
6. Check damage to ranked colonists from unranked enemies (even ordinary animals).
7. Save/reload a ranked monster and its corpse. Its rank and Astral Loot eligibility should persist.
8. Check that hazards without instigators still behave normally.
9. Verify the log has no Harmony patching or XML errors.

## Upgrade notes

Existing saves and v0.14 essence ranks remain intact. Existing manifested monsters from v0.13/v0.14 load as **Iron-ranked** unless spawned anew with the v0.15 ranking logic. Existing astral tear snapshots are unchanged.

This version does not yet add unique monster sprites or entirely new monster powers. Those are reserved for the next pass after rank combat and difficulty have been validated.
