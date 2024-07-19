﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v1.8.8.0
// Roslyn (Microsoft.CodeAnalysis) v4.1000.24.32408
// Generation: 1

namespace TestApp.Commands.GeneratedCode
{
    /// <inheritdoc />
    public class HelpCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
    {
        /// <inheritdoc />
        public HelpCliCommandBuilder()
        {
            DefinitionType = typeof(TestApp.Commands.HelpCliCommand);
            ParentDefinitionType = null;
            NameCasingConvention = DotMake.CommandLine.CliNameCasingConvention.KebabCase;
            NamePrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.DoubleHyphen;
            ShortFormPrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.SingleHyphen;
            ShortFormAutoGenerate = true;
        }

        private TestApp.Commands.HelpCliCommand CreateInstance()
        {
            return new TestApp.Commands.HelpCliCommand();
        }

        /// <inheritdoc />
        public override System.CommandLine.CliCommand Build()
        {
            // Command for 'HelpCliCommand' class
            var rootCommand = new System.CommandLine.CliRootCommand()
            {
                Description = "A root cli command",
            };

            var defaultClass = CreateInstance();

            // Option for 'Option1' property
            var option0 = new System.CommandLine.CliOption<string>
            (
                "--option-1"
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
            option0.Aliases.Add("-o");
            rootCommand.Add(option0);

            // Argument for 'Argument1' property
            var argument0 = new System.CommandLine.CliArgument<string>
            (
                "argument-1"
            )
            {
                Description = "Description for Argument1",
                DefaultValueFactory = _ => defaultClass.Argument1,
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            rootCommand.Add(argument0);

            // Add nested or external registered children
            foreach (var child in Children)
            {
                rootCommand.Add(child.Build());
            }

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
                var targetClass = (TestApp.Commands.HelpCliCommand) Bind(parseResult);

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
            var commandBuilder = new TestApp.Commands.GeneratedCode.HelpCliCommandBuilder();

            // Register this command builder so that it can be found by the definition class
            // and it can be found by the parent definition class if it's a nested/external child.
            commandBuilder.Register();
        }

        /// <inheritdoc />
        public class SubCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
        {
            /// <inheritdoc />
            public SubCliCommandBuilder()
            {
                DefinitionType = typeof(TestApp.Commands.HelpCliCommand.SubCliCommand);
                ParentDefinitionType = typeof(TestApp.Commands.HelpCliCommand);
                NameCasingConvention = DotMake.CommandLine.CliNameCasingConvention.KebabCase;
                NamePrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.DoubleHyphen;
                ShortFormPrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.SingleHyphen;
                ShortFormAutoGenerate = true;
            }

            private TestApp.Commands.HelpCliCommand.SubCliCommand CreateInstance()
            {
                return new TestApp.Commands.HelpCliCommand.SubCliCommand();
            }

            /// <inheritdoc />
            public override System.CommandLine.CliCommand Build()
            {
                // Command for 'SubCliCommand' class
                var command = new System.CommandLine.CliCommand("sub")
                {
                    Description = "A sub cli command",
                };

                var defaultClass = CreateInstance();

                // Argument for 'Argument2' property
                var argument0 = new System.CommandLine.CliArgument<string>
                (
                    "argument-2"
                )
                {
                    Description = "Description for Argument2",
                    DefaultValueFactory = _ => defaultClass.Argument2,
                    CustomParser = GetArgumentParser<string>
                    (
                        null
                    ),
                };
                command.Add(argument0);

                // Add nested or external registered children
                foreach (var child in Children)
                {
                    command.Add(child.Build());
                }

                Binder = (parseResult) =>
                {
                    var targetClass = CreateInstance();

                    //  Set the parsed or default values for the options

                    //  Set the parsed or default values for the arguments
                    targetClass.Argument2 = GetValueForArgument(parseResult, argument0);

                    //  Set the values for the parent command accessors

                    return targetClass;
                };

                command.SetAction(parseResult =>
                {
                    var targetClass = (TestApp.Commands.HelpCliCommand.SubCliCommand) Bind(parseResult);

                    //  Call the command handler
                    var cliContext = new DotMake.CommandLine.CliContext(parseResult);
                    var exitCode = 0;
                    targetClass.Run(cliContext);
                    return exitCode;
                });

                return command;
            }

            [System.Runtime.CompilerServices.ModuleInitializerAttribute]
            internal static void Initialize()
            {
                var commandBuilder = new TestApp.Commands.GeneratedCode.HelpCliCommandBuilder.SubCliCommandBuilder();

                // Register this command builder so that it can be found by the definition class
                // and it can be found by the parent definition class if it's a nested/external child.
                commandBuilder.Register();
            }
        }
    }
}
