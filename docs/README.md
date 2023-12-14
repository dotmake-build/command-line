![DotMake Command-Line Logo](https://raw.githubusercontent.com/dotmake-build/command-line/master/images/logo-wide.png "DotMake Command-Line Logo")

# DotMake Command-Line

Declarative syntax for [System.CommandLine](https://github.com/dotnet/command-line-api) via attributes for easy, fast, strongly-typed (no reflection) usage. Includes a source generator which automagically converts your classes to CLI commands and properties to CLI options or CLI arguments.

System.CommandLine is a very good parser but you need a lot of boilerplate code to get going and the API is hard to discover. This becomes complicated to newcomers and also you would have a lot of ugly code in your `Program.cs` to maintain. What if you had an easy class-based layer combined with a good parser?

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
- Visual Studio 2022 v17.3+ or .NET SDK 6.0.407+ (our incremental source generator requires performance features added first in these versions).
- Usually a console app project but you can also use a class library project which will be consumed later.

## Usage

Create a simple class like this:

```c#
using System;
using DotMake.CommandLine;

[DotMakeCliCommand(Description = "A root cli command")]
public class RootCliCommand
{
	[DotMakeCliOption(Description = "Description for Option1")]
	public string Option1 { get; set; } = "DefaultForOption1";
 
	[DotMakeCliArgument(Description = "Description for Argument1")]
	public string Argument1 { get; set; } = "DefaultForArgument1";
 
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
DotMakeCli.Run<RootCliCommand>(args);
```
And that's it! You now have a fully working command-line app. You just specify the name of your class which represents your root command to `DotMakeCli.Run<>` method and everything is wired.

If you want to go async, just use this:
```c#
await DotMakeCli.RunAsync<RootCliCommand>(args);
```
To handle exceptions, you just use a try-catch block:
```c#
try
{
	DotMakeCli.Run<RootCliCommand>(args);
}
catch (Exception e)
{
	Console.WriteLine(@"Exception in main: {0}", e.Message);
}
```
System.CommandLine, by default overtakes your exceptions that are thrown in command handlers (even if you don't set an exception handler explicitly) but DotMake.CommandLine, by default allows the exceptions to pass through. However if you wish, you can easily use an exception handler by using `configureBuilder` delegate parameter like this:
```c#
DotMakeCli.Run<RootCliCommand>(args, builder => 
	builder.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
);
```
### Summary
- Mark the class with `DotMakeCliCommand` attribute to make it a CLI command.
- Mark a property with `DotMakeCliOption` attribute to make it a CLI option.
- Mark a property with `DotMakeCliArgument` attribute to make it a CLI argument.
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
- Call `DotMakeCli.Run<>` or`DotMakeCli.RunAsync<>` method with your class name to run your CLI app.

---
When the command handler is run, the properties for CLI options and arguments will be already populated 
and bound from values passed in the command-line. If no matching value is passed, the property will have its default value.

When you run the app via 
- `TestApp.exe` in project output path (e.g. in `TestApp\bin\Debug\net6.0`)
- or `dotnet run`in project directory (e.g. in `TestApp`)

You see this result:
```console
Handler for 'TestApp.Commands.RootCliCommand' is run:
Value for Option1 property is 'DefaultForOption1'
Value for Argument1 property is 'DefaultForArgument1'
```
As we set default values for properties in the class, the option and the argument were already populated (even when the user did not pass any values).

---
When you run,
```console
TestApp.exe NewValueForArgument1 --option-1 NewValueForOption1
```
or (note the double hyphen/dash which allows `dotnet run` to pass arguments to our actual application):
```console
dotnet run -- NewValueForArgument1 --option-1 NewValueForOption1
```
You see this result:
```console
Handler for 'TestApp.Commands.RootCliCommand' is run:
Value for Option1 property is 'NewValueForOption1'
Value for Argument1 property is 'NewValueForArgument1'
```

---
When you run the app via `TestApp.exe -?` or `dotnet run -- -?`, you see this usage help:
```console
Description:
  A root cli command

Usage:
  TestApp [<argument-1>] [options]

Arguments:
  <argument-1>  Description for Argument1 [default: DefaultForArgument1]

Options:
  -o, --option-1 <option-1>  Description for Option1 [default: DefaultForOption1]
  -v, --version              Show version information
  -?, -h, --help             Show help and usage information
```
Note, how command/option/argument names, descriptions and default value are automatically populated.

By default,  command/option/argument names are generated as follows;
- First the following suffixes are stripped out from class and property names:
	- For commands:
	  "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli"
	- For options: 
	 "RootCommandOption", "SubCliCommandOption", "SubCommandOption", "CliCommandOption", "CommandOption", "CliOption", "Option"
	- For arguments: 
	"RootCliCommandArgument", "RootCommandArgument", "SubCliCommandArgument", "SubCommandArgument", "CliCommandArgument", "CommandArgument", "CliArgument", "Argument"
	
- Then the names are converted to **kebab-case**, this can be changed by setting `NameCasingConvention`  property of the `DotMakeCliCommand` attribute to one of the following values:
  - `DotMakeCliCasingConvention.None`
  - `DotMakeCliCasingConvention.LowerCase`
  - `DotMakeCliCasingConvention.UpperCase`
  - `DotMakeCliCasingConvention.TitleCase`
  - `DotMakeCliCasingConvention.PascalCase`
  - `DotMakeCliCasingConvention.CamelCase`
  - `DotMakeCliCasingConvention.KebabCase`
  - `DotMakeCliCasingConvention.SnakeCase`
  
- For options, double hyphen/dash prefix is added to the name (e.g. `--option`), this can be changed by setting `NamePrefixConvention`  (default: DoubleHyphen) property of the `DotMakeCliCommand` attribute to one of the following values:
  - `DotMakeCliPrefixConvention.SingleHyphen`
  - `DotMakeCliPrefixConvention.DoubleHyphen`
  - `DotMakeCliPrefixConvention.ForwardSlash`
  
- For options, short-form alias with first letter (e.g. `-o`) is automatically added. This can be changed by setting `ShortFormAutoGenerate` (default: true) and `ShortFormPrefixConvention` (default: SingleHyphen) properties of the `DotMakeCliCommand` attribute.

---
For example, change the name casing and prefix convention:
```c#
using System;
using DotMake.CommandLine;
 
[DotMakeCliCommand(
	Description = "A cli command with snake_case name casing and forward slash prefix conventions",
	NameCasingConvention = DotMakeCliCasingConvention.SnakeCase,
	NamePrefixConvention = DotMakeCliPrefixConvention.ForwardSlash,
	ShortFormPrefixConvention = DotMakeCliPrefixConvention.ForwardSlash
)]
public class RootCliCommand
{
	[DotMakeCliOption(Description = "Description for Option1")]
	public string Option1 { get; set; } = "DefaultForOption1";
 
	[DotMakeCliArgument(Description = "Description for Argument1")]
	public string Argument1 { get; set; } = "DefaultForArgument1";
 
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
Description:
  A cli command with snake_case name casing and forward slash prefix conventions

Usage:
  TestApp [<argument_1>] [options]

Arguments:
  <argument_1>  Description for Argument1 [default: DefaultForArgument1]

Options:
  /o, /option_1 <option_1>  Description for Option1 [default: DefaultForOption1]
  /v, /version              Show version information
  -?, -h, /help             Show help and usage information
```
Note how even the default options `version` and `help` use the new prefix convention `ForwardSlash`. By the way, as `help` is a special option, which allows user to discover your app, we still add short-form aliases with other prefix to prevent confusion.

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
[DotMakeCliCommand(Description = "A root cli command with nested children")]
public class WithNestedChildrenCliCommand
{
	[DotMakeCliOption(Description = "Description for Option1")]
	public string Option1 { get; set; } = "DefaultForOption1";
 
	[DotMakeCliArgument(Description = "Description for Argument1")]
	public string Argument1 { get; set; } = "DefaultForArgument1";
 
	public void Run()
	{
		Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
		Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
		Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
		Console.WriteLine();
	}
 
	[DotMakeCliCommand(Description = "A nested level 1 sub-command")]
	public class Level1SubCliCommand
	{
		[DotMakeCliOption(Description = "Description for Option1")]
		public string Option1 { get; set; } = "DefaultForOption1";
 
		[DotMakeCliArgument(Description = "Description for Argument1")]
		public string Argument1 { get; set; } = "DefaultForArgument1";
 
		public void Run()
		{
			Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
			Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
			Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
			Console.WriteLine();
		}
 
		[DotMakeCliCommand(Description = "A nested level 2 sub-command")]
		public class Level2SubCliCommand
		{
			[DotMakeCliOption(Description = "Description for Option1")]
			public string Option1 { get; set; } = "DefaultForOption1";
 
			[DotMakeCliArgument(Description = "Description for Argument1")]
			public string Argument1 { get; set; } = "DefaultForArgument1";
 
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
Just make sure you apply `DotMakeCliCommand` attribute to the nested classes as well.
Command hierarchy in above example is: **WithNestedChildrenCliCommand** -> **Level1SubCliCommand** -> **Level2SubCliCommand**

Another way to create hierarchy between commands, especially if you want to use standalone classes, is to use `Parent` property of `DotMakeCliCommand` attribute to specify `typeof` parent class:
```c#
[DotMakeCliCommand(Description = "A root cli command")]
public class RootCliCommand
{
	[DotMakeCliOption(Description = "Description for Option1")]
	public string Option1 { get; set; } = "DefaultForOption1";
 
	[DotMakeCliArgument(Description = "Description for Argument1")]
	public string Argument1 { get; set; } = "DefaultForArgument1";
 
	public void Run()
	{
		Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
		Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
		Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
		Console.WriteLine();
	}
}

[DotMakeCliCommand(
	Name = "Level1External",
	Description = "An external level 1 sub-command",
	Parent = typeof(RootCliCommand)
)]
public class ExternalLevel1SubCliCommand
{
    [DotMakeCliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [DotMakeCliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; } = "DefaultForArgument1";

    public void Run()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();
    }

    [DotMakeCliCommand(Description = "A nested level 2 sub-command")]
    public class Level2SubCliCommand
    {
        [DotMakeCliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [DotMakeCliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; } = "DefaultForArgument1";

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
Command hierarchy in above example is: **RootCliCommand** -> **ExternalLevel1SubCliCommand** -> **Level2SubCliCommand**

---
The class that `DotMakeCliCommand` attribute is applied to,
- will be a root command if the class is not a nested class and `Parent`property is not set.
- will be a sub command if the class is a nested class or `Parent` property is set.

The properties for `DotMakeCliCommand` attribute:
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

The properties for `DotMakeCliOption` attribute:
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
The properties for `DotMakeCliArgument` attribute:
- Name
- Description
- HelpName
- Hidden
- Arity
- AllowedValues

## Additional documentation
- [Command-line syntax overview for System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/syntax)
