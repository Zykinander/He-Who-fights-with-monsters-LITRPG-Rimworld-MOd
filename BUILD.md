# Essencebound v0.13 — RimWorld 1.7 build instructions

The repository includes C# source, XML definitions, and textures. It does not include a checked-in game DLL; GitHub Actions produces a compiled release artifact.

## Automatic build (recommended)
- Push to `main`, or manually run the workflow in `.github/workflows/build-essencebound.yml`.
- The workflow installs .NET 10, runs `Tests/test_database.py` and `Tests/test_powers.py`, compiles the assembly, and packages `Essencebound-1.7-compiled`.
- Confirm that the `Compile DLL` and `Verify data and assets` steps succeed.

## Local build
1. Install .NET 10 SDK and use the project's NuGet reference package for RimWorld 1.7, or point references at a legal local RimWorld copy.
2. Install Python and Pillow for structural tests (`python -m pip install Pillow`).
3. Run `python Tests/test_database.py`, `python Tests/test_powers.py`, then `dotnet build Source/Essencebound.Core.csproj -c Release --nologo`.
4. Confirm that `1.7/Assemblies/Essencebound.Core.dll` exists.
5. Copy `About/`, `1.7/`, and `LoadFolders.xml` into `Mods/Essencebound/`. Back up saves and test on a temporary colony.

The game requires RimWorld 1.7; the mod development version is v0.13. A successful build is not a substitute for in-game validation.
