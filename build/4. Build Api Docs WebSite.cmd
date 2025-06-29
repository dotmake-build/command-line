@echo off

set projectName=DotMake.CommandLine
set srcFolder=..\src
set publishFolder=..\docs\api

dotnet build %srcFolder%\HelpBuilder\%projectName%.shfbproj --configuration Release --output %publishFolder%

@echo off
if %ERRORLEVEL% EQU 0 (
  echo:
  echo *************
  echo Generated "Api Docs WebSite" should be found in "%publishFolder%" folder.
  echo *************
  echo:
)
@echo on

@pause