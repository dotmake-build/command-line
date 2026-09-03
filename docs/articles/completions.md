# Completions

Apps that use System.CommandLine have built-in support for tab completion in certain shells. 
To enable it, the end user has to [take a few steps once per shell](https://learn.microsoft.com/en-us/dotnet/standard/commandline/tab-completion#get-tab-completion-values-at-run-time). 
Once the user does this, tab completion is automatic for static values in your app, such as enum values or values you 
define by setting `[CliOption].AllowedValues` or `[CliArgument].AllowedValues`. 
You can also customize the tab completion by getting values dynamically at runtime.

In your command class, inherit `ICliGetCompletions` and implement `GetCompletions` method.
This method will be called for every option and argument in your class.
In the  method, you should switch according to the property name
which corresponds to the option or argument whose completions will be retrieved.

```c#
using System;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Linq;
using DotMake.CommandLine;

[CliCommand(Description = "A root cli command with completions for options and arguments")]
public class GetCompletionsCliCommand : ICliGetCompletions
{
    [CliOption(Description = "Description for DateOption")]
    public DateTime DateOption { get; set; }

    [CliArgument(Description = "Description for FruitArgument")]
    public string FruitArgument { get; set; } = "DefaultForFruitArgument";

    public void Run(CliContext context)
    {
        if (!context.Result.HasArgs)
            context.ShowHelp();
        else
            context.ShowValues();
    }

    public IEnumerable<CompletionItem> GetCompletions(string propertyName, CompletionContext completionContext)
    {
        switch (propertyName)
        {
            case nameof(DateOption):
                var today = DateTime.Today;
                var dates = new List<CompletionItem>();

                foreach (var i in Enumerable.Range(1, 7))
                {
                    var date = today.AddDays(i);
                    dates.Add(new CompletionItem(
                        label: date.ToShortDateString(),
                        sortText: $"{i:2}"));
                }

                return dates;

            case nameof(FruitArgument):
                return new [] { "apple", "orange", "banana" }
                    .Select(value => new CompletionItem(value));
        }

        return Enumerable.Empty<CompletionItem>();
    }
}
```

The dynamic tab completion list created by this code also appears in help output:

```console
DotMake Command-Line TestApp v2.5.0
Copyright © 2023-2025 DotMake

A root cli command with completions for options and arguments

Usage:
  TestApp [<fruit>] [options]

Arguments:
  <apple|banana|orange>  Description for FruitArgument [default: DefaultForFruitArgument]

Options:
  -d, --date                                                  Description for DateOption [default: 1.01.0001 00:00:00]
  <22.04.2025|23.04.2025|24.04.2025|25.04.2025|26.04.2025|27
  .04.2025|28.04.2025>
  -?, -h, --help                                              Show help and usage information
  -v, --version                                               Show version information
```
