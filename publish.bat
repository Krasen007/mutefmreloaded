@echo off
setlocal enabledelayedexpansion

call build.bat
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

for /f "tokens=2 delims=()" %%v in ('findstr /v /c:"//" src\win\Properties\AssemblyInfo.cs ^| findstr /c:"AssemblyVersion("') do set "VERSION=%%~v"

tar -a -c -f "mute_fm_reloaded-v!VERSION!.zip" dist
echo Created mute_fm_reloaded-v!VERSION!.zip
