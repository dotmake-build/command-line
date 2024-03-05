using System;
using System.Collections.Generic;
using System.CommandLine;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Context used during binding of commands
    /// </summary>
    public class CliBindContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliBindContext" /> class.
        /// </summary>
        /// <param name="parseResult">A parse result describing the outcome of the parse operation.</param>
        public CliBindContext(ParseResult parseResult)
        {
            ParseResult = parseResult;
        }

        /// <summary>A parse result describing the outcome of the parse operation.</summary>
        public ParseResult ParseResult { get; }

        /// <summary>
        /// Creates a new instance of the definition class and binds/populates the properties from the parse result,
        /// or returns a cached instance of the definition class earlier returned from either BindOrGetBindResult() overload.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class.</typeparam>
        /// <returns></returns>
        public TDefinition BindOrGetBindResult<TDefinition>() => (TDefinition)BindOrGetBindResult(typeof(TDefinition));

        /// <summary>
        /// Creates a new instance of the definition class and binds/populates the properties from the parse result,
        /// or returns a cached instance of the definition class earlier returned from either BindOrGetBindResult() overload.
        /// </summary>
        /// <param name="commandDefinitionType">The type of the definition class.</param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public object BindOrGetBindResult(Type commandDefinitionType)
        {
            if (bindResults.TryGetValue(commandDefinitionType, out object bindResult))
            {
                return bindResult;
            }
            var commandBuilder = CliCommandBuilder.Get(commandDefinitionType);
            object commandObj = commandBuilder.Bind(this);
            bindResults[commandDefinitionType] = commandObj;
            return commandObj;
        }

        private readonly Dictionary<Type, object> bindResults = new();
    }
}
