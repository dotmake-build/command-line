@echo off
setlocal enabledelayedexpansion

set srcFolder=..\docs
set apiFolder=..\docs\api
set publishFolder=..\publish\Docs-WebSite
set publishOfflineFolder=..\publish\Docs-Offline
set publishedCount=0

if exist "%apiFolder%" rmdir /S /Q "%apiFolder%"
if exist "%publishFolder%" rmdir /S /Q "%publishFolder%"
if exist "%publishOfflineFolder%" rmdir /S /Q "%publishOfflineFolder%"

dotnet tool update -g docfx-plus
if %ERRORLEVEL% neq 0 goto :Exit

docfx-plus %srcFolder%\docfx.json -o "%publishFolder%"
if %ERRORLEVEL% neq 0 goto :Exit
set /a publishedCount+=1
set published[!publishedCount!]=Published "Docs WebSite" to "%publishFolder%" folder.

docfx-plus build %srcFolder%\docfx.json -m _enableOfflineMode -o "%publishOfflineFolder%"
if %ERRORLEVEL% neq 0 goto :Exit
set /a publishedCount+=1
set published[!publishedCount!]=Published "Docs Offline" to "%publishOfflineFolder%" folder.


:Exit
echo:
echo ----------------------------------------------------

for /l %%i in (1, 1, %publishedCount%) do (
  echo %%i. !published[%%i]!
)
echo:

if %ERRORLEVEL% neq 0 (
  echo Script stopped with error!
) else (
  echo Script completed successfully.
)

echo ----------------------------------------------------
echo:

docfx-plus serve "%publishFolder%"

pause