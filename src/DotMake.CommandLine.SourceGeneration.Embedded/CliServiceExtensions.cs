using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="Cli"/> services feature.
    /// </summary>
    public static class CliServiceExtensions
    {
        private static readonly ServiceCollection ServiceCollection = new ServiceCollection();
        private static ServiceProvider serviceProvider;

        /// <summary>
        /// Registers services into the <see cref="Cli"/>'s service collection.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <param name="configure">An <see cref="Action{IServiceCollection}"/> to configure the <see cref="Cli"/>'s service collection.</param>
        public static void ConfigureServices(this CliExtensions ext, Action<IServiceCollection> configure)
        {
            configure(ServiceCollection);
        }

        /// <summary>
        /// Gets the service provider built from <see cref="Cli"/>'s service collection (built on first access).
        /// If <see cref="SetServiceProvider"/> was used, then gets the custom <see cref="ServiceProvider"/> that was set.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <returns>A <see cref="ServiceProvider"/> instance.</returns>
        public static ServiceProvider GetServiceProvider(this CliExtensions ext)
        {
            return serviceProvider ??= ServiceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Sets a custom service provider built from a custom service collection (to override the internal one).
        /// When <see cref="GetServiceProvider"/> is called, this custom <see cref="ServiceProvider"/> will be returned.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <param name="customServiceProvider">The custom <see cref="ServiceProvider"/> instance to use.</param>
        public static void SetServiceProvider(this CliExtensions ext, ServiceProvider customServiceProvider)
        {
            serviceProvider = customServiceProvider;
        }
    }
}
