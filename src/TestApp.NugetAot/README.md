# TestApp.NugetAot

TestApp for release, has reference to latest DotMake.CommandLine NuGet package.
Used for testing trimming and AOT compilation.

## Trim self-contained deployment

- Use `PublishTrimmed.pubxml` profile from Build -> Publish Selection menu in Visual Studio.
  
- Or use this command in project folder:
  ```
  dotnet publish -p:PublishProfile=PublishTrimmed
  ```

- Or use this command in project folder:
  ```
  dotnet publish -c Release -r win-x64 -f net8.0 -p:PublishTrimmed=true;PublishSingleFile=true
  ```

- The output will be generated in `bin\Release\net8.0\win-x64\publish-trimmed` folder.
  We fix `$(PublishDir)` if not set, in .csproj file.

## Native AOT deployment

- Use `PublishAot.pubxml` profile from Build -> Publish Selection menu in Visual Studio.
  
- Or use this command in project folder:
  ```
  dotnet publish -p:PublishProfile=PublishAot
  ```

- Or use this command in project folder:
  ```
  dotnet publish -c Release -r win-x64 -f net8.0 -p:PublishAot=true
  ```

- The output will be generated in `bin\Release\net8.0\win-x64\publish-aot` folder.
  We fix `$(PublishDir)` if not set, in .csproj file.

Note that for AOT, you need to make sure to install the "Desktop development with C++" workload from
Visual Studio 2022 installer. Without it you may see errors like error : Platform linker not found 
Installer may take a while, for example it may seem stuck at Microsoft.VisualCpp.Redist14 but
wait a few minutes and it will complete.

## References

https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained
https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot
https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish#pubxml-files
