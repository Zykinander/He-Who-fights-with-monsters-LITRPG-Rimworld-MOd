# Essencebound v0.14 — Colony strength drives magical density

**Development version v0.14 | RimWorld 1.7 | Experimental — test on backed-up saves**

## Magical density now follows your colony

Instead of assigning each map a random magic-density value when first created, **Essencebound recalculates local magic density from the strength of the colony on that map**.

| Source of colony strength | Maximum contribution |
|---|---:|
| Starting magical background | 9 percentage points |
| Colony wealth (gear, buildings, resources) | 23 percentage points |
| Number of spawned free colonists | 14 percentage points |
| Colonists' Shooting or Melee skills (whichever is higher) | 12 percentage points |
| Absorbed essence count and the rank/sublevels of each essence | 42 percentage points |

These add up to 100% at very high strength and are subject to the **Low (0.65×), Normal (1×), High (1.45×)** multiplier and a final 5%–100% clamp. The displayed percentages for each strength factor in settings indicate *how close that factor is to its full contribution*, not its percentage-point contribution.

The weights are early gameplay balancing choices, not canonical book formulas. A new colony starts at relatively low magic density, while a wealthy, experienced, essence-rich colony attracts far more magic.

### How progression affects encounters

- The map's daily astral-tear roll now uses the **current** colony-derived density. More developed colonies have a higher chance of a tear, subject to the existing event-frequency setting.
- A tear takes a **snapshot** of the density when it opens. If your colony changes while the tear is already open, that tear's outcome remains based on the earlier reading.
- The chance that a tear releases a hostile magical monster still increases with density.
- Existing monster placeholders continue to scale from Megascarab (low) → Warg (medium) → Thrumbo and two monsters (high).
- When your pawns level their Iron 0–9, Bronze, Silver, Gold and Diamond essences, their growing magical ability contributes directly to colony density. More Essencebound pawns contribute more.

## See your colony's actual density

Open **Options → Mod Settings → Essencebound** while playing a colony. Under *Astral manifestations and magic density*, you will see:

- Current colony magic density as a percentage.
- Wealth, population, combat-skill and essence-rank factors, each on a 0–100 scale.

The density changes as your population, wealth, skills, and essence power grow. The setting changes the multiplier, not the underlying strength.

## Existing colonies and saves

Existing v0.13 saves can upgrade to v0.14 without resetting any pawn's essences, ranks, powers or affinities. The old map-wide random `ebAmbientDensity` is simply ignored; the map's next scheduled manifestation check remains saved. **Already-open astral tears** retain the density they recorded when they appeared.

No changes to Astral Loot's nearby-corpse menu, mark requirement, 10-cell range or cooldown.

## Testing checklist

1. Open a newly founded colony and check the displayed magic-density value in Mod Settings.
2. Add wealth or recruit colonists; the density should rise.
3. Absorb essences into a colonist and advance an essence from Iron 0 to Iron 1; the essence contribution should rise, even if slightly.
4. Try the density preset Low → Normal → High; the total should change immediately.
5. In Dev Mode, open a tear and inspect its density. Increase colony strength while it is open; the tear should still resolve at its recorded opening density.
6. Save and reload; check that the current density reflects colony strength and that the scheduled tear and pawn progress remain intact.
7. On a mature test colony, check whether stronger monster manifestations become more likely than on the starter colony.

## Still in development

The manifestation creatures, tear sprites and loot pool are still prototypes; custom creatures, progression-adjusted loot and unique abilities come later. All existing configurable 2–5 ability slots, Iron 0–9 advancement, weighted Power/Speed/Spirit/Recovery affinities and Astral Loot controls remain.

GitHub Actions creates uniquely named build archives (e.g. `Essencebound-v0.14-RW1.7-build23-attempt1`); the installed folder remains `Essencebound`.
