# Essencebound v0.10 — Four-panel character interface and configurable slots

- The Essence profile pawn gizmo now opens a scrollable RimWorld-native window.
- Four panels: three normal essences and one confluence (locked until discovered).
- Displays unlocked powers with their names, icons, short descriptions, innate/stone source and cooldowns.
- Shows the existing rank and the sequence Iron → Bronze → Silver → Gold → Diamond; rank XP is not yet implemented.
- New Mod Settings screen; shared per-essence ability slot count can be set to 2, 3, 4 or 5. Default 3.
- Fully awakened pawn cap becomes 8–20 powers depending on configuration.
- Added slot validation in absorption, awakening and casting. Pawn gizmos only expose active slots.
- Settings changes apply to existing pawns on the next UI/ability interaction.
- Old ability data and cooldowns in inactive high-index slots remain in save data and return when slot cap rises again.
- No changes to essence identity, confluence recipe resolution, targeted spell allegiance rules or 30 awakening stone defs.
- GitHub Actions build artifact naming stays unique per workflow run and attempt.

## Validation still required
The change needs successful GitHub Actions compilation and a RimWorld 1.7 in-game UI test at 2, 3 and 5 slots.
