# Essencebound v0.15 — ranked manifestations and universal lower-rank resistance

- Roll Iron/Bronze/Silver/Gold/Diamond for each newly manifested monster based on **magic density when its tear opened**. Weighted rank distributions progressively move from Iron toward Gold/Diamond as the colony matures.
- Store each monster's major rank with its existing persistent astral hediff, with labelled health stages, and retain marker/corpse-loot behavior.
- Scale monster outgoing damage with rank: x1, x1.15, x1.35, x1.6 and x2.
- For attacks with a pawn instigator, apply universal damage reduction when the defender has a higher rank than the attacker: x0.75 for one gap, x0.5625 for two, etc, with a x0.25 floor.
- Treat unranked attackers as below Iron. Essencebound users use their *lowest owned essence major rank* for defense, respecting synchronized rank advancement.
- Exclude environmental damage without a pawn instigator. Do not modify neutral medical effects or XP accounting.
- Use Harmony 3 `Pawn.PreApplyDamage` patch for both vanilla and Essencebound attacks; declare Harmony as a required mod, without bundling its runtime DLL.
- Preserve saves and settings; pre-v0.15 monsters have the default Iron rank.
- Add code/metadata assertions and rank-probability/mitigation regression checks.

Automated C# compilation and in-game confirmation still required. Vanilla monster artwork and monster abilities remain future work.
