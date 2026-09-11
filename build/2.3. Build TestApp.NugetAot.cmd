@echo off
setlocal enabledelayedexpansion

set projectName=TestApp.NugetAot
set srcFolder=..\src
set publishFolder=..\publish
set publishedCount=0

for %%r in (
  win-x64
) do (
  setlocal EnableDelayedExpansion
  set outputFolder=%publishFolder%\%projectName%-%%r-trimmed
  
  dotnet publish %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --runtime %%r -p:PublishTrimmed=true;PublishSingleFile=true --output !outputFolder!
  if %ERRORLEVEL% neq 0 goto :Exit
  set /a publishedCount+=1
  set published[!publishedCount!]=Published "%projectName%" to "!outputFolder!" folder.
  
  set outputFolder=%publishFolder%\%projectName%-%%r-native
  
  dotnet publish %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --runtime %%r -p:PublishAot=true --output !outputFolder!
  if %ERRORLEVEL% neq 0 goto :Exit
  set /a publishedCount+=1
  set published[!publishedCount!]=Published "%projectName%" to "!outputFolder!" folder.
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