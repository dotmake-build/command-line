﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v2.0.0.0
// Roslyn (Microsoft.CodeAnalysis) v4.1300.25.16703
// Generation: 1

namespace TestApp.Commands.PrefixConvention.GeneratedCode
{
    /// <inheritdoc />
    public class ForwardSlashCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
    {
        /// <inheritdoc />
        public ForwardSlashCliCommandBuilder()
        {
            DefinitionType = typeof(TestApp.Commands.PrefixConvention.ForwardSlashCliCommand);
            ParentDefinitionType = null;
            NameCasingConvention = DotMake.CommandLine.CliNameCasingConvention.KebabCase;
            NamePrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.ForwardSlash;
            ShortFormPrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.ForwardSlash;
            ShortFormAutoGenerate = true;
        }

        private TestApp.Commands.PrefixConvention.ForwardSlashCliCommand CreateInstance()
        {
            return new TestApp.Commands.PrefixConvention.ForwardSlashCliCommand();
        }

        /// <inheritdoc />
        public override System.CommandLine.Command Build()
        {
            // Command for 'ForwardSlashCliCommand' class
            var rootCommand = new System.CommandLine.RootCommand()
            {
                Description = "A cli command with forward slash prefix convention",
            };

            var defaultClass = CreateInstance();

            // Option for 'Option1' property
            var option0 = new System.CommandLine.Option<string>
            (
                "/option-1"
            )
            {
                Description = "Description for Option1",
                Required = false,
                DefaultValueFactory = _ => defaultClass.Option1,
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            option0.Aliases.Add("/o");
            rootCommand.Add(option0);

            // Argument for 'Argument1' property
            var argument0 = new System.CommandLine.Argument<string>
            (
                "argument-1"
            )
            {
                Description = "Description for Argument1",
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            rootCommand.Add(argument0);

            Binder = (parseResult) =>
            {
                var targetClass = CreateInstance();

                //  Set the parsed or default values for the options
                targetClass.Option1 = GetValueForOption(parseResult, option0);

                //  Set the parsed or default values for the arguments
                targetClass.Argument1 = GetValueForArgument(parseResult, argument0);

                //  Set the values for the parent command accessors

                return targetClass;
            };

            rootCommand.SetAction(parseResult =>
            {
                var targetClass = (TestApp.Commands.PrefixConvention.ForwardSlashCliCommand) Bind(parseResult);

                //  Call the command handler
                var cliContext = new DotMake.CommandLine.CliContext(parseResult);
                var exitCode = 0;
                cliContext.ShowHelp();
                return exitCode;
            });

            return rootCommand;
        }

        [System.Runtime.CompilerServices.ModuleInitializerAttribute]
        internal static void Initialize()
        {
            var commandBuilder = new TestApp.Commands.PrefixConvention.GeneratedCode.ForwardSlashCliCommandBuilder();

            // Register this command builder so that it can be found by the definition class
            // and it can be found by the parent definition class if it's a nested/external child.
            commandBuilder.Register();
        }
    }
}
