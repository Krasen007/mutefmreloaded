@echo off
setlocal

set MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
set CONFIG=Release
set PLATFORM=x86
set SOLUTION=src\win\mutefm.sln

echo === mute.fm Reloaded - Development Build ===
echo.
echo Building (%CONFIG%^|%PLATFORM%)...
%MSBUILD% /p:Configuration=%CONFIG% /p:Platform=%PLATFORM% %SOLUTION% /v:quiet /nologo
if %ERRORLEVEL% neq 0 (
    echo Build failed with error code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
echo.
echo Build succeeded.
echo Output: src\win\bin\%CONFIG%\