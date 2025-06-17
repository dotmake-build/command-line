#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Linq;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region GetCompletionsCliCommand

    /*
        Apps that use System.CommandLine have built-in support for tab completion in certain shells. 
        To enable it, the end user has to [take a few steps once per shell](https://learn.microsoft.com/en-us/dotnet/standard/commandline/tab-completion#get-tab-completion-values-at-run-time). 
        Once the user does this, tab completion is automatic for static values in your app, such as enum values or values you 
        define by setting `CliOptionAttribute.AllowedValues` or `CliArgumentAttribute.AllowedValues`. 
        You can also customize the tab completion by getting values dynamically at runtime.

        In your command class, inherit `ICliGetCompletions` and implement `GetCompletions` method.
        This method will be called for every option and argument in your class.
        In the  method, you should switch according to the property name
        which corresponds to the option or argument whose completions will be retrieved.
     */

    [CliCommand(Description = "A root cli command with completions for options and arguments")]
    public class GetCompletionsCliCommand : ICliGetCompletions
    {
        [CliOption(Description = "Description for DateOption")]
        public DateTime DateOption { get; set; }

        [CliArgument(Description = "Description for FruitArgument")]
        public string FruitArgument { get; set; } = "DefaultForFruitArgument";

        public void Run(CliContext context)
        {
            if (context.IsEmptyCommand())
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
                        .Select(value => new CompletionItem(value, "Value", null, null, null, null));
            }

            return Enumerable.Empty<CompletionItem>();
        }
    }

    #endregion
}
