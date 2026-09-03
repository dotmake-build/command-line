![DotMake Command-Line Logo](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/logo-wide.svg "DotMake Command-Line Logo")

# DotMake Command-Line

System.CommandLine is a very good parser but you need a lot of boilerplate code to get going and the API is hard to discover.
This becomes complicated to newcomers and also you would have a lot of ugly code in your `Program.cs` to maintain. 
What if you had an easy class-based layer combined with a good parser?

DotMake.CommandLine is a library which provides declarative syntax for 
[System.CommandLine](https://github.com/dotnet/command-line-api) 
via attributes for easy, fast, strongly-typed (no reflection) usage. The library includes a source generator 
which automagically converts your classes to CLI commands and properties to CLI options or CLI arguments. 
Supports 
[trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained), 
[AOT compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) and
[dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)!

[![Nuget](https://img.shields.io/nuget/v/DotMake.CommandLine?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/DotMake.CommandLine)

![DotMake Command-Line Intro](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/intro.gif "DotMake Command-Line Intro")

![DotMake Command-Line Themes](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/themes.gif "DotMake Command-Line Themes")
