using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using DotMake.CommandLine.Binding;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Context used for binding of commands to definition classes.
    /// </summary>
    public class CliBindingContext
    {
        private readonly Dictionary<Tuple<ParseResult, Type>, object> bindCache = new();
        private const string ErrorCommon = "Ensure Cli.Run, Cli.Parse or Cli.GetParser method is called for self or a definition class in the same command hierarchy.";

        /// <summary>
        /// Map for looking up definition classes by commands, which are set by the source generator to be called from <see cref="IsCalled{TDefinition}"/> or <see cref="Contains{TDefinition}"/> methods.
        /// </summary>
        public Dictionary<Command, Type> CommandMap { get; } = new();

        /// <summary>
        /// Map for looking up instance creators by definition classes, which are set by the source generator to be called from <see cref="Bind{TDefinition}"/> method.
        /// </summary>
        public Dictionary<Type, Func<object>> CreatorMap { get; } = new();

        /// <summary>
        /// Map for looking up command binder delegates by definition classes, which are set by the source generator to be called from <see cref="Bind{TDefinition}"/> method.
        /// </summary>
        public Dictionary<Type, Action<object, ParseResult>> BinderMap { get; } = new();


        /// <inheritdoc cref="Create" />
        /// <typeparam name="TDefinition"><inheritdoc cref="Cli.GetParser{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        public TDefinition Create<TDefinition>()
        {
            var definitionType = typeof(TDefinition);

            return (TDefinition)Create(definitionType);
        }
        
        /// <summary>
        /// Creates a new instance of the command definition class but without any binding.
        /// This is useful for example when you need to instantiate a definition class when using dependency injection.
        /// </summary>
        /// <returns>An instance of the definition class.</returns>
        /// <inheritdoc cref="Bind" />
        public object Create(Type definitionType)
        {
            if (!CreatorMap.TryGetValue(definitionType, out var creator))
                throw new Exception($"Creator is not found for definition class '{definitionType.Name}'. {ErrorCommon}");

            return creator();
        }


        /// <inheritdoc cref="Bind" />
        /// <typeparam name="TDefinition"><inheritdoc cref="Cli.GetParser{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        public TDefinition Bind<TDefinition>(ParseResult parseResult, bool returnEmpty = false)
        {
            var definitionType = typeof(TDefinition);

            return (TDefinition)Bind(parseResult, definitionType, returnEmpty);
        }

        /// <summary>
        /// Creates a new instance of the command definition class and binds/populates the properties from the parse result.
        /// <para>
        /// Note that binding will be done only once per parse result and definition class, so calling this method consecutively for
        /// the same parse result and the same definition class will return the cached result.
        /// </para>
        /// <para>
        /// If the command line input does not contain the indicated definition class (as self or as a parent), this method will return <see langword="null" />
        /// unless <paramref name="returnEmpty"/> is set to <see langword="true"/>, in that case it will return an empty instance with default property values.
        /// </para>
        /// </summary>
        /// <param name="parseResult">A parse result describing the outcome of the parse operation.</param>
        /// <param name="definitionType"><inheritdoc cref="Cli.GetParser" path="/param[@name='definitionType']/node()" /></param>
        /// <param name="returnEmpty">
        /// Whether to return an empty instance with default property values instead of <see langword="null" />
        /// if the command line input does not contain the indicated definition class (as self or as a parent).
        /// </param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public object Bind(ParseResult parseResult, Type definitionType, bool returnEmpty = false)
        {
            //To prevent circular dependency, we separate creating and binding of the definition instance.
            //This is because while setting command accessor properties, Bind<TDefinition> is called again which
            //can cause circular dependency due to recursive calls,
            //for example when parent and child has command accessors that point to each other.

            var key = Tuple.Create(parseResult, definitionType);
            if (bindCache.TryGetValue(key, out var value))
                return value;

            if (!returnEmpty && !Contains(parseResult, definitionType))
                return null;

            var definitionInstance = Create(definitionType);

            //We immediately add definitionInstance to cache so it's available in recursive calls inside binder below.
            bindCache.Add(key, definitionInstance);

            if (!BinderMap.TryGetValue(definitionType, out var binder))
                throw new Exception($"Binder is not found for definition class '{definitionType.Name}'. {ErrorCommon}");

            binder(definitionInstance, parseResult);

            return definitionInstance;
        }


        /// <summary>
        /// Creates a new instance of the definition class for called command and binds/populates the properties from the parse result.
        /// <para>
        /// Note that binding will be done only once per parse result and definition class, so calling this method consecutively for
        /// the same parse result and the same definition class will return the cached result.
        /// </para>
        /// </summary>
        /// <inheritdoc cref="Bind" />
        public object BindCalled(ParseResult parseResult)
        {
            var currentCommandResult = parseResult.CommandResult;
            var currentCommand = currentCommandResult.Command;

            if (!CommandMap.TryGetValue(currentCommand, out var currentDefinitionType))
                throw new Exception($"Definition class is not found for command '{currentCommand.Name}'. {ErrorCommon}");

            return Bind(parseResult, currentDefinitionType);
        }

        /// <summary>
        /// Creates a new instance of the definition class for all contained commands (self and parents) and binds/populates the properties from the parse result.
        /// <para>
        /// Note that binding will be done only once per parse result and definition class, so calling this method consecutively for
        /// the same parse result and the same definition class will return the cached result.
        /// </para>
        /// </summary>
        /// <inheritdoc cref="Bind" />
        public object[] BindAll(ParseResult parseResult)
        {
            var currentCommandResult = parseResult.CommandResult;
            var list = new List<object>();

            while (currentCommandResult != null)
            {
                var currentCommand = currentCommandResult.Command;

                if (!CommandMap.TryGetValue(currentCommand, out var currentDefinitionType))
                    throw new Exception($"Definition class is not found for command '{currentCommand.Name}'. {ErrorCommon}");

                list.Add(Bind(parseResult, currentDefinitionType));

                currentCommandResult = currentCommandResult.Parent as CommandResult;
            }

            return list.ToArray();
        }


        /// <inheritdoc cref="IsCalled" />
        /// <typeparam name="TDefinition"><inheritdoc cref="Cli.GetParser{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        public bool IsCalled<TDefinition>(ParseResult parseResult)
        {
            var definitionType = typeof(TDefinition);

            return IsCalled(parseResult, definitionType);
        }

        /// <summary>
        /// Checks if the command line input is for the indicated definition class.
        /// </summary>
        /// <param name="parseResult"><inheritdoc cref="Bind" path="/param[@name='parseResult']/node()" /></param>
        /// <param name="definitionType"><inheritdoc cref="Bind" path="/param[@name='definitionType']/node()" /></param>
        public bool IsCalled(ParseResult parseResult, Type definitionType)
        {
            var currentCommandResult = parseResult.CommandResult;
            var currentCommand = currentCommandResult.Command;

            if (CommandMap.TryGetValue(currentCommand, out var currentDefinitionType)
                && currentDefinitionType == definitionType)
                return true;

            return false;
        }

        /// <inheritdoc cref="Contains" />
        /// <typeparam name="TDefinition"><inheritdoc cref="Cli.GetParser{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        public bool Contains<TDefinition>(ParseResult parseResult)
        {
            var definitionType = typeof(TDefinition);

            return Contains(parseResult, definitionType);
        }

        /// <summary>
        /// Checks if the command line input contains the indicated definition class (as self or as a parent).
        /// </summary>
        /// <param name="parseResult"><inheritdoc cref="Bind" path="/param[@name='parseResult']/node()" /></param>
        /// <param name="definitionType"><inheritdoc cref="Bind" path="/param[@name='definitionType']/node()" /></param>
        public bool Contains(ParseResult parseResult, Type definitionType)
        {
            var currentCommandResult = parseResult.CommandResult;

            while (currentCommandResult != null)
            {
                var currentCommand = currentCommandResult.Command;

                if (CommandMap.TryGetValue(currentCommand, out var currentDefinitionType)
                    && currentDefinitionType == definitionType)
                    return true;

                currentCommandResult = currentCommandResult.Parent as CommandResult;
            }

            return false;
        }


        /// <summary>
        /// Gets an argument parser method for an argument type, if it's a collection type.
        /// <para>
        /// This is mainly used for adding support for all <see cref="IEnumerable{T}"/> compatible types which have
        /// a public constructor with a <see cref="IEnumerable{T}"/> or <see cref="IList{T}"/> parameter (other parameters, if any, should be optional).
        /// </para>
        /// </summary>
        /// <param name="convertFromArray">A delegate which creates an instance of collection type from an array.</param>
        /// <param name="convertFromString">A delegate which creates an instance of item type from a string.</param>
        /// <typeparam name="TCollection">The collection type, the argument type itself.</typeparam>
        /// <typeparam name="TItem">The item type, e.g. if argument type is IEnumerable&lt;T&gt;, item type will be T.</typeparam>
        /// <returns>A delegate which can be passed to an option or argument.</returns>
        public Func<ArgumentResult, TCollection> GetArgumentParser<TCollection, TItem>(Func<Array, TCollection> convertFromArray, Func<string, TItem> convertFromString = null)
        {
            ArgumentConverter.RegisterCollectionConverter(convertFromArray);
            ArgumentConverter.RegisterStringConverter(convertFromString);

            return GetArgumentParser<TCollection>();
        }

        /// <summary>
        /// Gets an argument parser method for an argument type.
        /// <para>
        /// This is mainly used for adding support for binding custom types which have a public constructor
        /// or a static <c>Parse</c> method with a string parameter (other parameters, if any, should be optional).
        /// </para>
        /// </summary>
        /// <param name="convertFromString">A delegate which creates an instance of custom type from a string.</param>
        /// <typeparam name="TArgument">The argument type.</typeparam>
        /// <returns>A delegate which can be passed to an option or argument.</returns>
        public Func<ArgumentResult, TArgument> GetArgumentParser<TArgument>(Func<string, TArgument> convertFromString = null)
        {
            ArgumentConverter.RegisterStringConverter(convertFromString);

            return (result) =>
            {
                var tryConvertArgument = ArgumentConverter.GetConverter(result.Argument);

                if (tryConvertArgument == null)
                {
                    result.AddError($"No argument converter found for type '{result.Argument.ValueType}'");
                    return default; // Ignored.
                }

                tryConvertArgument(result, out var value);

                return value != null
                    ? (TArgument)value
                    : default;
            };
        }


        /// <summary>
        /// Gets the parsed or default value for the specified directive.
        /// <para>
        /// Extended version for DotMake CLI which can bind custom classes,
        /// does not fall back to internal ArgumentConverter.GetDefaultValue which does not support all IList compatible types.
        /// </para>
        /// </summary>
        /// <param name="parseResult">The parse result.</param>
        /// <param name="directive">The directive for which to get a value.</param>
        /// <typeparam name="T">The option type.</typeparam>
        /// <returns>The parsed value or a configured default.</returns>
        public T GetValue<T>(ParseResult parseResult, Directive directive)
        {
            var result = parseResult.GetResult(directive);
            if (result != null)
            {
                var type = typeof(T).GetNullableUnderlyingTypeOrSelf();
                if (type == typeof(bool))
                    return (T)(object)true;
                if (type == typeof(string))
                    return (T)(object)(result.Values.FirstOrDefault() ?? string.Empty);
                if (type == typeof(string[]))
                    return (T)(object)result.Values.ToArray();

                throw new Exception("Currently only 'bool', 'string' and 'string[]' types are supported for [CliDirective] properties.");
            }

            return (T)ArgumentConverter.GetDefaultValue(typeof(T));
        }

        /// <inheritdoc cref="GetValue{T}(ParseResult, Directive)"/>
        public object GetValue(ParseResult parseResult, Directive directive)
        {
            /*
            var result = parseResult.GetResult(directive);
            if (result != null)
            {
                var value = result.GetValueOrDefault<object>();
                if (value != null)
                    return value;
            }

            return ArgumentConverter.GetDefaultValue(directive.GetArgument().ValueType);
            */
            return GetValue<object>(parseResult, directive);
        }

        /// <summary>
        /// Gets the parsed or default value for the specified option.
        /// <para>
        /// Extended version for DotMake CLI which can bind custom classes,
        /// does not fall back to internal ArgumentConverter.GetDefaultValue which does not support all IList compatible types.
        /// </para>
        /// </summary>
        /// <param name="parseResult">The parse result.</param>
        /// <param name="option">The option for which to get a value.</param>
        /// <typeparam name="T">The option type.</typeparam>
        /// <returns>The parsed value or a configured default.</returns>
        public T GetValue<T>(ParseResult parseResult, Option<T> option)
        {
            var result = parseResult.GetResult(option);
            if (result != null)
            {
                //Note that there is a result when there is a DefaultValueFactory and if there is no error (e.g. other required options are not missing)
                var value = result.GetValueOrDefault<T>();
                if (value != null)
                    return value;
            }

            return (T)ArgumentConverter.GetDefaultValue(typeof(T));
        }

        /// <inheritdoc cref="GetValue{T}(ParseResult, Option{T})"/>
        public object GetValue(ParseResult parseResult, Option option)
        {
            var result = parseResult.GetResult(option);
            if (result != null)
            {
                var value = result.GetValueOrDefault<object>();
                if (value != null)
                    return value;
            }

            return ArgumentConverter.GetDefaultValue(option.GetArgument().ValueType);
        }

        /// <summary>
        /// Gets the parsed or default value for the specified argument.
        /// <para>
        /// Extended version for DotMake CLI which can bind custom classes,
        /// does not fall back to internal ArgumentConverter.GetDefaultValue which does not support all IList compatible types.
        /// </para>
        /// </summary>
        /// <param name="parseResult">The parse result.</param>
        /// <param name="argument">The argument for which to get a value.</param>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <returns>The parsed value or a configured default.</returns>
        public T GetValue<T>(ParseResult parseResult, Argument<T> argument)
        {
            var result = parseResult.GetResult(argument);
            if (result != null)
            {
                //Note that there is a result when there is a DefaultValueFactory and if there is no error (e.g. other required options are not missing)
                var value = result.GetValueOrDefault<T>();
                if (value != null)
                    return value;
            }

            return (T)ArgumentConverter.GetDefaultValue(typeof(T));
        }

        /// <inheritdoc cref="GetValue{T}(ParseResult, Argument{T})"/>
        public object GetValue(ParseResult parseResult, Argument argument)
        {
            var result = parseResult.GetResult(argument);
            if (result != null)
            {
                var value = result.GetValueOrDefault<object>();
                if (value != null)
                    return value;
            }

            return ArgumentConverter.GetDefaultValue(argument.ValueType);
        }
    }
}
