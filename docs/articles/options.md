# Options

An option is a named parameter that can be passed to a command. [POSIX](https://en.wikipedia.org/wiki/POSIX) CLIs typically prefix the option name with two hyphens (`--`). The following example shows two options:

```console
dotnet tool update dotnet-suggest --verbosity quiet --global
                                  ^---------^       ^------^
```

As this example illustrates, the value of the option may be explicit (`quiet` for `--verbosity`) or implicit (nothing follows `--global`). Options that have no value specified are typically Boolean parameters that default to `true` if the option is specified on the command line.

For some Windows command-line apps, you identify an option by using a leading slash (`/`) with the option name. For example:

```console
msbuild /version
        ^------^
```

Both POSIX and Windows prefix conventions are supported (e.g. `--option`, `-o`, `-option` or `/option`).

It's allowed to use a space, `=`, or `:` as the delimiter between an option name and its argument.
For example, the following commands are equivalent:
```console
dotnet build -v quiet
dotnet build -v=quiet
dotnet build -v:quiet
```
A POSIX convention lets you omit the delimiter when you are specifying a single-character option alias. For example, the following commands are equivalent:
```console
myapp -vquiet
myapp -v quiet
```

Bundling of single-character options are supported, also known as stacking.
Bundled options are single-character option aliases specified together after a single hyphen prefix.
For example if you have options "-a", "-b" and "-c", you can bundle them like "-abc".
Only the last option can specify an argument.
Note that if you have an explicit option named "-abc" then it will win over bundled options.

## Mutually exclusive option groups

**Mutually exclusive options** are options that belong to the same group but cannot be used together.  
At most one option from the group can be specified.  
**Example**: You shouldn’t ask a CLI to output both JSON and XML at the same time.  

To declare that options are mutually exclusive, assign them the same `Group` in `[CliOption]` attributes:

```c#
[CliCommand(Description = "Display different file formats")]
public class FormatCommand
{
    [CliOption(Group = "Format", Description = "Output as XML")]
    public bool Xml { get; set; }

    [CliOption(Group = "Format", Description = "Output as JSON")]
    public bool Json { get; set; }  

    [CliOption(Description = "Verbosity level", Required = false)]
    public string Verbose { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}
```

In the above example, the `Xml` and `Json` options are mutually exclusive because they share the same `Group` value `"Format"`.  

If the user tries to specify both options together, the CLI will display an error:

```console
mytool --xml --json
```

> **Error:**  
> Options in group 'Format' are mutually exclusive. You must specify only one of: `-x|--xml`, `-j|--json`

## Required option groups

Sometimes you want to enforce that **exactly one option from a group must be specified**.
This is done by setting the `RequiredGroups` property in `[CliCommand]` attribute and listing the group names:

```c#
[CliCommand(RequiredGroups = new[] { "auth" })]
public class ReportCommand
{
    // Group 1: Output format (mutually exclusive)
    [CliOption(Group = "output-format")]
    public bool Json { get; set; }

    [CliOption(Group = "output-format")]
    public bool Xml { get; set; }

    // Group 2: Authentication (required group)
    [CliOption(Group = "auth")]
    public string ApiKey { get; set; }

    [CliOption(Group = "auth")]
    public string Token { get; set; }
    
    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}
```

In this example:
- The `"output-format"` group is **mutually exclusive**: you can choose JSON or XML, but not both.  
- The `"auth"` group is **required**: you must specify exactly one of `--apikey` or `--token`.  

If the user does not provide any option from the required group, the CLI will display an error:

```console
mytool --json
```

> **Error:**  
> You must specify exactly one option in required group 'auth': `-ak|--api-key`, `-t|--token`

---
The properties for `[CliOption]` attribute (see [CliOptionAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliOptionAttribute.html) docs for more info):
- Name
- Description
- Hidden
- Order
- Alias
- Aliases
- HelpName
- Required
- Recursive
- Arity
- AllowedValues
- Group
- ValidationRules
- ValidationPattern
- ValidationMessage
- AllowMultipleArgumentsPerToken
