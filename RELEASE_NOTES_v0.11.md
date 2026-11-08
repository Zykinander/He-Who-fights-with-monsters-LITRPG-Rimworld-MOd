# Essencebound v0.11 — ranks, work-based training and weighted affinities

## Gameplay
- Every unlocked ability starts at Iron and earns work through actual damage, healing, buff active duration, successful cleanse or area damage.
- Iron/Bronze/Silver/Gold/Diamond progression requires 40, 120, 360 and 1080 additional work.
- Each essence advances its own rank only after all *required* slots are unlocked and their abilities reach the next tier. A pawn's overall rank is the minimum rank across possessed essences.
- The progression requirement persists the highest slot cap ever required for an essence, to stop an easy exploit by lowering the configured 2–5 slot limit.
- All 226 essence types have weighted probabilities for Power/Speed/Spirit/Recovery; each pawn rolls independently and saves the result. No reroll on reloading.
- Rank bonuses stack based on each essence's rolled affinity: Power +4% essence damage, Speed -2.5% ability cooldown, Spirit +5% positive buff duration, Recovery +6% essence healing per tier.
- Existing v0.10 save data and unlocked abilities are preserved; missing affinity/rank records initialize when loading.
- Attack training only counts damage to hostile targets, reducing friendly-fire XP farming.
- New character UI shows per-essence affinity/rank and power XP bars.

## Data
`Data/essence_attribute_weights.csv` mirrors four positive XML weights on each normal and confluence `EssenceDef`, with some thematically weighted and some balanced.

## Validation
GitHub Actions should run structural XML/source checks and compile the 1.7 assembly. In-game tests are needed for accuracy of work credit, persistence and rank bonuses.
