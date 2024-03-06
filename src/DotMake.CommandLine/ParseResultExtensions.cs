using System;
using System.Collections.Concurrent;
using System.CommandLine;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="ParseResult"/>. 
    /// </summary>
    public static class ParseResultExtensions
    {
        private static readonly ConcurrentDictionary<ParseResult, ConcurrentDictionary<Type, object>> BindResults = new();

        /// <inheritdoc cref = "Bind{TDefinition}" />
        /// <typeparam name="TDefinition"><inheritdoc cref="Cli.GetConfiguration{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        public static TDefinition Bind<TDefinition>(this ParseResult parseResult)
        {
            var commandBuilder = CliCommandBuilder.Get<TDefinition>();

            return (TDefinition)commandBuilder.Bind(parseResult);
        }

        /// <summary>
        /// Creates a new instance of the command definition class and binds/populates the properties from the parse result.
        /// Note that binding will be done only once per definition class, so calling this method consecutively for
        /// the same definition class will return the cached result.
        /// <para>
        /// If the command line input is not for the indicated definition class (e.g. it's for a sub-command but not for
        /// the indicated root command or vice versa), then the returned instance would be empty (i.e. properties would have default values).
        /// </para>
        /// </summary>
        /// <param name="parseResult">A parse result describing the outcome of the parse operation.</param>
        /// <param name="definitionType"><inheritdoc cref="Cli.GetConfiguration(Type, CliSettings)" path="/param[@name='definitionType']/node()" /></param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public static object Bind(this ParseResult parseResult, Type definitionType)
        {
            var commandBuilder = CliCommandBuilder.Get(definitionType);

            return commandBuilder.Bind(parseResult);
        }
    }
}
