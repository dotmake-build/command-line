@echo off

set projectName=TestApp
set srcFolder=..\src
set publishFolder=..\publish

::To force source-generator to reload the changed DLL
dotnet build-server shutdown

for %%f in (
  net472
  net8.0
) do (
  setlocal EnableDelayedExpansion
  set outputFolder=%publishFolder%\%projectName%-%%f
  
  dotnet clean %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --framework %%f --output !outputFolder!
  
  dotnet publish %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --framework %%f --output !outputFolder!
  
  if %ERRORLEVEL% EQU 0 (
    echo:
    echo *************
    echo Published "%projectName%" should be found in "!outputFolder!" folder.
    echo *************
    echo:
  )
)

pause