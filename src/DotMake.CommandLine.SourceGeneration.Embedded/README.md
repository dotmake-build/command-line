# DotMake.CommandLine.SourceGeneration.Embedded

This project contains .cs files that are embedded as resources in DotMake.CommandLine.SourceGeneration project, 
this project is used for checking compile errors.

Ensure that the .Embedded project is built after DotMake.CommandLine project only for checking compile errors,
otherwise .cs files are only embedded as resource in DotMake.CommandLine.SourceGeneration.dll

Make sure it compiles for lowest supported LangVersion 9.0 as source may be generated in a netstandard2.0 project.
- `ModuleInitializerAttribute` requires LangVersion 9.0 (polyfill injected only when attribute does not exist).  
  If your target framework is below net5.0, you need `<LangVersion>9.0</LangVersion>` tag (minimum) in your .csproj file.
- `RequiredMemberAttribute` requires LangVersion 11.0 (polyfill injected only when 11+ and attribute does not exist).  
  If your target framework is below net7.0, you need `<LangVersion>11.0</LangVersion>` tag (minimum) in your .csproj file
- `CliServiceProviderExtensions` feature injected only when project references `Microsoft.Extensions.DependencyInjection.Abstractions (>= 2.1.1)`.  
  Although `IServiceProvider` is in `System.ComponentModel` assembly, 
  used class `ActivatorUtilities` is in `Microsoft.Extensions.DependencyInjection.Abstractions` assembly.
- `CliServiceCollectionExtensions` feature injected only when project references `Microsoft.Extensions.DependencyInjection (>= 2.1.1)`.  
  Although `ServiceCollection` is in `Microsoft.Extensions.DependencyInjection.Abstractions` assembly, 
  used method `ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(IServiceCollection)` is in `Microsoft.Extensions.DependencyInjection` assembly.
