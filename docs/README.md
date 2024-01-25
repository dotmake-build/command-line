![DotMake Command-Line Logo](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/logo-wide.png "DotMake Command-Line Logo")

# DotMake Command-Line

System.CommandLine is a very good parser but you need a lot of boilerplate code to get going and the API is hard to discover.
This becomes complicated to newcomers and also you would have a lot of ugly code in your `Program.cs` to maintain. 
What if you had an easy class-based layer combined with a good parser?

DotMake.CommandLine is a library which provides declarative syntax for 
[System.CommandLine](https://github.com/dotnet/command-line-api) 
via attributes for easy, fast, strongly-typed (no reflection) usage. The library includes includes a source generator 
which automagically converts your classes to CLI commands and properties to CLI options or CLI arguments. 
Supports 
[trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained), 
[AOT compilation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) and
[dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)!

[![Nuget](https://img.shields.io/nuget/v/DotMake.CommandLine?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/DotMake.CommandLine)

## Getting started

Install the library to your console app project with  [NuGet](https://www.nuget.org/).

In your project directory, via dotnet cli:
```console
dotnet add package DotMake.CommandLine
```
or in Visual Studio Package Manager Console:
```console
PM> Install-Package DotMake.CommandLine
```

### Prerequisites

- .NET 6.0 and later project or .NET Standard 2.0 and later project (note that .NET Framework 4.7.2+ can reference netstandard2.0 libraries).
  If your target framework is below net5.0, you also need `<LangVersion>9.0</LangVersion>` tag (minimum) in your .csproj file.
- Visual Studio 2022 v17.3+ or .NET SDK 6.0.407+ (our incremental source generator requires performance features added first in these versions).
- Usually a console app project but you can also use a class library project which will be consumed later.

## Usage

Create a CLI App with DotMake.Commandline in seconds!
In Program.cs, add this simple code:
```c#
Cli.Run(([CliArgument]string argument1, bool option1) =>
{
    Console.WriteLine($@"Value for {nameof(argument1)} parameter is '{argument1}'");
    Console.WriteLine($@"Value for {nameof(option1)} parameter is '{option1}'");
});
```
And that's it! You now have a fully working command-line app.

#### Summary
- Pass a delegate (a parenthesized lambda expression or a method reference) which has parameters that represent your options and arguments, to `Cli.Run`.
- A parameter is by default considered as a CLI option but you can;
  - Mark a parameter with `CliArgument` attribute to make it a CLI argument and specify settings (see [CliArgumentAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliArgumentAttribute.htm) docs for more info).
  - Mark a parameter with `CliOption` attribute to specify CLI option settings (see [CliOptionAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliOptionAttribute.htm) docs for more info).
  - Mark the delegate itself with `CliCommand` attribute to specify CLI command settings (see [CliCommandAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliCommandAttribute.htm) docs for more info).
  - Note that for being able to mark a parameter with an attribute in an anonymous lambda function, 
    if your target framework is below net6.0, you also need `<LangVersion>10.0</LangVersion>` tag (minimum) in your .csproj file.
- Set a default value for a parameter if you want it to be optional (not required to be specified on the command-line).
- Your delegate can be `async`.
- Your delegate can have a return type `void` or `int` and if it's async `Task` or `Task<int>`.

### Class-based model

While delegate-based model above is useful for simple apps, for more complex apps, you can use the class-based model.
Create a simple class like this:

```c#
using System;
using DotMake.CommandLine;

[CliCommand(Description = "A root cli command")]
public class RootCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";
 
    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }
 
    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }
}
```
In Program.cs, add this single line:
```c#
Cli.Run<RootCliCommand>(args);
```
And that's it! You now have a fully working command-line app. You just specify the name of your class which represents your root command to `Cli.Run<>` method and everything is wired.

> `args` is the string array typically passed to a program. This is usually
the special variable `args` available in `Program.cs` (new style with top-level statements)
or the string array passed to the program's `Main` method (old style).
We also have method signatures which does not require `args`, 
for example you can also call `Cli.Run<RootCliCommand>()` and in that case `args` will be retrieved automatically from the current process via `Cli.GetArgs()`.

If you want to go async, just use this:
```c#
await Cli.RunAsync<RootCliCommand>(args);
```
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
System.CommandLine, by default overtakes your exceptions that are thrown in command handlers (even if you don't set an exception handler explicitly) but DotMake.CommandLine, by default allows the exceptions to pass through. However if you wish, you can easily use an exception handler by using `configureBuilder` delegate parameter like this:
```c#
Cli.Run<RootCliCommand>(args, builder => 
    builder.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
);
```
If you need to simply parse the command-line arguments without invocation, use this:
```c#
var rootCliCommand = Cli.Parse<RootCliCommand>(args);
```
If you need to examine the parse result, such as errors:
```c#
var rootCliCommand = Cli.Parse<RootCliCommand>(args, out var parseResult);
if (parseResult.Errors.Count > 0)
{

}
```
#### Summary
- Mark the class with `CliCommand` attribute to make it a CLI command (see [CliCommandAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliCommandAttribute.htm) docs for more info).
- Mark a property with `CliOption` attribute to make it a CLI option (see [CliOptionAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliOptionAttribute.htm) docs for more info).
- Mark a property with `CliArgument` attribute to make it a CLI argument (see [CliArgumentAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliArgumentAttribute.htm) docs for more info).
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
  Optionally the method signature can have a System.CommandLine.Invocation.InvocationContext parameter in case you need to access it:
  
  - 
    ```c#
    Run(InvocationContext context)
    ```
  -
    ```c#  
    RunAsync(InvocationContext context)
    ```

  The signatures which return int value, sets the ExitCode of the app.
  If no handler method is provided, then by default it will show help for the command.
  This can be also controlled manually by extension method `ShowHelp` in `InvocationContext`.
  Other extension methods `IsEmptyCommand` and `ShowValues` are also useful.
- Call `Cli.Run<>` or`Cli.RunAsync<>` method with your class name to run your CLI app (see [Cli](https://dotmake.build/api/html/T_DotMake_CommandLine_Cli.htm) docs for more info).

## Help output

When you run the app via 
- `TestApp.exe -?` in project output path (e.g. in `TestApp\bin\Debug\net6.0`)
- or `dotnet run -- -?` in project directory (e.g. in `TestApp`) (note the double hyphen/dash which allows `dotnet run` to pass arguments to our actual application)

- You see this usage help:
```console
DotMake Command-Line TestApp v1.6.0
Copyright © 2023-2024 DotMake

A root cli command

Usage:
  TestApp <argument-1> [options]

Arguments:
  <argument-1>  Description for Argument1 [required]

Options:
  -o, --option-1 <option-1>  Description for Option1 [default: DefaultForOption1]
  -v, --version              Show version information
  -?, -h, --help             Show help and usage information
```
First line comes from `AssemblyProductAttribute` or `AssemblyName`. 
Version comes from `AssemblyInformationalVersionAttribute` or `AssemblyFileVersionAttribute` or `AssemblyVersionAttribute`.
Second line comes from `AssemblyCopyrightAttribute`.
Third line comes from `CliCommand.Description` or `AssemblyDescriptionAttribute`.

Note, how command/option/argument names, descriptions and default values are automatically populated.

By default,  command/option/argument names are generated as follows;
- First the following suffixes are stripped out from class and property names:
    - For commands:
      "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli"
    - For options: 
     "RootCommandOption", "SubCliCommandOption", "SubCommandOption", "CliCommandOption", "CommandOption", "CliOption", "Option"
    - For arguments: 
    "RootCliCommandArgument", "RootCommandArgument", "SubCliCommandArgument", "SubCommandArgument", "CliCommandArgument", "CommandArgument", "CliArgument", "Argument"
    
- Then the names are converted to **kebab-case**, this can be changed by setting `NameCasingConvention`  property of the `CliCommand` attribute to one of the following values:
  - `CliNameCasingConvention.None`
  - `CliNameCasingConvention.LowerCase`
  - `CliNameCasingConvention.UpperCase`
  - `CliNameCasingConvention.TitleCase`
  - `CliNameCasingConvention.PascalCase`
  - `CliNameCasingConvention.CamelCase`
  - `CliNameCasingConvention.KebabCase`
  - `CliNameCasingConvention.SnakeCase`
  
- For options, double hyphen/dash prefix is added to the name (e.g. `--option`), this can be changed by setting `NamePrefixConvention`  (default: DoubleHyphen) property of the `CliCommand` attribute to one of the following values:
  - `CliNamePrefixConvention.SingleHyphen`
  - `CliNamePrefixConvention.DoubleHyphen`
  - `CliNamePrefixConvention.ForwardSlash`
  
- For options, short-form alias with first letter (e.g. `-o`) is automatically added. This can be changed by setting `ShortFormAutoGenerate` (default: true) and `ShortFormPrefixConvention` (default: SingleHyphen) properties of the `CliCommand` attribute.

---
For example, change the name casing and prefix convention:
```c#
using System;
using DotMake.CommandLine;
 
[CliCommand(
    Description = "A cli command with snake_case name casing and forward slash prefix conventions",
    NameCasingConvention = CliNameCasingConvention.SnakeCase,
    NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
    ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
)]
public class RootSnakeSlashCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";
 
    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }
 
    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }
}
```
When you run the app via `TestApp.exe -?` or `dotnet run -- -?`, you see this usage help:
```console
DotMake Command-Line TestApp v1.6.0
Copyright © 2023-2024 DotMake

A cli command with snake_case name casing and forward slash prefix conventions

Usage:
  TestApp <argument_1> [options]

Arguments:
  <argument_1>  Description for Argument1 [required]

Options:
  /o, /option_1 <option_1>  Description for Option1 [default: DefaultForOption1]
  /v, /version              Show version information
  -?, -h, /help             Show help and usage information
```
Note how even the default options `version` and `help` use the new prefix convention `ForwardSlash`. By the way, as `help` is a special option, which allows user to discover your app, we still add short-form aliases with other prefix to prevent confusion.

### Localization

Localizing commands, options and arguments is supported.
You can specify a `nameof` operator expression with a resource property (generated by resx) in the attribute's argument (for `string` types only)
and the source generator will smartly use the resource property accessor as the value of the argument so that it can localize at runtime.
If the property in the `nameof` operator expression does not point to a resource property, then the name of that property will be used as usual.

```c#
[CliCommand(Description = nameof(TestResources.CommandDescription))]
internal class LocalizedCliCommand
{
    [CliOption(Description = nameof(TestResources.OptionDescription))]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = nameof(TestResources.ArgumentDescription))]
    public string Argument1 { get; set; }

    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }
}
```

## Model binding

When the command handler is run, the properties for CLI options and arguments will be already populated 
and bound from values passed in the command-line. If no matching value is passed, the property will have its default value if
it has one or an error will be displayed if it's a required option/argument and it was not specified on the command-line.

An option/argument will be considered required when
- There is no property initializer and the property type is a reference type (e.g. `public string Arg { get; set; }`). 
  `string` is a reference type which has a null as the default value but `bool` and `enum` are value
  types which already have non-null default values. `Nullable<T>` is a reference type, e.g. `bool?`.
- There is a property initializer, but it's initialized with `null` or `null!` (SuppressNullableWarningExpression)
  (e.g. `public string Arg { get; set; } = null!;`).
- If it's forced via attribute property `Required` (e.g. `[CliArgument(Required = true)]`).
- If it's forced via `required` modifier (e.g. `public required string Opt { get; set; }`).
  Note that for being able to use `required` modifier, if your target framework is below net7.0, 
  you also need `<LangVersion>11.0</LangVersion>` tag (minimum) in your .csproj file (our source generator supplies the polyfills
  automatically as long as you set C# language version to 11).

An option/argument will be considered optional when
- There is no property initializer (e.g. `public bool Opt { get; set; }`) but the property type is a value type 
  which already have non-null default value.
- There is a property initializer, and it's not initialized with `null` or `null!` (SuppressNullableWarningExpression)
  (e.g. `public string Arg { get; set; } = "Default";`).
- If it's forced via attribute property `Required` (e.g. `[CliArgument(Required = false)]`).
---
When you run,
```console
TestApp.exe NewValueForArgument1
```
or (note the double hyphen/dash which allows `dotnet run` to pass arguments to our actual application):
```console
dotnet run -- NewValueForArgument1
```
You see this result:
```console
Handler for 'TestApp.Commands.RootCliCommand' is run:
Value for Option1 property is 'DefaultForOption1'
Value for Argument1 property is 'NewValueForArgument1'
```
---
### Supported types
Note that you can have a specific type (other than `string`) for a property which a `CliOption` or `CliArgument` attribute is applied to, for example these properties will be parsed and bound/populated automatically:
```c#
[CliCommand]
public class WriteFileCommand
{
    [CliArgument]
    public FileInfo OutputFile { get; set; }

    [CliOption]
    public List<string> Lines { get; set; }
}
```
The following types for properties are supported:
* Booleans (flags) - If `true` or `false` is passed for an option having a `bool` argument, it is parsed and bound as expected.
  But an option whose argument type is `bool` doesn't require an argument to be specified.
  The presence of the option token on the command line, with no argument following it, results in a value of `true`.
* Enums - The values are bound by name, and the binding is case insensitive
* Common CLR types:
  
  * `string`, `bool`
  * `FileSystemInfo`, `FileInfo`, `DirectoryInfo`
  * `int`, `long`, `short`, `uint`, `ulong`, `ushort`
  * `double`, `float`, `decimal`
  * `byte`, `sbyte`
  * `DateTime`, `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly`
  * `Guid`
  * `Uri`, `IPAddress`, `IPEndPoint`

* Any type with a public constructor or a static `Parse` method with a string parameter (other parameters, if any, should be optional) - These types can be bound/parsed 
  automatically even if they are wrapped with `Enumerable` or `Nullable` type.
    ```c#
    [CliCommand]
    public class ArgumentConverterCliCommand
    {
        [CliOption]
        public ClassWithConstructor Opt { get; set; }

        [CliOption(AllowMultipleArgumentsPerToken = true)]
        public ClassWithConstructor[] OptArray { get; set; }

        [CliOption]
        public CustomStruct? OptNullable { get; set; }

        [CliOption]
        public IEnumerable<ClassWithConstructor> OptEnumerable { get; set; }

        [CliOption]
        public List<ClassWithConstructor> OptList { get; set; }

        [CliOption]
        public CustomList<ClassWithConstructor> OptCustomList { get; set; }

        [CliArgument]
        public IEnumerable<ClassWithParser> Arg { get; set; }
    }

    public class ClassWithConstructor
    {
        private readonly string value;

        public ClassWithConstructor(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
    
    public class ClassWithParser
    {
        private string value;

        public override string ToString()
        {
            return value;
        }

        public static ClassWithParser Parse(string value)
        {
            var instance = new ClassWithParser();
            instance.value = value;
            return instance;
        }
    }

    public struct CustomStruct
    {
        private readonly string value;

        public CustomStruct(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
    ```
  
* Arrays, lists, collections - any type that implements `IEnumerable<T>` and has a public constructor with a `IEnumerable<T>` or `IList<T>` parameter (other parameters, if any, should be optional).
  If type is generic `IEnumerable<T>`, `IList<T>`, `ICollection<T>` interfaces itself, array `T[]` will be used.
  If type is non-generic `IEnumerable`, `IList`, `ICollection` interfaces itself, array `string[]` will be used.
    ```c#
    [CliCommand]
    public class EnumerableCliCommand
    {
        [CliOption]
        public IEnumerable<int> OptEnumerable { get; set; }

        [CliOption]
        public List<string> OptList { get; set; }

        [CliOption(AllowMultipleArgumentsPerToken = true)]
        public FileAccess[] OptEnumArray { get; set; }

        [CliOption]
        public Collection<string> OptCollection { get; set; }

        [CliOption]
        public HashSet<string> OptHashSet { get; set; }

        [CliOption]
        public Queue<FileInfo> OptQueue { get; set; }

        [CliOption]
        public CustomList<string> OptCustomList { get; set; }
 
        [CliArgument]
        public IList ArgIList { get; set; }
    }
 
    public class CustomList<T> : List<T>
    {
        public CustomList(IEnumerable<T> items)
            : base(items)
        {

        }
    }
    ```

## Dependency Injection

Commands can have injected dependencies, this is supported via `Microsoft.Extensions.DependencyInjection` package (version >= 6.0.0).
In your project directory, via dotnet cli:
```console
dotnet add package Microsoft.Extensions.DependencyInjection
```
or in Visual Studio Package Manager Console:
```console
PM> Install-Package Microsoft.Extensions.DependencyInjection
```
When the source generator detects that your project has reference to `Microsoft.Extensions.DependencyInjection`,
it will generate extension methods for supporting dependency injection.
For example, you can now add your services with the extension method `Cli.Ext.ConfigureServices`:
```c#
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<TransientClass>();
    services.AddScoped<ScopedClass>();
    services.AddSingleton<SingletonClass>();
});

Cli.Run<RootCliCommand>();
```
Then let them be injected to your command class automatically by providing a constructor with the required services:
```c#
[CliCommand(Description = "A root cli command with dependency injection")]
public class RootCliCommand
{
    private readonly TransientClass transientDisposable;
    private readonly ScopedClass scopedDisposable;
    private readonly SingletonClass singletonDisposable;

    public RootCliCommand(
        TransientClass transientDisposable,
        ScopedClass scopedDisposable,
        SingletonClass singletonDisposable
    )
    {
        this.transientDisposable = transientDisposable;
        this.scopedDisposable = scopedDisposable;
        this.singletonDisposable = singletonDisposable;
    }

    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();

        Console.WriteLine($"Instance for {transientDisposable.Name} is available");
        Console.WriteLine($"Instance for {scopedDisposable.Name} is available");
        Console.WriteLine($"Instance for {singletonDisposable.Name} is available");
        Console.WriteLine();
    }
}

public sealed class TransientClass : IDisposable
{
    public string Name => nameof(TransientClass);

    public void Dispose() => Console.WriteLine($"{nameof(TransientClass)}.Dispose()");
}

public sealed class ScopedClass : IDisposable
{
    public string Name => nameof(ScopedClass);

    public void Dispose() => Console.WriteLine($"{nameof(ScopedClass)}.Dispose()");
}

public sealed class SingletonClass : IDisposable
{
    public string Name => nameof(SingletonClass);

    public void Dispose() => Console.WriteLine($"{nameof(SingletonClass)}.Dispose()");
}
```

## Command Hierarchy

A *command* in command-line input is a token that specifies an action or defines a group of related actions. For example:

* In `dotnet run`, `run` is a command that specifies an action.
* In `dotnet tool install`, `install` is a command that specifies an action, and `tool` is a command that specifies a group of related commands. There are other tool-related commands, such as `tool uninstall`, `tool list`, and `tool update`.

### Root commands

The *root command* is the one that specifies the name of the app's executable. For example, the `dotnet` command specifies the *dotnet.exe* executable.

### Subcommands

Most command-line apps support *subcommands*, also known as *verbs*. For example, the `dotnet` command has a `run` subcommand that you invoke by entering `dotnet run`.

Subcommands can have their own subcommands. In `dotnet tool install`, `install` is a subcommand of `tool`.

---
Defining sub-commands in DotMake.Commandline is very easy. We simply use nested classes to create a hierarchy:
```c#
[CliCommand(Description = "A root cli command with nested children")]
public class RootWithNestedChildrenCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";
 
    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }
 
    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }
 
    [CliCommand(Description = "A nested level 1 sub-command")]
    public class Level1SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";
 
        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }
 
        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }
 
        [CliCommand(Description = "A nested level 2 sub-command")]
        public class Level2SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";
 
            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }
 
            public void Run()
            {
                Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                Console.WriteLine();
            }
        }
    }
}
```
Just make sure you apply `CliCommand` attribute to the nested classes as well.
Command hierarchy in above example is: **RootWithNestedChildrenCliCommand** -> **Level1SubCliCommand** -> **Level2SubCliCommand**

Another way to create hierarchy between commands, especially if you want to use standalone classes, is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class:
```c#
[CliCommand(Description = "A root cli command")]
public class RootWithExternalChildrenCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";
 
    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }
 
    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }
}

[CliCommand(
    Name = "Level1External",
    Description = "An external level 1 sub-command",
    Parent = typeof(RootWithExternalChildrenCliCommand)
)]
public class ExternalLevel1SubCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }

    [CliCommand(Description = "A nested level 2 sub-command")]
    public class Level2SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }
    }
}
```
Command hierarchy in above example is: **RootWithExternalChildrenCliCommand** -> **ExternalLevel1SubCliCommand** -> **Level2SubCliCommand**

---
The class that `CliCommand` attribute is applied to,
- will be a root command if the class is not a nested class and `Parent`property is not set.
- will be a sub command if the class is a nested class or `Parent` property is set.

The properties for `CliCommand` attribute (see [CliCommandAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliCommandAttribute.htm) docs for more info):
- Name
- Description
- Aliases
- Hidden
- Parent
- TreatUnmatchedTokensAsErrors
- NameCasingConvention *(inherited by child options, child arguments and subcommands)*
- NamePrefixConvention *(inherited by child options and subcommands)*
- ShortFormPrefixConvention *(inherited by child options and subcommands)*
- ShortFormAutoGenerate *(inherited by child options and subcommands)*

## Command Inheritance

When you have repeating/common options and arguments for your commands, you can define them once in a base class and then 
share them by inheriting that base class in other command classes. Interfaces are also supported !

```c#
[CliCommand]
public class InheritanceCliCommand : CredentialCommandBase, IDepartmentCommand
{
    public string Department { get; set; } = "Accounting";
}

public abstract class CredentialCommandBase
{
    [CliOption(Description = "Username of the identity performing the command")]
    public string Username { get; set; } = "admin";

    [CliOption(Description = "Password of the identity performing the command")]
    public string Password { get; set; }

    public void Run()
    {
        Console.WriteLine($@"I am {Username}");
    }
}

public interface IDepartmentCommand
{
    [CliOption(Description = "Department of the identity performing the command (interface)")]
    string Department { get; set; }
}
```

The property attribute and the property initializer from the most derived class in the hierarchy will be used 
(they will override the base ones). The command handler (Run or RunAsync) will be also inherited.
So in the above example, `InheritanceCliCommand` inherits options `Username`, `Password` from a base class and
option `Department` from an interface. Note that the property initializer for `Department` is in the derived class, 
so that default value will be used.

## Options

An option is a named parameter that can be passed to a command. [POSIX](https://en.wikipedia.org/wiki/POSIX) CLIs typically prefix the option name with two hyphens (`--`). The following example shows two options:

```dotnetcli
dotnet tool update dotnet-suggest --verbosity quiet --global
                                  ^---------^       ^------^
```

As this example illustrates, the value of the option may be explicit (`quiet` for `--verbosity`) or implicit (nothing follows `--global`). Options that have no value specified are typically Boolean parameters that default to `true` if the option is specified on the command line.

For some Windows command-line apps, you identify an option by using a leading slash (`/`) with the option name. For example:

```console
msbuild /version
        ^------^
```

Both POSIX and Windows prefix conventions are supported. When you configure an option, you specify the option name including the prefix.

---
When manually setting a name (overriding target property's name), you should specify the option name including the prefix (e.g. `--option`, `-option` or `/option`)

The properties for `CliOption` attribute (see [CliOptionAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliOptionAttribute.htm) docs for more info):
- Name
- Description
- Aliases
- HelpName
- Hidden
- Required
- Global
- Arity
- AllowedValues
- AllowMultipleArgumentsPerToken
- AllowExisting

## Arguments

An argument is a value passed to an option or a command. The following examples show an argument for the `verbosity` option and an argument for the `build` command.

```console
dotnet tool update dotnet-suggest --verbosity quiet --global
                                              ^---^
```

```console
dotnet build myapp.csproj
             ^----------^
```

Arguments can have default values that apply if no argument is explicitly provided. For example, many options are implicitly Boolean parameters with a default of `true` when the option name is in the command line. The following command-line examples are equivalent:

```dotnetcli
dotnet tool update dotnet-suggest --global
                                  ^------^

dotnet tool update dotnet-suggest --global true
                                  ^-----------^
```

Some options have required arguments. For example in the .NET CLI, `--output` requires a folder name argument. If the argument is not provided, the command fails.

Arguments can have expected types, and `System.CommandLine` displays an error message if an argument can't be parsed into the expected type. For example, the following command errors because "silent" isn't one of the valid values for `--verbosity`:

```dotnetcli
dotnet build --verbosity silent
```

```output
Cannot parse argument 'silent' for option '-v' as expected type 'Microsoft.DotNet.Cli.VerbosityOptions'. Did you mean one of the following?
Detailed
Diagnostic
Minimal
Normal
Quiet
```

---
The properties for `CliArgument` attribute (see [CliArgumentAttribute](https://dotmake.build/api/html/T_DotMake_CommandLine_CliArgumentAttribute.htm) docs for more info):
- Name
- Description
- HelpName
- Hidden
- Required
- Arity
- AllowedValues
- AllowExisting

## Additional documentation
- [DotMake Command-Line API docs](https://dotmake.build/api/)
- [Command-line syntax overview for System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax)
