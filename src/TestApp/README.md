# TestApp

TestApp for debugging purpose, has project references to DotMake.CommandLine and DotMake.CommandLine.SourceGeneration.

Uses `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>`:
Generated files output path, excluding from compile and cleaning before build, are all handled in Directory.Build.targets.

EmitCompilerGeneratedFiles is useful for debugging, it doesn't work for live editing (VS continues to run the cached 
source generator, needs restart as it uses VBCSCompiler.exe for Roslyn analyzers/generators) but when project is 
explicitly built (probably VS runs msbuild externally in that case), the latest source generator DLL is used to 
generate files (the in-memory generated files in VS Analyzers node is still not updated but at least we have files on disk).
CompilerGeneratedFilesOutputPath defaults to `obj\Debug\net6.0\generated` but we set a folder inside project to easily
examine the files (`GeneratedFiles\$(TargetFramework)`), however we exclude the files from Compile (and add to None) to prevent class conflict errors.
We also use CleanSourceGeneratedFiles target to clean all generated files before build so that we don't have 
outdated files (e.g. for renamed classes).
