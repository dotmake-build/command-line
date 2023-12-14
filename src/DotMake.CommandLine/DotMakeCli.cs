﻿using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace DotMake.CommandLine
{
	/// <summary>
	/// Provides methods for parsing command line input and running an indicated command.
	/// </summary>
	public class DotMakeCli
	{
		/// <summary>
		/// Gets a command line builder for the indicated command.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="configureBuilder">A delegate to further configure the command line builder.</param>
		/// <param name="useBuilderDefaults">
		/// Whether to use the default DotMakeCli configuration for the command line builder.
		/// Default is <see langword="true" />.
		/// <para>
		/// Setting this value to <see langword="true" /> is the equivalent to calling:
		/// <code>
		///   commandLineBuilder
		///     .UseVersionOption(commandBuilder.NamePrefixConvention,
		///			commandBuilder.ShortFormPrefixConvention,
		///			commandBuilder.ShortFormAutoGenerate)
		///		.UseHelp(commandBuilder.NamePrefixConvention)
		///     .UseEnvironmentVariableDirective()
		///     .UseParseDirective()
		///     .UseSuggestDirective()
		///     .RegisterWithDotnetSuggest()
		///     .UseTypoCorrections()
		///     .UseParseErrorReporting()
		///     .CancelOnProcessTermination();
		/// </code>
		/// </para>
		/// </param>
		/// <returns>A <see cref="CommandLineBuilder" /> providing details about command line configurations.</returns>
		public static CommandLineBuilder GetBuilder<TDefinition>(Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true)
		{
			var commandBuilder = DotMakeCommandBuilder.Get<TDefinition>();
			var command = commandBuilder.Build();
			var commandLineBuilder = new CommandLineBuilder(command);

			if (useBuilderDefaults)
			{
				commandLineBuilder
					.UseVersionOption(commandBuilder.NamePrefixConvention,
						commandBuilder.ShortFormPrefixConvention,
						commandBuilder.ShortFormAutoGenerate)
					.UseHelp(commandBuilder.NamePrefixConvention)
					.UseEnvironmentVariableDirective()
					.UseParseDirective()
					.UseSuggestDirective()
					.RegisterWithDotnetSuggest()
					.UseTypoCorrections()
					.UseParseErrorReporting()
					//.UseExceptionHandler() //This registers an exception handler even with null parameter so disabling it to let exceptions go through.
					.CancelOnProcessTermination();
			}

			if (configureBuilder != null)
				configureBuilder(commandLineBuilder);

			return commandLineBuilder;
		}

		/*
		Note about documentation, VS intellisense wants
			/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		but SHFB wants (for some reason VS is confused for second parameter, repeats the first one's contents)
			/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		This happens probably the method is generic?
		*/

		/// <summary>
		/// Parses a command line string value and runs the handler for the indicated command.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
		/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		/// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
		/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		/// <param name="console">A console to which output can be written. By default, <see cref="Console" /> is used.</param>
		/// <returns>The exit code for the invocation.</returns>
		public static int Run<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
		{
			var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

			return parser.Invoke(commandLine, console);
		}

		/// <summary>
		/// Parses a command line string array and runs the handler for the indicated command.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
		/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		/// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
		/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		/// <param name="console">A console to which output can be written. By default, <see cref="Console" /> is used.</param>
		/// <returns>The exit code for the invocation.</returns>
		public static int Run<TDefinition>(string[] args, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
		{
			var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

			return parser.Invoke(args, console);
		}

		/// <summary>
		/// Parses a command line string value and runs the handler for the indicated command.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
		/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		/// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
		/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		/// <param name="console">A console to which output can be written. By default, <see cref="System.Console" /> is used.</param>
		/// <returns>The exit code for the invocation.</returns>
		public static async Task<int> RunAsync<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
		{
			var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

			return await parser.InvokeAsync(commandLine, console);
		}

		/// <summary>
		/// Parses a command line string array and runs the handler for the indicated command.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
		/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		/// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
		/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		/// <param name="console">A console to which output can be written. By default, <see cref="System.Console" /> is used.</param>
		/// <returns>The exit code for the invocation.</returns>
		public static async Task<int> RunAsync<TDefinition>(string[] args, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
		{
			var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

			return await parser.InvokeAsync(args, console);
		}

		/// <summary>
		/// Parses a command line string.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
		/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		/// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
		/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		/// <returns>A <see cref="ParseResult" /> providing details about the parse operation.</returns>
		public static ParseResult Parse<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true)
		{
			var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

			return parser.Parse(commandLine);
		}

		/// <summary>
		/// Parses a list of arguments.
		/// </summary>
		/// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
		/// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
		/// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
		/// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
		/// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
		/// <returns>A <see cref="ParseResult" /> providing details about the parse operation.</returns>
		public static ParseResult Parse<TDefinition>(string[] args, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true)
		{
			var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

			return parser.Parse(args);
		}
	}
}