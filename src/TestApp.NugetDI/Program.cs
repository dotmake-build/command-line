using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using TestDIApp.Commands;

Cli.Ext.ConfigureServices(services =>
{
    services.AddTransient<TransientClass>();
    services.AddScoped<ScopedClass>();
    services.AddSingleton<SingletonClass>();
});

Cli.Run<RootCliCommand>();
