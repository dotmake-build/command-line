@echo off

set projectName=TestApp.NugetAot
set srcFolder=..\src
set publishFolder=..\publish

for %%r in (
  win-x64
) do (
  setlocal EnableDelayedExpansion
  set outputFolder=%publishFolder%\%projectName%-%%r-trimmed
  
  dotnet publish %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --runtime %%r -p:PublishTrimmed=true;PublishSingleFile=true --output !outputFolder!
  
  if %ERRORLEVEL% EQU 0 (
    echo:
    echo *************
    echo Published "%projectName%" should be found in "!outputFolder!" folder.
    echo *************
    echo:
  )
  
  
  set outputFolder=%publishFolder%\%projectName%-%%r-native
  
  dotnet publish %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --runtime %%r -p:PublishAot=true --output !outputFolder!
  
  if %ERRORLEVEL% EQU 0 (
    echo:
    echo *************
    echo Published "%projectName%" should be found in "!outputFolder!" folder.
    echo *************
    echo:
  )
)

pause