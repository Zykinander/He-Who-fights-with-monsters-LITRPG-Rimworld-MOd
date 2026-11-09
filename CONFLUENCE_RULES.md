# Universal confluence rules (v0.21 development)

A pawn can absorb **three different normal essences**. It may not absorb a duplicate, and absorption order does not affect the result. As soon as the third unique essence is absorbed, its confluence is automatically unlocked as the fourth essence, including its innate starter power and ability slots.

There are 125 normal essence definitions, giving **317,750** possible three-distinct-essence combinations. The catalog contains 366 community-listed recipe records covering 364 different triples. Every valid triple now produces a confluence:

1. **Known unambiguous triple:** keep the existing recipe's named confluence and restriction status.
2. **Two disputed triples:** choose consistently among the explicitly catalogued outcomes, using a stable hash of the sorted essence names; mark the result `conflictresolved`. This is an *in-game resolution*, not a claim that the disputed canon recipe is settled.
3. **Unlisted triple:** select an existing confluence essence that has the greatest overlap with nearby known recipes (two matching components preferred over one, and one over none). Ties are resolved with the same stable hash. Mark the result `generated`. This is a mod-designed synthesis, not a source-novel recipe.

No extra XML entries are generated and there are no new confluence Def names; every result comes from the 101 existing confluence essences. The generator is deterministic, never uses runtime RNG or changes based on absorption order. A previously saved confluence remains untouched. For older saves with three distinct essences but an empty confluence, the missing fourth essence is restored upon loading; existing awakenings and ranks are retained.

The preview in the crystal absorption menu shows the exact confluence when a pawn already has two normal essences, with a generated/disputed label where appropriate. The pawn profile no longer implies that only known combinations work.

## Important gameplay distinction

"Every combination" means every combination of **three different** normal essences. A repeated normal essence is rejected. Results can be shared by multiple triples because there are more possible triples than confluence types.

## QA

The test `Tests/test_confluence_resolution.py` checks the full 317,750-triple input domain, confirms the 364 explicitly catalogued recipes and two disputes, and verifies code integration, save migration, and stable order-independent hashing. The GitHub Actions workflow also compiles RimWorld 1.7 C#.

**In-game testing still required:** absorb two different essences, preview third, complete a listed triple, complete an unlisted triple, try to repeat one essence, restart/reload to confirm a stable result, and load a legacy character with three essences and an unknown confluence. Test the two disputed triples to confirm a playable, stable result. The new results are procedural game content, not assertions of canon accuracy.
