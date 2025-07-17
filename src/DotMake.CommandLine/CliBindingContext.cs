using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Context used during binding of commands.
    /// </summary>
    public class CliBindingContext
    {
        private readonly ConcurrentDictionary<Tuple<ParseResult, Type>, object> bindResults = new();

        /// <summary>
        /// Delegates for command binders which are set by the source generator to be called from <see cref="Bind(ParseResult, Type)"/> method.
        /// </summary>
        public Dictionary<Type, Func<ParseResult, object>> Binders { get; } = new();


        /// <inheritdoc cref="Bind" />
        /// <typeparam name="TDefinition"><inheritdoc cref="Cli.GetParser{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        public TDefinition Bind<TDefinition>(ParseResult parseResult)
        {
            var definitionType = typeof(TDefinition);

            return (TDefinition)Bind(parseResult, definitionType);
        }

        /// <summary>
        /// Creates a new instance of the command definition class and binds/populates the properties from the parse result.
        /// Note that binding will be done only once per parse result and definition class, so calling this method consecutively for
        /// the same parse result and the same definition class will return the cached result.
        /// <para>
        /// If the command line input is not for the indicated definition class (e.g. it's for a sub-command but not for
        /// the indicated root command or vice versa), then the returned instance would be empty (i.e. properties would have default values).
        /// </para>
        /// </summary>
        /// <param name="parseResult">A parse result describing the outcome of the parse operation.</param>
        /// <param name="definitionType"><inheritdoc cref="Cli.GetParser" path="/param[@name='definitionType']/node()" /></param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public object Bind(ParseResult parseResult, Type definitionType)
        {
            return bindResults.GetOrAdd(Tuple.Create(parseResult, definitionType), CallBinder);
        }

        private object CallBinder(Tuple<ParseResult, Type> key)
        {
            var parseResult = key.Item1;
            var definitionType = key.Item2;

            if (!Binders.TryGetValue(definitionType, out var binder))
                throw new Exception($"Binder is not found for definition type '{definitionType.Name}'. Ensure Cli.Run or Cli.Parse method is called first (command should be build first).");

            return binder(parseResult);
        }
    }
}
