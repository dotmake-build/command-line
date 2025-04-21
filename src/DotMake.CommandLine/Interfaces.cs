using System;
using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Threading.Tasks;

namespace DotMake.CommandLine
{
    /// <summary>
    /// An interface to add a command handler with <c>void Run()</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRun
    {
        /// <summary>The command handler that will be called when your command is invoked.</summary>
        void Run();
    }

    /// <summary>
    /// An interface to add a command handler with <c>int Run()</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunWithReturn
    {
        /// <summary>The command handler that will be called when your command is invoked. Handler can return an exit code.</summary>
        int Run();
    }

    /// <summary>
    /// An interface to add a command handler with <c>void Run(CliContext)</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunWithContext
    {
        /// <summary>The command handler that will be called when your command is invoked. Handler receives a <see cref="CliContext"/> instance.</summary>
        void Run(CliContext cliContext);
    }

    /// <summary>
    /// An interface to add a command handler with <c>int Run(CliContext)</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunWithContextAndReturn
    {
        /// <summary>The command handler that will be called when your command is invoked. Handler receives a <see cref="CliContext"/> instance. Handler can return an exit code.</summary>
        int Run(CliContext cliContext);
    }



    /// <summary>
    /// An interface to add an async command handler with <c>Task RunAsync()</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunAsync
    {
        /// <summary>The async command handler that will be called when your command is invoked.</summary>
        Task RunAsync();
    }

    /// <summary>
    /// An interface to add an async command handler with <c>Task&lt;int&gt; RunAsync()</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunAsyncWithReturn
    {
        /// <summary>The async command handler that will be called when your command is invoked. Handler can return an exit code.</summary>
        Task<int> RunAsync();
    }

    /// <summary>
    /// An interface to add an async command handler with <c>Task RunAsync(CliContext)</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunAsyncWithContext
    {
        /// <summary>The async command handler that will be called when your command is invoked. Handler receives a <see cref="CliContext"/> instance.</summary>
        Task RunAsync(CliContext cliContext);
    }

    /// <summary>
    /// An interface to add an async command handler with <c>Task&lt;int&gt; RunAsync(CliContext)</c> signature to a command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interface can be used to prevent your IDE complain about unused method in class.
    /// </summary>
    public interface ICliRunAsyncWithContextAndReturn
    {
        /// <summary>The async command handler that will be called when your command is invoked. Handler receives a <see cref="CliContext"/> instance. Handler can return an exit code.</summary>
        Task<int> RunAsync(CliContext cliContext);
    }



    /// <summary>
    /// An interface to add custom completions for options and arguments in a command class.
    /// </summary>
    public interface ICliAddCompletions
    {
        /// <summary>
        /// This method will be called for every option and argument, you should switch according to the property name
        /// which corresponds to the option or argument whose completions will be modified.
        /// </summary>
        /// <param name="propertyName">The property name which corresponds to the current option or argument.</param>
        /// <param name="completionSources">The completion sources for the current option or argument, which will be modified.</param>
        void AddCompletions(string propertyName, List<Func<CompletionContext, IEnumerable<CompletionItem>>> completionSources);
    }
}
