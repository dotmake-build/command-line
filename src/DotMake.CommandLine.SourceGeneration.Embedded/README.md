# DotMake.CommandLine.SourceGeneration.Embedded

This project contains .cs files that are embedded as resources in DotMake.CommandLine.SourceGeneration project, 
this project is used for checking compile errors.

Code should compile against DotMake.CommandLine.dll (not project due to circular dependency),
which will be available during runtime.

Make sure it compiles for lowest supported langversion 7.3 as source may be generated in a netstandard2.0 project.
