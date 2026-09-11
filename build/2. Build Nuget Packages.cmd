@echo off
setlocal enabledelayedexpansion

set srcFolder=..\src
set publishFolder=..\publish
set publishedCount=0

for %%f in (
  DotMake.CommandLine
) do (
  setlocal EnableDelayedExpansion
  set projectName=%%f
  
  dotnet pack %srcFolder%\!projectName!\!projectName!.csproj --configuration Release --output %publishFolder%
  if %ERRORLEVEL% neq 0 goto :Exit
  set /a publishedCount+=1
  set published[!publishedCount!]=Published "!projectName!.X.X.X.nupkg" to "%publishFolder%" folder.
)


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

pause