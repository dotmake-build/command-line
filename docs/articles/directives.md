# Directives

`System.CommandLine` introduces a syntactic element called a *directive*. The `[diagram]` directive is an example. When you include `[diagram]` after the app's name, `System.CommandLine` displays a diagram of the parse result instead of invoking the command-line app:

```console
dotnet [diagram] build --no-restore --output ./build-output/
       ^-------^
```
Output:
```console
[ dotnet [ build [ --no-restore <True> ] [ --output <./build-output/> ] ] ]
```

The purpose of directives is to provide cross-cutting functionality that can apply across command-line apps. Because directives are syntactically distinct from the app's own syntax, they can provide functionality that applies across apps.

A directive must conform to the following syntax rules:

* It's a token on the command line that comes after the app's name but before any subcommands or options.
* It's enclosed in square brackets.
* It doesn't contain spaces.

An unrecognized directive is ignored without causing a parsing error.

A directive can include an argument, separated from the directive name by a colon (`:`).
```console
myapp [directive:value]
```
```console
myapp [directive:value1] [directive:value2]
```

You can define custom directives with `[CliDirective]` attribute like below:
```c#
[CliCommand(Description = "A root cli command with directives")]
public class DirectiveCliCommand
{
    [CliDirective]
    public bool Debug { get; set; }

    [CliDirective]
    public string Directive2 { get; set; }

    [CliDirective]
    public string[] Vars { get; set; }

    public void Run(CliContext context)
    {
        if (context.IsEmpty())
            context.ShowHelp();
        else
        {
            Console.WriteLine($"Directive '{nameof(Debug)}' = {StringExtensions.FormatValue(Debug)}");
            Console.WriteLine($"Directive '{nameof(Directive2)}' = {StringExtensions.FormatValue(Directive2)}");
            Console.WriteLine($"Directive '{nameof(Vars)}' = {StringExtensions.FormatValue(Vars)}");
        }
    }
}
```
Currently only `bool`, `string` and `string[]` types are supported for `[CliDirective]` properties.
Here is sample usage and output:
```console
src\TestApp\bin\Debug\net8.0>TestApp [debug] [directive-2:val1] [vars:val2] [vars:val3]
Directive 'Debug' = true
Directive 'Directive2' = "val1"
Directive 'Vars' = ["val2", "val3"]
```

The following directives are built in:

## The `[diagram]` directive

This directive can be enabled with `CliSettings.EnableDiagramDirective`.

Both users and developers may find it useful to see how an app will interpret a given input. One of the default features of a `System.CommandLine` app is the `[diagram]` directive, which lets you preview the result of parsing command input. For example:

```console
myapp [diagram] --delay not-an-int --interactive --file filename.txt extra
```

```console
![ myapp [ --delay !<not-an-int> ] [ --interactive <True> ] [ --file <filename.txt> ] *[ --fgcolor <White> ] ]   ???--> extra
```

In the preceding example:

* The command (`myapp`), its child options, and the arguments to those options are grouped using square brackets.
* For the option result `[ --delay !<not-an-int> ]`, the `!` indicates a parsing error. The value `not-an-int` for an `int` option can't be parsed to the expected type. The error is also flagged by `!` in front of the command that contains the error-ed option: `![ myapp...`.
* For the option result `*[ --fgcolor <White> ]`, the option wasn't specified on the command line, so the configured default was used. `White` is the effective value for this option. The asterisk indicates that the value is the default.
* `???-->` points to input that wasn't matched to any of the app's commands or options.

## The `[suggest]` directive

This directive is enabled by default and can be disabled with `CliSettings.EnableSuggestDirective`.

The `[suggest]` directive lets you search for commands when you don't know the exact command.

```console
dotnet [suggest] buil
```

```console
build
build-server
msbuild
```

## The `[env]` directive

This directive can be enabled with `CliSettings.EnableEnvironmentVariablesDirective`.

The `[env]` directive allows environment variables to be set from the command line during invocation:
```console
myapp [env:key=value]
```
```console
myapp [env:key1=value1] [env:key2=value2]
```
