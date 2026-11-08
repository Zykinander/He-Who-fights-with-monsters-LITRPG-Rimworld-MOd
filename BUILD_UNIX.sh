#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
echo "=== Essencebound RimWorld 1.7 local build ==="
if ! command -v dotnet >/dev/null; then
  echo "ERROR: .NET 10 SDK is not installed; no DLL can be generated here." >&2
  exit 2
fi
if ! dotnet --list-sdks | grep -q '^10\.'; then
  echo "ERROR: .NET 10 SDK missing; net10.0 is required." >&2
  exit 2
fi
if command -v python3 >/dev/null; then
  python3 Tests/test_database.py
  python3 Tests/test_powers.py
fi
rm -f 1.7/Assemblies/Essencebound.Core.dll
dotnet build Source/Essencebound.Core.csproj -c Release --nologo
test -s 1.7/Assemblies/Essencebound.Core.dll || { echo "ERROR: DLL was not generated." >&2; exit 1; }
echo "SUCCESS: generated 1.7/Assemblies/Essencebound.Core.dll"
echo "Install About/, LoadFolders.xml, and 1.7/ into the RimWorld 1.7 Mods/Essencebound folder."
