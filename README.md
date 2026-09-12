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

## Getting started

Install the library to your console app project with  [NuGet](https://www.nuget.org/).

In your project directory, via dotnet cli:
```console
dotnet add package DotMake.CommandLine
```
or in your `.csproj` project file:
```xml
<PackageReference Include="DotMake.CommandLine" Version="x.y.z" />
```
where `x.y.z` can be replaced by ![NuGet Version](https://img.shields.io/nuget/v/DotMake.CommandLine?style=social&label=Latest)

### Prerequisites

- .NET 8.0 and later project or .NET Standard 2.0 and later project.  
  Note that .NET Framework 4.7.2+ or .NET Core 2.0 to .NET 7.0 projects can reference our netstandard2.0 target (automatic in nuget).  
  If your target framework is below net5.0, you also need `<LangVersion>9.0</LangVersion>` tag (minimum) in your .csproj file.
- Visual Studio 2022 v17.3+ or .NET SDK 6.0.407+ (when building via `dotnet` cli).  
  Our incremental source generator requires performance features added first in these versions.
- Usually a console app project but you can also use a class library project which will be consumed later.  

## Usage

DotMake.CommandLine offers 2 models: class-based model and delegate-based model.
Delegate-based model is useful for simple apps, for more complex apps, you should use the class-based model 
because you can have sub-commands and command inheritance.

### Class-based model

**Create a CLI App with DotMake.Commandline in seconds!**

In `Program.cs`, define your root command and run it with `Cli.Run`:
```c#
using System;
using DotMake.CommandLine;

// Run your CLI app:
Cli.Run<RootCliCommand>(args);

// Define your root command:
[CliCommand(Description = "A root cli command")]
public class RootCliCommand
{
    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; } = "DefaultForArgument1";

    [CliOption(Description = "Description for Option1")]
    public bool Option1 { get; set; };
 
    public void Run()
    {
        //When this method, i.e. your command's handler is run, the properties of your class 
        //will be already populated and bound from values passed in the command-line.

        Console.WriteLine($"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine($"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine();
    }
}
```
That's it! You now have a fully working command-line app.
You just pass the name of your class which represents your root command and `args` to `Cli.Run<>` method and everything is wired.
DotMake.CommandLine turns your class and its properties into a CLI command, options, and arguments at compile time 
and parses `args` and binds values to your class properties at runtime.

> `args` is the string array typically passed to a program. This is usually
the special variable `args` available in `Program.cs` (new style with top-level statements)
or the string array passed to the program's `Main` method (old style).
We also have method signatures which does not require `args`, 
for example you can also call `Cli.Run<RootCliCommand>()` and in that case `args` will be retrieved automatically from the current process via `Cli.GetArgs()`.

If you want to go async, just use this:
```c#
await Cli.RunAsync<RootCliCommand>(args);
```

When you run the app via 
- `TestApp.exe -?` in project output path (e.g. in `TestApp\bin\Debug\net8.0`)
- or `dotnet run -- -?` in project directory (e.g. in `TestApp`) (note the double hyphen/dash which allows `dotnet run` to pass arguments to our actual application)

You see this usage help:
```console
DotMake Command-Line TestApp v3.7.0
Copyright © 2023-2026 DotMake

A root cli command

Usage:
  TestApp [<argument-1>] [options]

Arguments:
  <argument-1>  Description for Argument1 [default: DefaultForArgument1]

Options:
  -o, --option-1  Description for Option1 [default: False]
  -?, -h, --help  Show help and usage information
  -v, --version   Show version information
```

Note, how CLI header, command/directive/option/argument names, descriptions and default values are automatically populated.
By default, it uses [POSIX conventions](https://learn.microsoft.com/en-us/dotnet/standard/commandline/design-guidance) for 
transforming your class and property names but this can be changed/overridden via settings.
DotMake.CommandLine will also smartly auto-generate short-from aliases for commands and options, for you
(see [Help output](https://dotmake.build/command-line/articles/help-output.html) docs for more info).
This way you can concentrate on the logic of your CLI App and let DotMake.CommandLine handle all the boilerplate.

Defining sub-commands (multi command CLI App) or sharing options between different commands, is a breeze in DotMake.CommandLine
(see [Commands](https://dotmake.build/command-line/articles/commands.html) docs for more info).


To handle exceptions, you just use a try-catch block:
```c#
try
{
    Cli.Run<RootCliCommand>(args);
}
catch (Exception e)
{
    Console.WriteLine(@"Exception in main: {0}", e.Message);
}
```
System.CommandLine, by default overtakes your exceptions that are thrown in command handlers
(even if you don't set an exception handler explicitly) but DotMake.CommandLine, by default allows
the exceptions to pass through. However if you wish, you can easily use the default exception handler
by passing a `CliSettings` instance like below. Default exception handler prints the exception in red color to console:
```c#
Cli.Run<RootCliCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });
```
If you need to simply parse the command-line arguments without invocation, use this:
```c#
var result = Cli.Parse<RootCliCommand>(args);
var rootCliCommand = result.Bind<RootCliCommand>();
```
If you need to examine the parse result, such as errors:
```c#
var result = Cli.Parse<RootCliCommand>(args);
if (result.ParseResult.Errors.Count > 0)
{

}
```
(see [Model binding](https://dotmake.build/command-line/articles/model-binding.html) docs for more info).

#### Summary
- Mark the class with `[CliCommand]` attribute to make it a CLI command 
  (see [CliCommandAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliCommandAttribute.html) 
  and [Commands](https://dotmake.build/command-line/articles/commands.html) docs for more info).
- Mark a property with `[CliOption]` attribute to make it a CLI option 
  (see [CliOptionAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliOptionAttribute.html) 
  and [Options](https://dotmake.build/command-line/articles/options.html) docs for more info).
- Mark a property with `[CliArgument]` attribute to make it a CLI argument 
  (see [CliArgumentAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliArgumentAttribute.html) 
  and [Arguments](https://dotmake.build/command-line/articles/arguments.html) docs for more info).
- Add a method with name `Run` or `RunAsync` to make it the handler for the CLI command. The method can have one of the following signatures: 
  
  - 
    ```c#
    void Run()
    ```
  - 
    ```c#
    int Run()
    ```
  - 
    ```c#
    async Task RunAsync()
    ```
  - 
    ```c#
    async Task<int> RunAsync()
    ```

  Optionally the method signature can have a `CliContext` parameter in case you need to access it:
  
  - 
    ```c#
    Run(CliContext context)
    ```
  - 
    ```c#  
    RunAsync(CliContext context)
    ```
  
  We also provide interfaces `ICliRun`, `ICliRunWithReturn`, `ICliRunWithContext`, `ICliRunWithContextAndReturn`
  and async versions `ICliRunAsync`, `ICliRunAsyncWithReturn`, `ICliRunAsyncWithContext`, `ICliRunAsyncWithContextAndReturn` 
  that you can inherit in your command class.
  Normally you don't need an interface for a handler method as the source generator can detect it automatically,
  but the interfaces can be used to prevent your IDE complain about unused method in class.

  The signatures which return int value, sets the ExitCode of the app.
  If no handler method is provided, then by default it will show help for the command.
  This can be also controlled manually by `ShowHelp()` method of `CliContext`.
  Other methods `ShowValues()` and `ShowHierarchy()` are also useful.
- Call `Cli.Run<>` or `Cli.RunAsync<>` method with your class name to run your CLI app 
  (see [Cli.Run](https://dotmake.build/command-line/api/DotMake.CommandLine.Cli.Run.html),
  [Cli.RunAsync](https://dotmake.build/command-line/api/DotMake.CommandLine.Cli.RunAsync.html) 
  and [Model binding](https://dotmake.build/command-line/articles/model-binding.html) docs for more info).
- For best practice, create a subfolder named `Commands` in your project and put your command classes there 
  so that they are easy to locate and maintain in the future.


### Delegate-based model

Create a CLI App with DotMake.Commandline in seconds!

In `Program.cs`, add this simple code:
```c#
using System;
using DotMake.CommandLine;

Cli.Run(([CliArgument]string arg1, bool opt1) =>
{
    Console.WriteLine($"Value for {nameof(arg1)} parameter is '{arg1}'");
    Console.WriteLine($"Value for {nameof(opt1)} parameter is '{opt1}'");
});
```
And that's it! You now have a fully working command-line app.

#### Summary
- Pass a delegate (a parenthesized lambda expression or a method reference) which has parameters that represent your options and arguments, to `Cli.Run<>` or `Cli.RunAsync<>` method
  (see [Cli.Run](https://dotmake.build/command-line/api/DotMake.CommandLine.Cli.Run.html),
  [Cli.RunAsync](https://dotmake.build/command-line/api/DotMake.CommandLine.Cli.RunAsync.html) 
  and [Model binding](https://dotmake.build/command-line/articles/model-binding.html) docs for more info).
- A parameter is by default considered as a CLI option but you can;
  - Mark a parameter with `[CliArgument]` attribute to make it a CLI argument and specify settings 
    (see [CliArgumentAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliArgumentAttribute.html) 
    and [Arguments](https://dotmake.build/command-line/articles/arguments.html) docs for more info).
  - Mark a parameter with `[CliOption]` attribute to specify CLI option settings 
    (see [CliOptionAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliOptionAttribute.html) 
    and [Options](https://dotmake.build/command-line/articles/options.html) docs for more info).
  - Mark the delegate itself with `[CliCommand]` attribute to specify CLI command settings 
    (see [CliCommandAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliCommandAttribute.html) 
    and [Commands](https://dotmake.build/command-line/articles/commands.html) docs for more info).
  - Note that for being able to mark a parameter with an attribute in an anonymous lambda function, 
    if your target framework is below net6.0, you also need `<LangVersion>10.0</LangVersion>` tag (minimum) in your .csproj file.
- Set a default value for a parameter if you want it to be optional (not required to be specified on the command-line).
- Your delegate can be `async`.
- Your delegate can have a return type `void` or `int` and if it's async `Task` or `Task<int>`.

## Building

We provide some `.cmd` batch scripts in `build` folder for easier building:
```console
1. Build TestApp.cmd
2. Build Nuget Packages.cmd
2.1. Build TestApp.Nuget.cmd
2.2. Build TestApp.NugetDI.cmd
2.3. Build TestApp.NugetAot.cmd
3. Build Docs WebSite.cmd         
```

Output results can be found in `publish` folder, for example:
```console
TestApp-net472
TestApp-net8.0

DotMake.CommandLine.3.7.0.nupkg

TestApp.Nuget-net472
TestApp.Nuget-net8.0

TestApp.NugetDI-net472
TestApp.NugetDI-net8.0

TestApp.NugetAot-win-x64-native
TestApp.NugetAot-win-x64-trimmed

Docs-WebSite
Docs-Offline
```

## Links

- [DotMake Command-Line Documentation](https://dotmake.build/command-line/)
- [DotMake Command-Line API Reference](https://dotmake.build/command-line/api/)
- [Command-line syntax overview for System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax)
- [Release Notes](https://github.com/dotmake-build/command-line/releases)
- [NuGet Package](https://www.nuget.org/packages/DotMake.CommandLine)
