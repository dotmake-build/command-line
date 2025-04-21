#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Completions;
using System.Linq;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region AddCompletionsCliCommand

    // A root cli command with completions for options and arguments.
    // After you inherit ICliAddCompletions, implement AddCompletions method which will be called for every option and argument,
    // you should switch according to the property name
    // which corresponds to the option or argument whose completions will be modified.

    [CliCommand(Description = "A root cli command with completions for options and arguments")]
    public class AddCompletionsCliCommand : ICliAddCompletions
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

        public void AddCompletions(string propertyName, List<Func<CompletionContext, IEnumerable<CompletionItem>>> completionSources)
        {
            switch (propertyName)
            {
                case nameof(DateOption):
                    completionSources.Add(completionContext =>
                    {
                        var today = System.DateTime.Today;
                        var dates = new List<CompletionItem>();
                        foreach (var i in Enumerable.Range(1, 7))
                        {
                            var date = today.AddDays(i);
                            dates.Add(new CompletionItem(
                                label: date.ToShortDateString(),
                                sortText: $"{i:2}"));
                        }
                        return dates;
                    });
                    break;

                case nameof(FruitArgument):
                    completionSources.Add("apple", "orange", "banana");
                    break;
            }
        }
    }

    #endregion
}
