@echo off

set projectName=TestApp
set publishFolder=.\publish

for %%f in (
  net472
  net8.0
) do (
  setlocal EnableDelayedExpansion
  set outputFolder=%publishFolder%\%projectName%-%%f
  
  dotnet publish src\%projectName%\%projectName%.csproj --configuration Release --framework %%f --output !outputFolder!
  
  if %ERRORLEVEL% EQU 0 (
    echo:
    echo *************
    echo Published "%projectName%" should be found in "!outputFolder!" folder.
    echo *************
    echo:
  )
)

pause