# Essencebound v0.19 — First-pass art and animated sigils

- All 100 ability power XML definitions now reference dedicated power icons, not awakening-stone placeholders.
- The CI builder uses Pillow to export **100 distinct transparent 128×128 icons** and **100 matching sigil PNGs**, included in the installable mod ZIP.
- Added 100 new `EB_AbilitySigil_...` fleck definitions using the matching per-power PNG. Successful casting shows a short fade/growth at the caster and target. Persisting field pulses show corresponding sigil effects.
- Art generation is deterministic and documented in `Tools/generate_ability_icons.py` plus `Data/ability_visual_designs.json`.
- Test suite validates dimensions, RGBA alpha, uniqueness, ability-to-asset references, and packaged existence. C# compilation validates the visual dispatch.
- **Not yet** custom hand-painted art, frame-by-frame character animations, unique sound files, or in-game tested.
- Existing powers, balance, rank mitigation, cooldowns, awakening eligibility and old save data are left intact. Legacy saved world fields lacking the new sigil ID remain safe and simply skip their field VFX.

## Local build
`python -m pip install Pillow` then `python Tools/generate_ability_icons.py` then `python Tests/test_generated_ability_art.py` before compiling. GitHub Actions performs these steps and includes the files automatically.
