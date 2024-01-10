using System;
#region Namespace
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
#endregion

// ReSharper disable once CheckNamespace
namespace DotMake.CommandLine.CliServiceExtensionsExamples
{
    public class Program
    {
        public void ConfigureServices(string[] args)
        {
            #region ConfigureServices

            //When the source generator detects that your project has reference to `Microsoft.Extensions.DependencyInjection`,
            //it will generate extension methods for supporting dependency injection.
            //For example, you can now add your services with the extension method `Cli.Ext.ConfigureServices`:

            Cli.Ext.ConfigureServices(services =>
            {
                services.AddTransient<TransientClass>();
                services.AddScoped<ScopedClass>();
                services.AddSingleton<SingletonClass>();
            });

            Cli.Run<RootCliCommand>();

            #endregion
        }

        #region RootCliCommand

        //Then let them be injected to your command class automatically by providing a constructor with the required services:

        [CliCommand(Description = "A root cli command with dependency injection")]
        public class RootCliCommand
        {
            private readonly TransientClass transientDisposable;
            private readonly ScopedClass scopedDisposable;
            private readonly SingletonClass singletonDisposable;

            public RootCliCommand(
                TransientClass transientDisposable,
                ScopedClass scopedDisposable,
                SingletonClass singletonDisposable
            )
            {
                this.transientDisposable = transientDisposable;
                this.scopedDisposable = scopedDisposable;
                this.singletonDisposable = singletonDisposable;
            }

            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            public void Run()
            {
                Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                Console.WriteLine();

                Console.WriteLine($"Instance for {transientDisposable.Name} is available");
                Console.WriteLine($"Instance for {scopedDisposable.Name} is available");
                Console.WriteLine($"Instance for {singletonDisposable.Name} is available");
                Console.WriteLine();
            }
        }

        public sealed class TransientClass : IDisposable
        {
            public string Name => nameof(TransientClass);

            public void Dispose() => Console.WriteLine($"{nameof(TransientClass)}.Dispose()");
        }

        public sealed class ScopedClass : IDisposable
        {
            public string Name => nameof(ScopedClass);

            public void Dispose() => Console.WriteLine($"{nameof(ScopedClass)}.Dispose()");
        }

        public sealed class SingletonClass : IDisposable
        {
            public string Name => nameof(SingletonClass);

            public void Dispose() => Console.WriteLine($"{nameof(SingletonClass)}.Dispose()");
        }

        #endregion
    }
}
