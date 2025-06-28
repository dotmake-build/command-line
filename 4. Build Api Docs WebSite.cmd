@echo off

set projectName=DotMake.CommandLine
set publishFolder=.\docs\api

dotnet build src\HelpBuilder\%projectName%.shfbproj --configuration Release --output %publishFolder%

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