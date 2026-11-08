# Essencebound v0.11 — ranks, work-based training and weighted affinities

## Gameplay
- Every unlocked ability starts at Iron and earns work through actual damage, healing, buff active duration, successful cleanse or area damage.
- Iron/Bronze/Silver/Gold/Diamond progression requires 40, 120, 360 and 1080 additional work.
- Each essence advances its own rank only after all *required* slots are unlocked and their abilities reach the next tier. A pawn's overall rank is the minimum rank across possessed essences.
- The progression requirement persists the highest slot cap ever required for an essence, to stop an easy exploit by lowering the configured 2–5 slot limit.
- All 226 essence types have weighted probabilities for Power/Speed/Spirit/Recovery; each pawn rolls independently and saves the result. No reroll on reloading.
- Rank bonuses **double** at each tier: Iron x1, Bronze x2, Silver x4, Gold x8, Diamond x16. Per-essence bonuses therefore reach Power +64%, Speed -40% cooldown, Spirit +80% buff duration and Recovery +96% healing at Diamond.
- Identically rolled affinities still stack across essences. Speed reductions have an 80% total cap to prevent zero cooldown; other benefits have a high +400% stacking cap so no legal four-essence Diamond combination is truncated.
- Existing v0.10 save data and unlocked abilities are preserved; missing affinity/rank records initialize when loading.
- Attack training only counts damage to hostile targets, reducing friendly-fire XP farming.
- New character UI shows per-essence affinity/rank and power XP bars.

## Data
`Data/essence_attribute_weights.csv` mirrors four positive XML weights on each normal and confluence `EssenceDef`, with some thematically weighted and some balanced.

## Validation
GitHub Actions should run structural XML/source checks and compile the 1.7 assembly. In-game tests are needed for accuracy of work credit, persistence and rank bonuses.
