# Essencebound v0.13 — astral manifestations

- Add per-map saved magical density with basic biome adjustment and a normal/low/high density modifier.
- Schedule astral tears via a daily density-weighted roll on populated player-home maps.
- Tear is a visible 900-tick Thing with a persisted countdown; it resolves into a normal essence crystal or hostile monster(s).
- Monster chance and placeholder creature type increase with local density; two creatures at high density.
- Spawned magical monsters carry a saved astral marker; their corpses are eligible for one-time essence extraction.
- Add an **Astral Loot** targeted gizmo to essence users, using 10-cell range, line of sight, one essence per marked corpse, 3600-tick cooldown, and one-time remains consumption.
- Add DEV-mode test gizmo for immediately opening a tear.
- Mod settings control tear occurrence, monster presence, frequency and effective density.
- No changes to v0.12 XP and essence rank progression save fields.

This is an experimental first pass: vanilla animal templates and the existing StoneMagus texture are placeholders pending unique magical monster and portal artwork. Needs compilation and in-game validation.
