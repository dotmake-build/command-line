# Deployment

## Trim self-contained deployment

Use this command in your CLI App's project folder, e.g. for `win-x64` platform:
```console
dotnet publish -c Release -r win-x64 -p:PublishTrimmed=true;PublishSingleFile=true
```
If you have multiple `TargetFrameworks` in the project use this:
```console
dotnet publish -c Release -r win-x64 -f net8.0 -p:PublishTrimmed=true;PublishSingleFile=true;TargetFrameworks=net8.0
```

The output will be generated in `bin\Release\net8.0\win-x64\publish` folder.
You will get a trimmed and single executable file which does not require .NET runtime (e.g. a 12MB exe file).
[More details on trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained).

## Native AOT deployment

Use this command in your CLI App's project folder, e.g. for `win-x64` platform:
```console
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```
If you have multiple `TargetFrameworks` in the project use this:
```console
dotnet publish -c Release -r win-x64 -f net8.0 -p:PublishAot=true;TargetFrameworks=net8.0
```

The output will be generated in `bin\Release\net8.0\win-x64\publish` folder.
You will get a native and single executable file for the platform, which runs blazingly fast (e.g. a 4MB exe file).

Note that for AOT, you need to make sure to install the "Desktop development with C++" workload from
Visual Studio 2022 installer. Without it you may see errors like error `Platform linker not found`.
Installer may take a while, for example it may seem stuck at Microsoft.VisualCpp.Redist14 but
wait a few minutes and it will complete.
[More details on AOT compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot).
