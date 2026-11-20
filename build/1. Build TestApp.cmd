@echo off
setlocal enabledelayedexpansion

set projectName=TestApp
set srcFolder=..\src
set publishFolder=..\publish
set publishedCount=0

::To force source-generator to reload the changed DLL
dotnet build-server shutdown

for %%f in (
  net472
  net10.0
) do (
  setlocal EnableDelayedExpansion
  set outputFolder=%publishFolder%\%projectName%-%%f
  
  dotnet clean %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --framework %%f --output !outputFolder!
  if %ERRORLEVEL% neq 0 goto :Exit
  
  dotnet publish %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --framework %%f --output !outputFolder!
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