# Essencebound v0.12 — numbered rank levels, smoother attribute growth and damage XP fixes

**RimWorld 1.7 | Development version v0.12 | Please test on a copy of your save**

## Progression

Each ability advances **Iron 0 → Iron 1 → ... → Iron 9 → Bronze 0**, and repeats for Bronze, Silver and Gold before reaching Diamond 0. Each numbered step costs 1/10th the previous v0.11 full-rank work threshold:

| Major rank | Work per numbered advancement | Total work to next major rank |
|---|---:|---:|
| Iron | 4 | 40 |
| Bronze | 12 | 120 |
| Silver | 36 | 360 |
| Gold | 108 | 1080 |

Each ability still earns work from its **actual game effect**: inflicted damage, injury severity healed, active buff time, successful cleansing, or total area damage. Zero-damage hits give zero damage XP.

An **essence's displayed numbered level** is its least-developed required awakened ability. If required slots are empty, it cannot progress beyond numbered level 0 yet. The essence only advances to the next *major rank* when **all its required slots** have abilities that reached that next rank.

### Tier synchronization

After any ability reaches **Bronze 0**, its Bronze training waits until **every currently owned essence** (normal or confluence) also reaches Bronze. The same synchronized gate applies at Silver and Gold. Unowned slots and missing confluences do not block progress. A newly absorbed Iron essence can temporarily pause higher-tier training.

### Attribute growth every numbered level

Power, Speed, Spirit and Recovery bonuses **increase at each numbered essence level** rather than only at major rank boundaries. They interpolate smoothly between the existing doubled rank bonuses:

| Essence level | Power | Speed cooldown reduction | Spirit buff duration | Recovery healing |
|---|---:|---:|---:|---:|
| Iron 0 | +4.0% | -2.5% | +5.0% | +6.0% |
| Iron 1 | +4.4% | -2.75% | +5.5% | +6.6% |
| Iron 5 | +6.0% | -3.75% | +7.5% | +9.0% |
| Iron 9 | +7.6% | -4.75% | +9.5% | +11.4% |
| Bronze 0 | +8.0% | -5.0% | +10.0% | +12.0% |
| Silver 0 | +16.0% | -10.0% | +20.0% | +24.0% |
| Gold 0 | +32.0% | -20.0% | +40.0% | +48.0% |
| Diamond 0 | +64.0% | -40.0% | +80.0% | +96.0% |

The four attribute affinities are still randomly rolled for each essence with its original themed weights and saved on the pawn. An essence's affinity bonus is based on **that essence's own rank and numbered level**, not the pawn's global rank. Bonuses from multiple matching affinities still stack, with the existing 80% cap on cooldown reduction.

## Damage progression fix

Previously, even though targeting was unrestricted, **friendly and neutral hits earned zero progression**, because the executor required `HostileTo(caster)`. Now valid hits earn actual reported damage on **any pawn**, with an injury-severity fallback for damage handlers that report no direct damage result. Hits that do no damage grant zero work. The cast feedback reports **Progress +X** and the current ability level. If a rank gate blocks training, the message explicitly reports that.

No explicit Essencebound exception appeared in the provided Player.log; this addresses a concrete inconsistency in the code rather than claiming every possible lack-of-XP issue is resolved.

## Old saves

The new `abilitySublevel` save value migrates the previous v0.11 `rankWork` bar to numbered levels, preserving leftover earned progress. Existing attribute rolls, unlocked abilities, cooldowns and essence rank records are reused.

The existing configurable 2–5 slots per essence remain. The highest slot requirement previously reached is preserved so lowering the cap cannot promote an essence prematurely.

## Test plan

1. Use an **existing essence pawn** and hit an allied pawn in a temporary test colony. Verify health drops, then check cast feedback shows **Progress +X** and that the ability bar increases.
2. Repeat on a hostile pawn. Both hits should award work if damage was inflicted.
3. Build up an ability through Iron 1 and ensure the profile shows its new level.
4. Advance *all required abilities* through Iron 1 and check that their essence reaches Iron 1; its affinity bonus should rise slightly.
5. Advance all required abilities to Bronze 0; check the essence reaches Bronze 0.
6. Keep another owned essence at Iron and confirm Bronze training is stalled until that essence also reaches Bronze 0.
7. Save/reload; confirm rolled affinities, numbered levels, partial work, and stat bonuses remain unchanged.

The in-game mod folder is still **Essencebound**. GitHub Actions names test downloads uniquely by the development version and build number.
