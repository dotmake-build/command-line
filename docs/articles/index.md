![DotMake Command-Line Logo](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/logo-wide.svg "DotMake Command-Line Logo")

# DotMake Command-Line

Build modern .NET command-line applications quickly and easily using strongly-typed C#.

DotMake.CommandLine lets you define commands, options, and arguments using
classes, properties, and attributes. Its source generator handles the CLI
plumbing at compile time — with no runtime reflection.

Built on top of [System.CommandLine](https://github.com/dotnet/command-line-api), 
DotMake.CommandLine provides an attribute-based, source-generated programming model for building CLIs in idiomatic C#.
System.CommandLine is a very good parser but you need a lot of boilerplate code to get going and the API is hard to discover.
This becomes complicated to newcomers and also you would have a lot of ugly code in your `Program.cs` to maintain. 
What if you had an easy class-based layer combined with a good parser?

DotMake.CommandLine supports 
[trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained), 
[AOT compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) and
[dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
out of the box!

[![Nuget](https://img.shields.io/nuget/v/DotMake.CommandLine?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/DotMake.CommandLine)

![DotMake Command-Line Intro](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/intro.gif "DotMake Command-Line Intro")

![DotMake Command-Line Themes](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/themes.gif "DotMake Command-Line Themes")
