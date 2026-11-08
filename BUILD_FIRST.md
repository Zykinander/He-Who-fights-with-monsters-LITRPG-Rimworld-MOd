# Essencebound RimWorld 1.7 — v0.6 build kit

**Status: source only; NO compiled DLL is included.** Source compiled/in-game tested? **NO**.

## Requirements
- RimWorld **1.7** (unstable branch as of November 2026)
- **.NET 10 SDK** (not merely runtime), available as `dotnet` on PATH
- Internet access to restore the `Krafs.Rimworld.Ref` NuGet reference package (1.7 prerelease)

No proprietary RimWorld DLLs are redistributed in this kit. The NuGet reference is only used to compile.

## Windows
1. Extract the ZIP and open the `Essencebound_v0.6_BuildKit` directory.
2. Double-click `BUILD_WINDOWS.cmd` or use a terminal within that directory.
3. It runs the structural tests and builds with .NET 10. If a DLL is successfully generated, it will be at `1.7/Assemblies/Essencebound.Core.dll`.
4. Rename the mod directory to `Essencebound` and copy it into RimWorld's local `Mods` directory. The runtime mod needs `About`, `LoadFolders.xml`, and `1.7` only.
5. Enable in RimWorld 1.7 on a **test save** and inspect the game's debug log for errors.

## Linux/macOS
Run `bash BUILD_UNIX.sh` from within the extracted directory, after installing .NET 10 SDK.

## Automated build (GitHub Actions)
The included `.github/workflows/build-essencebound.yml` builds with .NET 10 in a hosted runner, runs tests, and uploads the compiled mod as an artifact. To use it, put the CONTENTS of this kit at the repository root and push to GitHub, then run the workflow from Actions.

## If it fails
The compiler's error lines (including file name, line number, `CSxxxx`) are important. Please share those and the `dotnet --info` output so the project can be corrected for your specific 1.7 build.

### Limitations
- The structural tests are NOT compilation or gameplay tests.
- A successful build is NOT a guarantee of in-game compatibility.
- Do NOT run the XML-only preview with the compiled mod; item definitions overlap.
- Confluence conflicts are preserved rather than guessed.
