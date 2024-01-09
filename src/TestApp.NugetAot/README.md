# TestApp.NugetAot

TestApp for release, has reference to latest DotMake.CommandLine NuGet package.
Used for testing trimming and AOT compilation.

Use PublishTrimmed.pubxml profile when publishing to test "Trim self-contained deployment", results will be in `bin\Publish\trimmed`
https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained

Use PublishAot.pubxml profile when publishing to test "Native AOT deployment", results will be in `bin\Publish\aot`
https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net7%2Cwindows#limitations-of-native-aot-deployment
      
Note that for AOT, you need to make sure to install the "Desktop development with C++" workload from
Visual Studio 2022 installer. Without it you may see errors like error : Platform linker not found 
