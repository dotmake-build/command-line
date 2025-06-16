#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Linq;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region GetCompletionsCliCommand

    // A root cli command with completions for options and arguments.
    // After you inherit ICliGetCompletions, implement GetCompletions method which will be called for every option and argument,
    // you should switch according to the property name
    // which corresponds to the option or argument whose completions will be retrieved.

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
