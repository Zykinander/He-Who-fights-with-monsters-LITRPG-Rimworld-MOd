# Essencebound v0.6 — RimWorld 1.7 source build

This is NOT a compiled/tested game-ready mod. It supplies C# source, XML game defs, assets and offline validation.

1. Install the .NET 10 SDK. Install/use a legal RimWorld 1.7 copy.
2. In `Source/`, use the provided `net10.0` project. Its current NuGet reference (`Krafs.Rimworld.Ref` 1.7 pre-release) is an *untested* convenience. If unavailable, replace it with local References to `Assembly-CSharp.dll`, `UnityStubs.dll`, and `Waylith.dll` from your 1.7 install. Set CopyLocal=false.
3. Run `dotnet restore` and `dotnet build -c Release`.
4. Verify `1.7/Assemblies/Essencebound.Core.dll` was created. Without that file, the gameplay portion of this source archive will NOT load correctly.
5. Copy the whole `Essencebound_v0.6` mod directory into RimWorld's `Mods/`, ideally renamed `Essencebound`, and enable it on a new TEST save only.
6. Run `python Tests/test_database.py` and `python Tests/test_powers.py` for offline structural checks (these are not runtime tests).

The prototype follows the RimWorld 1.7 wiki guidance of .NET 10 and SDK-style C# projects. Its exact compatibility with the installed game assemblies requires local build and load testing. See the manual checklist in README.md.
