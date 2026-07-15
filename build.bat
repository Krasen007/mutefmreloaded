@echo off
setlocal

set MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
set CONFIG=Release
set PLATFORM=x86
set SOLUTION=src\win\mutefm.sln
set OUTPUT_DIR=src\win\bin\%CONFIG%
set DIST_DIR=dist

echo === mute.fm Reloaded - Production Build ===
echo.

:: Step 1: Kill any running instance
echo [1/5] Stopping any running instance...
taskkill /f /im mute_fm_reloaded.exe >nul 2>&1
timeout /t 1 /nobreak >nul

:: Step 2: Clean previous build output
echo [2/5] Cleaning previous build output...
if exist "%OUTPUT_DIR%" rd /s /q "%OUTPUT_DIR%" 2>nul
if exist "%DIST_DIR%" rd /s /q "%DIST_DIR%" 2>nul

:: Step 3: Build the solution
echo [3/5] Building solution (%CONFIG%^|%PLATFORM%)...
%MSBUILD% /p:Configuration=%CONFIG% /p:Platform=%PLATFORM% /p:DebugSymbols=false /p:DebugType=none %SOLUTION% /v:quiet /nologo
if %ERRORLEVEL% neq 0 (
    echo Build failed with error code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
echo Build succeeded.

:: Step 4: Strip debug symbols (PDB files) from release output (belt-and-suspenders)
echo [4/5] Removing debug symbols from release output...
del "%OUTPUT_DIR%\*.pdb" /Q /F >nul 2>&1

:: Step 5: Create distribution package
echo [5/5] Creating distribution package...
if exist "%DIST_DIR%" rd /s /q "%DIST_DIR%" 2>nul
mkdir "%DIST_DIR%"
robocopy "%OUTPUT_DIR%" "%DIST_DIR%" /E /XF *.pdb /NJH /NJS /NDL /NP >nul
echo.

echo === Production build complete ===
echo Output: %DIST_DIR%\
dir "%DIST_DIR%" /A-D