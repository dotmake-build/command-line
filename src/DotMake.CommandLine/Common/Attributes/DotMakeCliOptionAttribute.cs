using System;

namespace DotMake.CommandLine
{
	/// <summary>
	/// Specifies a class property that represents an option which is a named parameter and a value for that parameter, that is used on the command line.
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
	[AttributeUsage(AttributeTargets.Property)]
	public class DotMakeCliOptionAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the option that will be used on the command line to specify the option.
		/// When manually setting a name (overriding target property’s name), you should specify the option name including the prefix (e.g. <c>--option</c>, <c>-option</c> or <c>/option</c>)
		/// This will be displayed in usage help of the command line application.
		/// <para>
		/// If not set (or is empty/whitespace), the name of the property that this attribute is applied to, will be used to generate option name automatically:
		/// These suffixes will be stripped from the property name: <c>RootCliCommandOption, RootCommandOption, SubCliCommandOption, SubCommandOption, CliCommandOption, CommandOption, CliOption, Option</c>.
		/// Then the name will be converted to kebab-case and will be prefixed with POSIX convention two hyphens (<c>--</c>) (default is changed via parent <see cref="DotMakeCliCommandAttribute.NamePrefixConvention"/>), for example:
		/// <list type="bullet">
		///     <item>If property name is <c>Input</c> or <c>InputOption</c> or <c>InputCliOption</c> -> option name will be <c>--input</c></item>
		///     <item>If property name is <c>SearchPath</c> or <c>SearchPathOption</c> or <c>SearchPathCliOption</c> -> option name will be <c>--search-path</c></item>
		/// </list>
		/// </para>
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
		/// <para>
		/// When an option is required and its parent command is invoked without it,
		/// an error message is displayed and the command handler isn't called.
		/// </para>
		/// <para>
		/// If a required option has a default value, the option doesn't have to be specified on the command line.
		/// In that case, the default value provides the required option value.
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
		/// <para>In most cases setting argument arity is not necessary as it is automatically determined based on the argument type:</para>
		/// <list type="bullet">
		///		<item>Boolean -> ArgumentArity.ZeroOrOne</item>
		///		<item>Collection types -> ArgumentArity.ZeroOrMore</item>
		///		<item>Everything else -> ArgumentArity.ExactlyOne</item>
		/// </list>
		/// </summary>
		public DotMakeCliArgumentArity Arity { get; set; }

		/// <summary>
		/// Gets or sets the list of allowed values for an option.
		/// <para>Configures an option to accept only the specified values, and to suggest them as command line completions.</para>
		/// <para>Note that if the option's argument type is an enum, values are automatically added.</para>
		/// </summary>
		public string[] AllowedValues { get; set; }

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
	}
}