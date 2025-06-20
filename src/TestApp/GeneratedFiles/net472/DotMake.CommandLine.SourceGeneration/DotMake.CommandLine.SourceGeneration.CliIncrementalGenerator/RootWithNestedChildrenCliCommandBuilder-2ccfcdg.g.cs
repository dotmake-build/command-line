﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v2.4.3.0
// Roslyn (Microsoft.CodeAnalysis) v4.1400.25.27905
// Generation: 1

namespace TestApp.Commands.GeneratedCode
{
    /// <inheritdoc />
    public class RootWithNestedChildrenCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
    {
        /// <inheritdoc />
        public RootWithNestedChildrenCliCommandBuilder()
        {
            DefinitionType = typeof(TestApp.Commands.RootWithNestedChildrenCliCommand);
            ParentDefinitionType = null;
            ChildDefinitionTypes = null;
            NameCasingConvention = null;
            NamePrefixConvention = null;
            ShortFormPrefixConvention = null;
            ShortFormAutoGenerate = null;
        }

        private TestApp.Commands.RootWithNestedChildrenCliCommand CreateInstance()
        {
            return new TestApp.Commands.RootWithNestedChildrenCliCommand();
        }

        /// <inheritdoc />
        public override System.CommandLine.Command Build()
        {
            // Command for 'RootWithNestedChildrenCliCommand' class
            var command = IsRoot
                ? new System.CommandLine.RootCommand()
                : new System.CommandLine.Command(GetCommandName("RootWithNestedChildren"));
            command.Description = "A root cli command with nested children";

            // Option for 'Option1' property
            var option0 = new System.CommandLine.Option<string>
            (
                GetOptionName("Option1")
            )
            {
                Description = "Description for Option1",
                Required = false,
                DefaultValueFactory = _ => "DefaultForOption1",
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            AddShortFormAlias(option0);
            command.Add(option0);

            // Argument for 'Argument1' property
            var argument0 = new System.CommandLine.Argument<string>
            (
                GetArgumentName("Argument1")
            )
            {
                Description = "Description for Argument1",
                DefaultValueFactory = _ => "DefaultForArgument1",
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            command.Add(argument0);

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

            command.SetAction(parseResult =>
            {
                var targetClass = (TestApp.Commands.RootWithNestedChildrenCliCommand) Bind(parseResult);

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
            var commandBuilder = new TestApp.Commands.GeneratedCode.RootWithNestedChildrenCliCommandBuilder();

            // Register this command builder so that it can be found by the definition class
            // and it can be found by the parent definition class if it's a nested/external child.
            commandBuilder.Register();
        }

        /// <inheritdoc />
        public class Level1SubCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
        {
            /// <inheritdoc />
            public Level1SubCliCommandBuilder()
            {
                DefinitionType = typeof(TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand);
                ParentDefinitionType = typeof(TestApp.Commands.RootWithNestedChildrenCliCommand);
                ChildDefinitionTypes = null;
                NameCasingConvention = null;
                NamePrefixConvention = null;
                ShortFormPrefixConvention = null;
                ShortFormAutoGenerate = null;
            }

            private TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand CreateInstance()
            {
                return new TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand();
            }

            /// <inheritdoc />
            public override System.CommandLine.Command Build()
            {
                // Command for 'Level1SubCliCommand' class
                var command = IsRoot
                    ? new System.CommandLine.RootCommand()
                    : new System.CommandLine.Command(GetCommandName("Level1"));
                command.Description = "A nested level 1 sub-command";

                // Option for 'Option1' property
                var option0 = new System.CommandLine.Option<string>
                (
                    GetOptionName("Option1")
                )
                {
                    Description = "Description for Option1",
                    Required = false,
                    DefaultValueFactory = _ => "DefaultForOption1",
                    CustomParser = GetArgumentParser<string>
                    (
                        null
                    ),
                };
                AddShortFormAlias(option0);
                command.Add(option0);

                // Argument for 'Argument1' property
                var argument0 = new System.CommandLine.Argument<string>
                (
                    GetArgumentName("Argument1")
                )
                {
                    Description = "Description for Argument1",
                    CustomParser = GetArgumentParser<string>
                    (
                        null
                    ),
                };
                command.Add(argument0);

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

                command.SetAction(parseResult =>
                {
                    var targetClass = (TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand) Bind(parseResult);

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
                var commandBuilder = new TestApp.Commands.GeneratedCode.RootWithNestedChildrenCliCommandBuilder.Level1SubCliCommandBuilder();

                // Register this command builder so that it can be found by the definition class
                // and it can be found by the parent definition class if it's a nested/external child.
                commandBuilder.Register();
            }

            /// <inheritdoc />
            public class Level2SubCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
            {
                /// <inheritdoc />
                public Level2SubCliCommandBuilder()
                {
                    DefinitionType = typeof(TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand.Level2SubCliCommand);
                    ParentDefinitionType = typeof(TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand);
                    ChildDefinitionTypes = null;
                    NameCasingConvention = null;
                    NamePrefixConvention = null;
                    ShortFormPrefixConvention = null;
                    ShortFormAutoGenerate = null;
                }

                private TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand.Level2SubCliCommand CreateInstance()
                {
                    return new TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand.Level2SubCliCommand();
                }

                /// <inheritdoc />
                public override System.CommandLine.Command Build()
                {
                    // Command for 'Level2SubCliCommand' class
                    var command = IsRoot
                        ? new System.CommandLine.RootCommand()
                        : new System.CommandLine.Command(GetCommandName("Level2"));
                    command.Description = "A nested level 2 sub-command";

                    // Option for 'Option1' property
                    var option0 = new System.CommandLine.Option<string>
                    (
                        GetOptionName("Option1")
                    )
                    {
                        Description = "Description for Option1",
                        Required = false,
                        DefaultValueFactory = _ => "DefaultForOption1",
                        CustomParser = GetArgumentParser<string>
                        (
                            null
                        ),
                    };
                    AddShortFormAlias(option0);
                    command.Add(option0);

                    // Argument for 'Argument1' property
                    var argument0 = new System.CommandLine.Argument<string>
                    (
                        GetArgumentName("Argument1")
                    )
                    {
                        Description = "Description for Argument1",
                        CustomParser = GetArgumentParser<string>
                        (
                            null
                        ),
                    };
                    command.Add(argument0);

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

                    command.SetAction(parseResult =>
                    {
                        var targetClass = (TestApp.Commands.RootWithNestedChildrenCliCommand.Level1SubCliCommand.Level2SubCliCommand) Bind(parseResult);

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
                    var commandBuilder = new TestApp.Commands.GeneratedCode.RootWithNestedChildrenCliCommandBuilder.Level1SubCliCommandBuilder.Level2SubCliCommandBuilder();

                    // Register this command builder so that it can be found by the definition class
                    // and it can be found by the parent definition class if it's a nested/external child.
                    commandBuilder.Register();
                }
            }
        }
    }
}
