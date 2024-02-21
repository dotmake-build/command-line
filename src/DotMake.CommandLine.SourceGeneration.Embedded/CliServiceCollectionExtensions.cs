using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> related extension methods for <see cref="Cli"/> services feature.
    /// <para>
    /// Requires dependency <c>Microsoft.Extensions.DependencyInjection (>= 2.1.1)</c>.
    /// <br/>Default implementation <see cref="ServiceCollection"/> is in <c>Microsoft.Extensions.DependencyInjection</c> assembly.
    /// </para>
    /// </summary>
    public static class CliServiceCollectionExtensions
    {
        private static readonly IServiceCollection ServiceCollection = new ServiceCollection();
        private static IServiceProvider serviceProvider;

        /// <summary>
        /// Registers services into the <see cref="Cli"/>'s default service collection.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <param name="configure">An <see cref="Action{IServiceCollection}"/> to configure the <see cref="Cli"/>'s default service collection.</param>
        public static void ConfigureServices(this CliExtensions ext, Action<IServiceCollection> configure)
        {
            configure(ServiceCollection);
        }

        /// <summary>
        /// Gets the <see cref="Cli"/>'s default service collection.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <returns>A <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection GetServiceCollection(this CliExtensions ext)
        {
            return ServiceCollection;
        }

        /// <summary>
        /// Gets the service provider built from <see cref="Cli"/>'s default service collection (built on first access).
        /// If <see cref="CliServiceProviderExtensions.SetServiceProvider"/> was used, then gets the custom <see cref="IServiceProvider"/> that was set.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <returns>A <see cref="IServiceProvider"/> instance.</returns>
        public static IServiceProvider GetServiceProviderOrDefault(this CliExtensions ext)
        {
            return serviceProvider ??= CliServiceProviderExtensions.GetServiceProvider(null)
                                       ?? ServiceCollection.BuildServiceProvider();
        }
    }
}
