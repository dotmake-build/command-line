using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides <see cref="IServiceProvider"/> related extension methods for <see cref="Cli"/> services feature.
    /// <para>
    /// Requires dependency <c>Microsoft.Extensions.DependencyInjection.Abstractions (>= 2.1.1)</c>.
    /// <br/>Although <see cref="IServiceProvider"/> is in <c>System.ComponentModel</c> assembly,
    /// used class <see cref="ActivatorUtilities"/> is in <c>Microsoft.Extensions.DependencyInjection.Abstractions</c> assembly.
    /// </para>
    /// </summary>
    public static class CliServiceProviderExtensions
    {
        private static IServiceProvider serviceProvider;
        private static readonly HashSet<IDisposable> Disposables = new();

        static CliServiceProviderExtensions()
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            foreach (var disposable in Disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Gets the custom service provider.
        /// If <see cref="SetServiceProvider"/> was used, then gets the custom <see cref="IServiceProvider"/> that was set.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <returns>A <see cref="IServiceProvider"/> instance or <see langword="null"/>.</returns>
        public static IServiceProvider GetServiceProvider(this CliExtensions ext)
        {
            return serviceProvider;
        }

        /// <summary>
        /// Sets a custom service provider built from a custom service collection (to override the internal default one).
        /// When <see cref="GetServiceProvider"/> is called, this custom <see cref="IServiceProvider"/> will be returned.
        /// </summary>
        /// <param name="ext">The CliExtensions instance to extend.</param>
        /// <param name="customServiceProvider">The custom <see cref="IServiceProvider"/> instance to use.</param>
        public static void SetServiceProvider(this CliExtensions ext, IServiceProvider customServiceProvider)
        {
            serviceProvider = customServiceProvider;

            if (customServiceProvider is IDisposable disposable)
                Disposables.Add(disposable);
        }
    }
}
