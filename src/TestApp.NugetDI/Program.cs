#region Namespace
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
#endregion
using TestApp.NugetDI.Commands;

#region ConfigureServices

// When the source generator detects that your project has reference to `Microsoft.Extensions.DependencyInjection`,
// it will generate extension methods for supporting dependency injection.
// For example, you can now add your services with the extension method `Cli.Ext.ConfigureServices`:

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<TransientClass>();
    services.AddScoped<ScopedClass>();
    services.AddSingleton<SingletonClass>();
});

Cli.Run<RootCliCommand>();

#endregion
