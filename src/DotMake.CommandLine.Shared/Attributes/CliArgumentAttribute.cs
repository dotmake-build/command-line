using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Specifies a class property that represents an argument which is a value that can be passed on the command line to a command or an option.
    /// <para>
    /// <b>Arguments:</b> An argument is a value passed to an option or a command. The following examples show an argument for the <c>verbosity</c> option and an argument for the <c>build</c> command.
    /// <code>
    /// dotnet tool update dotnet-suggest --verbosity quiet --global
    ///                                               ^---^
    /// </code>
    /// <code>
    /// dotnet build myapp.csproj
    ///              ^----------^
    /// </code>
    /// Arguments can have default values that apply if no argument is explicitly provided. For example, many options are implicitly Boolean parameters with a default of <c>true</c> when the option name is in the command line.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CliArgumentAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the argument that will be used mainly for displaying in usage help of the command line application.
        /// <para>
        /// If not set (or is empty/whitespace), the name of the property that this attribute is applied to, will be used to generate argument name automatically:
        /// These suffixes will be stripped from the property name: <c>RootCliCommandArgument, RootCommandArgument, SubCliCommandArgument, SubCommandArgument, CliCommandArgument, CommandArgument, CliArgument, Argument</c>.
        /// Then the name will be converted to kebab-case, for example:
        /// <list type="bullet">
        ///     <item>If property name is <c>Output</c> or <c>OutputArgument</c> or <c>OutputCliArgument</c> -> argument name will be <c>output</c></item>
        ///     <item>If property name is <c>ProjectPath</c> or <c>ProjectPathArgument</c> or <c>ProjectPathCliArgument</c> -> argument name will be <c>project-path</c></item>
        /// </list>
        /// </para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the argument. This will be displayed in usage help of the command line application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the argument when displayed in help.
        /// </summary>
        public string HelpName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the argument is hidden.
        /// <para>
        /// You might want to support a command, option, or argument, but avoid making it easy to discover.
        /// For example, it might be a deprecated or administrative or preview feature.
        /// Use the <see cref="Hidden"/> property to prevent users from discovering such features by using tab completion or help.
        /// </para>
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the argument is required when its parent command is invoked.
        /// Default is <see langword="true" /> for arguments.
        /// <para>
        /// When an argument is required and its parent command is invoked without it,
        /// an error message is displayed and the command handler isn't called.
        /// </para>
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Gets or sets the arity of the argument. The arity refers to the number of values that can be passed on the command line.
        /// <para>In most cases setting argument arity is not necessary as it is automatically determined based on the argument type:</para>
        /// <list type="bullet">
        ///		<item>Boolean -> ArgumentArity.ZeroOrOne</item>
        ///		<item>Collection types -> ArgumentArity.ZeroOrMore</item>
        ///		<item>Everything else -> ArgumentArity.ExactlyOne</item>
        /// </list>
        /// </summary>
        public CliArgumentArity Arity { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed values for an argument.
        /// <para>Configures an argument to accept only the specified values, and to suggest them as command line completions.</para>
        /// <para>Note that if the argument type is an enum, values are automatically added.</para>
        /// </summary>
        public string[] AllowedValues { get; set; }

        internal static CliArgumentAttribute Default { get; } = new CliArgumentAttribute();
    }
}
