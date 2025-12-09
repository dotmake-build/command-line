using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> related extension methods for <see cref="Cli"/> services feature.
    /// <para>
    /// Requires dependency <c>Microsoft.Extensions.DependencyInjection (>= 2.1.1)</c>.
    /// <br/>Although <see cref="ServiceCollection"/> is in <c>Microsoft.Extensions.DependencyInjection.Abstractions</c> assembly,
    /// <br/>used method <see cref="ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(IServiceCollection)"/> is in <c>Microsoft.Extensions.DependencyInjection</c> assembly.
    /// </para>
    /// </summary>
    public static class CliServiceCollectionExtensions
    {
        private static readonly IServiceCollection ServiceCollection = new ServiceCollection();

        /// <summary>
        /// Registers services into the <see cref="Cli"/>'s default service collection.
        /// <para>
        /// Note that calling this will reset existing service provider if it was already built or set,
        /// so that changed service collection can take effect.
        /// </para>
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <param name="configure">An <see cref="Action{IServiceCollection}"/> to configure the <see cref="Cli"/>'s default service collection.</param>
        public static void ConfigureServices(this CliExtensions ext, Action<IServiceCollection> configure)
        {
            //Reset existing service provider if it was already built
            var serviceProvider = CliServiceProviderExtensions.GetServiceProvider(null);
            if (serviceProvider != null)
                CliServiceProviderExtensions.SetServiceProvider(null, null);
            
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
            var serviceProvider = CliServiceProviderExtensions.GetServiceProvider(null);
            if (serviceProvider == null)
            {
                serviceProvider = ServiceCollection.BuildServiceProvider();
                CliServiceProviderExtensions.SetServiceProvider(null, serviceProvider);
            }

            return serviceProvider;
        }
    }
}
