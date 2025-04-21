#pragma warning disable CS1591
using System;
using System.Threading.Tasks;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RunAsyncCliCommand

    /*
      We also provide interfaces `ICliRun`, `ICliRunWithReturn`, `ICliRunWithContext`, `ICliRunWithContextAndReturn`
      and async versions `ICliRunAsync`, `ICliRunAsyncWithReturn`, `ICliRunAsyncWithContext`, `ICliRunAsyncWithContextAndReturn` 
      that you can inherit in your command class.
      Normally you don't need an interface for a handler method as the source generator can detect it automatically,
      but the interfaces can be used to prevent your IDE complain about unused method in class.
    */

    [CliCommand(Description = "A root cli command with async handler with Task (return void)")]
    public class RunAsyncCliCommand : ICliRunAsync
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public async Task RunAsync()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();

            await Task.Delay(1000);
        }
    }

    #endregion
}
