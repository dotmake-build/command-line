using System;

namespace DotMake.CommandLine
{
	/// <summary>
	/// Specifies a class that represents a command which is a specific action that the command line application performs.
	/// The class that this attribute is applied to, 
	/// <list type="bullet">
	///     <item>will be a root command if the class is not a nested class and <see cref="Parent"/> property is not set.</item>
	///     <item>will be a sub command if the class is a nested class or <see cref="Parent"/> property is set.</item>
	/// </list>
	/// <para>
	/// <b>Commands:</b> A command in command-line input is a token that specifies an action or defines a group of related actions. For example:
	/// <list type="bullet">
	///     <item>In <c>dotnet run</c>, <c>run</c> is a command that specifies an action.</item>
	///     <item>In <c>dotnet tool install</c>, <c>install</c> is a command that specifies an action, and <c>tool</c> is a command that specifies a <br/>
	///		group of related commands. There are other tool-related commands, such as <c>tool uninstall</c>, <c>tool list</c>,<br/>
	///		and <c>tool update</c>.</item>
	/// </list>
	/// </para>
	/// <para>
	/// <b>Root commands:</b> The root command is the one that specifies the name of the app's executable. For example, the <c>dotnet</c> command specifies the <c>dotnet.exe</c> executable.
	/// </para>
	/// <para>
	/// <b>Subcommands:</b> Most command-line apps support subcommands, also known as verbs. For example, the <c>dotnet</c> command has a <c>run</c> subcommand that you invoke by entering <c>dotnet run</c>.
	/// Subcommands can have their own subcommands. In <c>dotnet tool install</c>, <c>install</c> is a <c>subcommand</c> of tool.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CliCommandAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the name of the command that will be used on the command line to specify the command.
		/// This will be displayed in usage help of the command line application.
		/// <para>
		/// If not set (or is empty/whitespace), the name of the class that this attribute is applied to, will be used to generate command name automatically:
		/// These suffixes will be stripped from the class name: <c>RootCliCommand, RootCommand, SubCliCommand, SubCommand, CliCommand, Command, Cli</c>.
		/// Then the name will be converted to kebab-case, for example:
		/// <list type="bullet">
		///     <item>If class name is <c>Build</c> or <c>BuildCommand</c> or <c>BuildRootCliCommand</c> -> command name will be <c>build</c></item>
		///     <item>If class name is <c>BuildText</c> or <c>BuildTextCommand</c> or <c>BuildTextSubCliCommand</c> -> command name will be <c>build-text</c></item>
		/// </list>
		/// </para>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the description of the command. This will be displayed in usage help of the command line application.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the set of alternative strings that can be used on the command line to specify the command.
		/// <para>The aliases will be also displayed in usage help of the command line application.</para>
		/// </summary>
		public string[] Aliases { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the command is hidden.
		/// <para>
		/// You might want to support a command, option, or argument, but avoid making it easy to discover.
		/// For example, it might be a deprecated or administrative or preview feature.
		/// Use the <see cref="Hidden"/> property to prevent users from discovering such features by using tab completion or help.
		/// </para>
		/// </summary>
		public bool Hidden { get; set; }

		/// <summary>
		/// Gets or sets the parent of the command. This property is used when you prefer to use a non-nested class for a subcommand,
		/// i.e. when you want to separate root command and subcommands into different classes/files.
		/// If the class that this attribute is applied to, is already a nested class, then this property will be ignored.
		/// </summary>
		public Type Parent { get; set; }

		/// <summary>
		/// Gets or sets a value that indicates whether unmatched tokens should be treated as errors. For example,
		/// if set to <see langword="true" /> and an extra command or argument is provided, validation will fail.
		/// <para>Default is <see langword="true" />.</para>
		/// </summary>
		public bool TreatUnmatchedTokensAsErrors { get; set; }

		/// <summary>
		/// Gets or sets the character casing convention to use for automatically generated command, option and argument names.
		/// This setting will be inherited by child options, child arguments and subcommands.
		/// <para>Default is <see cref="CliNameCasingConvention.KebabCase"/> (e.g. <c>kebab-case</c>).</para>
		/// </summary>
		public CliNameCasingConvention NameCasingConvention { get; set; } = CliNameCasingConvention.KebabCase;

		/// <summary>
		/// Gets or sets the prefix convention to use for automatically generated option names.
		/// This setting will be inherited by child options and subcommands.
		/// <para>Default is <see cref="CliNamePrefixConvention.DoubleHyphen"/> (e.g. <c>--option</c>).</para>
		/// </summary>
		public CliNamePrefixConvention NamePrefixConvention { get; set; } = CliNamePrefixConvention.DoubleHyphen;

		/// <summary>
		/// Gets or sets the prefix convention to use for automatically generated short form option aliases.
		/// Short forms typically have a leading delimiter followed by a single character (e.g. <c>-o</c> or <c>--o</c> or <c>/o</c>).
		/// This setting will be inherited by child options and subcommands.
		/// <para>Default is <see cref="CliNamePrefixConvention.SingleHyphen"/> (e.g. <c>-o</c>).</para>
		/// </summary>
		public CliNamePrefixConvention ShortFormPrefixConvention { get; set; } = CliNamePrefixConvention.SingleHyphen;

		/// <summary>
		/// Gets or sets a value which indicates whether short form aliases are automatically generated for options.
		/// Short forms typically have a leading delimiter followed by a single character (e.g. <c>-o</c> or <c>--o</c> or <c>/o</c>).
		/// Default delimiter (e.g. <c>-o</c>) is changed via <see cref="ShortFormPrefixConvention"/>.
		/// This setting will be inherited by child options and subcommands.
		/// <para>Default is <see langword="true" />.</para>
		/// </summary>
		public bool ShortFormAutoGenerate { get; set; } = true;

		/// <summary>
		/// Gets the default instance of <see cref="CliCommandAttribute" />.
		/// </summary>
		/// <value>The default instance of <see cref="CliCommandAttribute" />.</value>
		public static CliCommandAttribute Default { get; } = new CliCommandAttribute();
	}
}