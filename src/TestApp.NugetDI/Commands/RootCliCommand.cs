using System;
using DotMake.CommandLine;

namespace TestDIApp.Commands
{
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
}
