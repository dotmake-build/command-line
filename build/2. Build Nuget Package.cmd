@echo off

set projectName=DotMake.CommandLine
set srcFolder=..\src
set publishFolder=..\publish

dotnet pack %srcFolder%\%projectName%\%projectName%.csproj --configuration Release --output %publishFolder%

@echo off
if %ERRORLEVEL% EQU 0 (
  echo:
  echo *************
  echo Generated "%projectName%.X.X.X.nupkg" should be found in "%publishFolder%" folder.
  echo *************
  echo:
)
@echo on

@pause