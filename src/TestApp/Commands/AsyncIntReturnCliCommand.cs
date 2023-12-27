using System;
using System.Threading.Tasks;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand(Description = "A root cli command with async handler with Task<int> (return int)")]
    public class AsyncIntReturnCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public async Task<int> RunAsync()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();

            await Task.Delay(1000);
            return 0;
        }
    }
}
