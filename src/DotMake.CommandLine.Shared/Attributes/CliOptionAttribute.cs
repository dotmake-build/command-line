using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Specifies a class property that represents an option which is a named parameter and a value for that parameter, that is used on the command line.
    /// <code>
    /// [CliOption]
    /// public string SomeCliOption { get; set; }
    /// </code>
    /// <para>
    /// Note that an option is required if the decorated property does not have a default value (set via a property initializer),
    /// see <see cref="Required"/> property for details.
    /// </para>
    /// <para>
    /// <b>Options:</b> An option is a named parameter that can be passed to a command. The POSIX convention is to prefix the option name with two hyphens (<c>--</c>).
    /// The following example shows two options:
    /// <code>
    /// dotnet tool update dotnet-suggest --verbosity quiet --global
    ///                                   ^---------^       ^------^
    /// </code>
    /// As this example illustrates, the value of the option may be explicit (<c>quiet</c> for <c>--verbosity</c>) or implicit (nothing follows <c>--global</c>).
    /// Options that have no value specified are typically Boolean parameters that default to <c>true</c> if the option is specified on the command line.
    /// </para>
    /// <para>
    /// For some Windows command-line apps, you identify an option by using a leading slash (<c>/</c>) with the option name. For example:
    /// <code>
    /// msbuild /version
    ///         ^------^
    /// </code>
    /// Both POSIX and Windows prefix conventions are supported. When you configure an option, you specify the option name including the prefix.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class CliOptionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the option that will be used on the command line to specify the option.
        /// When manually setting a name (overriding target property’s name), you should specify the option name including the prefix (e.g. <c>--option</c>, <c>-option</c> or <c>/option</c>)
        /// This will be displayed in usage help of the command line application.
        /// <para>
        /// If not set (or is empty/whitespace), the name of the property that this attribute is applied to, will be used to generate option name automatically:
        /// These suffixes will be stripped from the property name: <c>RootCliCommandOption, RootCommandOption, SubCliCommandOption, SubCommandOption, CliCommandOption, CommandOption, CliOption, Option</c>.
        /// Then the name will be converted to kebab-case and will be prefixed with POSIX convention two hyphens (<c>--</c>), for example:
        /// <list type="bullet">
        ///     <item>If property name is <c>Input</c> or <c>InputOption</c> or <c>InputCliOption</c> -> option name will be <c>--input</c></item>
        ///     <item>If property name is <c>SearchPath</c> or <c>SearchPathOption</c> or <c>SearchPathCliOption</c> -> option name will be <c>--search-path</c></item>
        /// </list>
        /// </para>
        /// <para>Default conventions can be changed via parent command's <see cref="CliCommandAttribute.NameCasingConvention"/> and <see cref="CliCommandAttribute.NamePrefixConvention"/> properties.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the option. This will be displayed in usage help of the command line application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the set of alternative strings that can be used on the command line to specify the option.
        /// <para>The aliases will be also displayed in usage help of the command line application.</para>
        /// <para>
        /// When manually setting an alias, you should specify the option name including the prefix
        /// (e.g. <c>--option</c>, <c>-option</c> or <c>/option</c>)
        /// </para>
        /// </summary>
        public string[] Aliases { get; set; }

        /// <summary>
        /// Gets or sets the name of the option's argument when displayed in help.
        /// </summary>
        public string HelpName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the option is hidden.
        /// <para>
        /// You might want to support a command, option, or argument, but avoid making it easy to discover.
        /// For example, it might be a deprecated or administrative or preview feature.
        /// Use the <see cref="Hidden"/> property to prevent users from discovering such features by using tab completion or help.
        /// </para>
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the option is required when its parent command is invoked.
        /// Default is auto-detected.
        /// <para>
        /// An option/argument will be considered required when
        /// <list type="bullet">
        ///     <item>
        ///         There is no property initializer and the property type is a reference type (e.g. <c>public string Arg { get; set; }</c>). 
        ///         <c>string</c> is a reference type which has a null as the default value but <c>bool</c> and <c>enum</c> are value
        ///         types which already have non-null default values. <c>Nullable&lt;T&gt;</c> is a reference type, e.g. <c>bool?</c>.
        ///     </item>
        ///     <item>
        ///         There is a property initializer, but it's initialized with <c>null</c> or <c>null!</c> (SuppressNullableWarningExpression)
        ///         (e.g. <c>public string Arg { get; set; } = null!;</c>).
        ///     </item>
        ///     <item>If it's forced via attribute property <c>Required</c> (e.g. <c>[CliArgument(Required = true)]</c>).</item>
        ///     <item>
        ///         If it's forced via <c>required</c> modifier (e.g. <c>public required string Opt { get; set; }</c>).
        ///         Note that for being able to use <c>required</c> modifier, if your target framework is below net7.0, 
        ///         you also need <c><LangVersion>11.0</LangVersion></c> tag (minimum) in your .csproj file (our source generator supplies the polyfills
        ///         automatically as long as you set C# language version to 11).
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// An option/argument will be considered optional when
        /// <list type="bullet">
        ///     <item>
        ///         There is no property initializer (e.g. <c>public bool Opt { get; set; }</c>) but the property type is a value type 
        ///         which already have non-null default value.
        ///     </item>
        ///     <item>
        ///         There is a property initializer but it's not initialized with <c>null</c> or <c>null!</c> (SuppressNullableWarningExpression)
        ///         (e.g. <c>public string Arg { get; set; } = "Default";</c>).
        ///     </item>
        ///     <item>If it's forced via attribute property <c>Required</c> (e.g. <c>[CliArgument(Required = false)]</c>).</item>
        /// </list>
        /// </para>
        /// <para>
        /// When an option is required, the option has to be specified on the command line and if its parent command is invoked
        /// without it, an error message is displayed and the command handler isn't called.
        /// When an option is not required, the option doesn't have to be specified on the command line, the default value provides the option value.
        /// </para>
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the option is added to the owner command and recursively to all of its sub-commands.
        /// <para>Global options do not apply to parent commands.</para>
        /// </summary>
        public bool Global { get; set; }

        /// <summary>
        /// Gets or sets the arity of the option's argument. The arity refers to the number of values that can be passed on the command line.
        /// <para>In most cases setting argument arity is not necessary as it is automatically determined based on the argument type (the decorated property's type):</para>
        /// <list type="bullet">
        ///     <item>Boolean -> ArgumentArity.ZeroOrOne</item>
        ///     <item>Collection types -> ArgumentArity.ZeroOrMore</item>
        ///     <item>Everything else -> ArgumentArity.ExactlyOne</item>
        /// </list>
        /// </summary>
        public CliArgumentArity Arity { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed values for an option.
        /// <para>Configures an option to accept only the specified values, and to suggest them as command line completions.</para>
        /// <para>Note that if the option's argument type is an enum, values are automatically added.</para>
        /// </summary>
        public string[] AllowedValues { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an option should accept only values corresponding to an existing file or directory.
        /// <list type="bullet">
        ///     <item>When option's argument type is <see cref="System.IO.FileInfo"/>, existing file will be checked.</item>
        ///     <item>When option's argument type is <see cref="System.IO.DirectoryInfo"/>, existing directory will be checked.</item>
        ///     <item>When option's argument type is <see cref="System.IO.FileSystemInfo"/> or any other type, existing file or directory will be checked.</item>
        /// </list>
        /// </summary>
        public bool AllowExisting { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether multiple argument tokens are allowed for each option identifier token.
        /// <para>
        /// By default, when you call a command, you can repeat an option name to specify multiple arguments for an option that has maximum arity greater than one.
        /// <code>myapp --items one --items two --items three</code>
        /// To allow multiple arguments without repeating the option name, set <see cref="AllowMultipleArgumentsPerToken"/> to <see langword="true" />. This setting lets you enter the following command line.
        /// <code>myapp --items one two three</code>
        /// The same setting has a different effect if maximum argument arity is 1. It allows you to repeat an option but takes only the last value on the line. In the following example, the value <c>three</c> would be passed to the app.
        /// <code>myapp --item one --item two --item three</code>
        /// </para>
        /// </summary>
        public bool AllowMultipleArgumentsPerToken { get; set; }

        internal static CliOptionAttribute Default { get; } = new CliOptionAttribute();
    }
}
