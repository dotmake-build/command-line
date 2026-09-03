# Dependency Injection

Commands can have injected dependencies, this is supported via `Microsoft.Extensions.DependencyInjection` package (version >= 2.1.1).
In your project directory, via dotnet cli:
```console
dotnet add package Microsoft.Extensions.DependencyInjection
```
or in Visual Studio Package Manager Console:
```console
PM> Install-Package Microsoft.Extensions.DependencyInjection
```
When the source generator detects that your project has reference to `Microsoft.Extensions.DependencyInjection`,
it will generate extension methods for supporting dependency injection.
For example, you can now add your services with the extension method `Cli.Ext.ConfigureServices`:
```c#
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<TransientClass>();
    services.AddScoped<ScopedClass>();
    services.AddSingleton<SingletonClass>();
});

Cli.Run<RootCliCommand>();
```
Then let them be injected to your command class automatically by providing a constructor with the required services:
```c#
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
        Console.WriteLine($"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($"Value for {nameof(Argument1)} property is '{Argument1}'");
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
```
Other dependency injection containers (e.g. Autofac) are also supported 
via `Microsoft.Extensions.DependencyInjection.Abstractions` package (version >= 2.1.1).
In your project directory, via dotnet cli:
```console
dotnet add package Microsoft.Extensions.DependencyInjection.Abstractions
```
or in Visual Studio Package Manager Console:
```console
PM> Install-Package Microsoft.Extensions.DependencyInjection.Abstractions
```
When the source generator detects that your project has reference to `Microsoft.Extensions.DependencyInjection.Abstractions`,
it will generate extension methods for supporting custom service providers.
For example, you can now set your custom service provider with the extension method `Cli.Ext.SetServiceProvider`:
```c#
using DotMake.CommandLine;
using Autofac.Core;
using Autofac.Core.Registration;

var cb = new ContainerBuilder();
cb.RegisterType<object>();
var container = cb.Build();

Cli.Ext.SetServiceProvider(container);

Cli.Run<RootCliCommand>();
```
