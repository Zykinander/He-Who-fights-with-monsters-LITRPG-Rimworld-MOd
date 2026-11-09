# Essencebound v0.18 — RimWorld 1.7 quick start

The preferred route is **GitHub Actions** because the working repository has already produced compiled DLLs using .NET 10.

1. In GitHub, open the `Actions` tab and select **Compile Essencebound RimWorld 1.7**.
2. Choose the newest successful `main` build. Download the `Essencebound-v0.18-RW1.7-buildN-attempt1` artifact.
3. Extract the artifact, then extract `Essencebound-v0.18-RW1.7-buildN-attempt1-mod.zip`.
4. Replace the old local `Mods/Essencebound/` folder with the new one; do not install two copies.
5. Open RimWorld 1.7, verify that the mod menu displays **Essencebound v0.18**, then create/load a disposable test colony.

The release remains experimental. Back up important saves before upgrading. The `v0.18` label is our development version, while `1.7` is the RimWorld game version.

To compile locally, install .NET 10 SDK and Python+Pillow, then run `BUILD_WINDOWS.cmd` or `BUILD_UNIX.sh` (see `BUILD.md`).
