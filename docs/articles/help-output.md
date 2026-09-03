# Help output

When you run the app via 
- `TestApp.exe -?` in project output path (e.g. in `TestApp\bin\Debug\net8.0`)
- or `dotnet run -- -?` in project directory (e.g. in `TestApp`) (note the double hyphen/dash which allows `dotnet run` to pass arguments to our actual application)

You see this usage help:
```console
DotMake Command-Line TestApp v2.5.0
Copyright © 2023-2025 DotMake

A root cli command

Usage:
  TestApp <argument-1> [options]

Arguments:
  <argument-1>  Description for Argument1 [required]

Options:
  -o1, --option-1  Description for Option1 [default: DefaultForOption1]
  -?, -h, --help   Show help and usage information
  -v, --version    Show version information
```

Note the header:
- First line comes from `AssemblyProductAttribute` or `AssemblyName` (`<Product>` tag in your .csproj file).  
  Version comes from `AssemblyInformationalVersionAttribute` or `AssemblyFileVersionAttribute` or `AssemblyVersionAttribute`
  (`<InformationalVersion>` or `<FileVersion >` or `<Version>` tag in your .csproj file).
- Second line comes from `AssemblyCopyrightAttribute` (`<Copyright>` tag in your .csproj file).
- Third line comes from `AssemblyDescriptionAttribute` (`<Description>` tag in your .csproj file) for root commands
  or from `[CliCommand].Description` property for subcommands.

Note, how command/directive/option/argument names, descriptions and default values are automatically populated.

By default, command/option/argument names are generated as follows;
- First the following suffixes are stripped out from class and property names:
    - For commands:
      "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli"
    - For directives:
      "Directive" or above command suffixes followed by "Directive", e.g. "CommandDirective" 
    - For options:
      "Option" or above command suffixes followed by "Option", e.g. "CommandOption" 
    - For arguments:
      "Argument" or above command suffixes followed by "Argument", e.g. "CommandArgument" 
    
- Then the names are converted to **kebab-case**.  
  (e.g. `Info` -> `info`,`ServerPort` -> `server-port`,  `Option1` -> `option-1`)  
  This can be changed by setting `[CliCommand].NameCasingConvention` property  to one of the following values:
  - `CliNameCasingConvention.None`
  - `CliNameCasingConvention.LowerCase`
  - `CliNameCasingConvention.UpperCase`
  - `CliNameCasingConvention.TitleCase`
  - `CliNameCasingConvention.PascalCase`
  - `CliNameCasingConvention.CamelCase`
  - `CliNameCasingConvention.KebabCase`
  - `CliNameCasingConvention.SnakeCase`
  
  For options, double hyphen/dash prefix is added to the name.   
  (e.g. `Info` -> `--info`,`ServerPort` -> `--server-port`,  `Option1` -> `--option-1`)  
  This can be changed by setting `[CliCommand].NamePrefixConvention` property (default: DoubleHyphen) 
  to one of the following values:
  - `CliNamePrefixConvention.None`
  - `CliNamePrefixConvention.SingleHyphen`
  - `CliNamePrefixConvention.DoubleHyphen`
  - `CliNamePrefixConvention.ForwardSlash`
  
  When you set a specific name via `[CliXXX].Name` property, that will be used instead of a auto-generated name.  
  For options, if you don't specify a prefix, it will be prefixed automatically according to `[CliCommand].NamePrefixConvention`  
  (e.g. `--option`, `-option` or `/option`) unless it's set to `CliNamePrefixConvention.None`.

  Auto-generated names can be disabled for all or specific CLI symbol types via `[CliCommand].NameAutoGenerate`.
  
- For commands and options, a short form alias is automatically added.
  First letters of every word in the name will be used to create short form to reduce conflicts.
  These first letters are converted according to `[CliCommand].NameCasingConvention` property.  
  (e.g. `Info` -> `i`,`ServerPort` -> `sp`,  `Option1` -> `o1`)  
    
  For options, single hyphen/dash prefix is added to the short form.  
  (e.g. `Info` -> `-i`,`ServerPort` -> `-sp`,  `Option1` -> `-o1`)  
  This can be changed via `[CliCommand].ShortFormPrefixConvention` property (default: SingleHyphen).

  When you set a specific alias via `[CliXXX].Alias` property, that will be used instead of a auto-generated short form alias.  
  For options,  if you don't specify a prefix, it will be prefixed automatically according to `[CliCommand].ShortFormPrefixConvention`  
  (e.g. `-o` or `--o` or `/o`) unless it's set to `CliNamePrefixConvention.None`.
  
  Auto-generated short form aliases can be disabled for all or specific CLI symbol types via `[CliCommand].ShortFormAutoGenerate`.

---
For example, change the name casing and prefix convention:
```c#
[CliCommand(
    Description = "A cli command with snake_case name casing and forward slash prefix conventions",
    NameCasingConvention = CliNameCasingConvention.SnakeCase,
    NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
    ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
)]
public class SnakeSlashCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }
}
```
When you run the app via `TestApp.exe -?` or `dotnet run -- -?`, you see this usage help:
```console
DotMake Command-Line TestApp v2.5.0
Copyright © 2023-2025 DotMake

A cli command with snake_case convention

Usage:
  TestApp <argument_1> [options]

Arguments:
  <argument_1>  Description for Argument1 [required]

Options:
  /o1, /option_1  Description for Option1 [default: DefaultForOption1]
  -?, -h, /help   Show help and usage information
  /v, /version    Show version information
```
Note how even the default options `version` and `help` use the new prefix convention `ForwardSlash`.
By the way, as `help` is a special option, which allows user to discover your app, we still add short form aliases with other prefix to prevent confusion.

## Themes

Cli app theme can be changed via setting `CliSettings.Theme` property to predefined themes Red, DarkRed, Green, DarkGreen, Blue, DarkBlue
or a custom `CliTheme`. These color and formatting option are mainly used by the help output.

```c#
Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.Red });

Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.DarkRed });

Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.Green });

Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.DarkGreen });

Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.Blue });

Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.DarkBlue });

Cli.Run<RootCliCommand>(args, new CliSettings
{
    Theme = new CliTheme(CliTheme.Default)
    {
        HeadingCasing = CliNameCasingConvention.UpperCase,
        HeadingNoColon = true
    }
});
```

Note that [NO_COLOR](https://no-color.org/) is supported, i.e. if `NO_COLOR` environment variable is set, the colors will be disabled.

## Localization

Localizing commands, options and arguments is supported.
You can specify a `nameof` operator expression with a resource property (generated by resx) in the attribute's argument (for `string` types only)
and the source generator will smartly use the resource property accessor as the value of the argument so that it can localize at runtime.
If the property in the `nameof` operator expression does not point to a resource property, then the name of that property will be used as usual.
The reason we use `nameof` operator is that attributes in `.NET` only accept compile-time constants and you get `CS0182` error if not,
so specifying resource property directly is not possible as it's not a compile-time constant but it's a static property access.

```c#
[CliCommand(Description = nameof(TestResources.CommandDescription))]
public class LocalizedCliCommand
{
    [CliOption(Description = nameof(TestResources.OptionDescription))]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = nameof(TestResources.ArgumentDescription))]
    public string Argument1 { get; set; }
}
```

## Triggering help

If a command represents a group and not an action, you may want to show help. 
If `Run` or `RunAsync` method is missing in a command class, then by default it will show help. 
You can also manually trigger help in `Run` or `RunAsync` method of a command class via calling `CliContext.ShowHelp()`.
For testing a command, these methods are also useful:
- `CliContext.Result.HasArgs` gets a value indicating whether called command is specified with any arguments or options.
  Note that this may return `false` even if any arguments or options were specified for parent commands.
  because only arguments or options specified for the called command, are checked.
  Note that arguments and options should be optional, if they are required (no default values),
  then handler will not run and missing error message will be shown.
- `CliContext.Result.HasTokens` gets a value indicating whether root command is specified with any subcommands, directives, options or arguments.
- `CliContext.ShowValues()` shows parsed values for current command and its arguments and options.
- `CliContext.ShowHierarchy()` shows hierarchy for all commands, it will start from the root command and show a tree.

See below example; root command does not have a handler method so it will always show help 
and sub-command will show help if command is specified without any arguments or option, 
and it will show (dump) values if not:

```c#
[CliCommand(Description = "A root cli command")]
public class HelpCliCommand
{
  [CliOption(Description = "Description for Option1")]
  public string Option1 { get; set; } = "DefaultForOption1";

  [CliArgument(Description = "Description for Argument1")]
  public string Argument1 { get; set; } = "DefaultForArgument1";

  [CliCommand(Description = "A sub cli command")]
  public class SubCliCommand
  {
      [CliArgument(Description = "Description for Argument2")]
      public string Argument2 { get; set; } = "DefaultForArgument2";

      public void Run(CliContext context)
      {
          if (!context.Result.HasArgs)
              context.ShowHelp();
          else
              context.ShowValues();
      }
  }
}
```
