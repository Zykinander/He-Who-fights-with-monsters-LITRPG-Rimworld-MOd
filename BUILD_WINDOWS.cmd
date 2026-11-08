@echo off
setlocal
cd /d "%~dp0"
echo.
echo === Essencebound 1.7 build ===
where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: .NET SDK not found. Install .NET 10 SDK, then rerun.
  exit /b 2
)
dotnet --list-sdks
set "HAVE_DOTNET10="
for /f "tokens=*" %%x in ('dotnet --list-sdks ^| findstr /b "10."') do set "HAVE_DOTNET10=1"
if not defined HAVE_DOTNET10 (
  echo ERROR: .NET 10 SDK not installed. The 1.7 mod targets net10.0.
  exit /b 2
)
if exist "1.7\Assemblies\Essencebound.Core.dll" del "1.7\Assemblies\Essencebound.Core.dll"
python --version >nul 2>nul
if not errorlevel 1 (
  python Tests\test_database.py || exit /b 1
  python Tests\test_powers.py || exit /b 1
)
dotnet build "Source\Essencebound.Core.csproj" -c Release --nologo
if errorlevel 1 (
  echo ERROR: Compile failed; inspect the compiler errors above. No working DLL is certified.
  exit /b 1
)
if not exist "1.7\Assemblies\Essencebound.Core.dll" (
  echo ERROR: Build command finished but DLL is missing from 1.7\Assemblies.
  exit /b 1
)
echo.
echo SUCCESS: 1.7\Assemblies\Essencebound.Core.dll generated.
echo Copy the folder (renamed Essencebound) to RimWorld Mods and test on a new save.
exit /b 0
