﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v2.4.3.0
// Roslyn (Microsoft.CodeAnalysis) v4.1400.25.27905
// Generation: 1

namespace TestApp.Commands.GeneratedCode
{
    /// <inheritdoc />
    public class InheritanceCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
    {
        /// <inheritdoc />
        public InheritanceCliCommandBuilder()
        {
            DefinitionType = typeof(TestApp.Commands.InheritanceCliCommand);
            ParentDefinitionType = null;
            ChildDefinitionTypes = null;
            NameCasingConvention = null;
            NamePrefixConvention = null;
            ShortFormPrefixConvention = null;
            ShortFormAutoGenerate = null;
        }

        private TestApp.Commands.InheritanceCliCommand CreateInstance()
        {
            return new TestApp.Commands.InheritanceCliCommand();
        }

        /// <inheritdoc />
        public override System.CommandLine.Command Build()
        {
            // Command for 'InheritanceCliCommand' class
            var command = IsRoot
                ? new System.CommandLine.RootCommand()
                : new System.CommandLine.Command(GetCommandName("Inheritance"));

            // Option for 'Username' property
            var option0 = new System.CommandLine.Option<string>
            (
                GetOptionName("Username")
            )
            {
                Description = "Username of the identity performing the command",
                Required = false,
                DefaultValueFactory = _ => "admin",
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            AddShortFormAlias(option0);
            command.Add(option0);

            // Option for 'Password' property
            var option1 = new System.CommandLine.Option<string>
            (
                GetOptionName("Password")
            )
            {
                Description = "Password of the identity performing the command",
                Required = true,
                CustomParser = GetArgumentParser<string>
                (
                    null
                ),
            };
            AddShortFormAlias(option1);
            command.Add(option1);

            // Argument for 'Department' property
            var argument0 = new System.CommandLine.Argument<string>
            (
                GetArgumentName("Department")
            )
            {
                Description = "Department of the identity performing the command (interface)",
                DefaultValueFactory = _ => "Accounting",
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
                targetClass.Username = GetValueForOption(parseResult, option0);
                targetClass.Password = GetValueForOption(parseResult, option1);

                //  Set the parsed or default values for the arguments
                targetClass.Department = GetValueForArgument(parseResult, argument0);

                //  Set the values for the parent command accessors

                return targetClass;
            };

            command.SetAction(parseResult =>
            {
                var targetClass = (TestApp.Commands.InheritanceCliCommand) Bind(parseResult);

                //  Call the command handler
                var cliContext = new DotMake.CommandLine.CliContext(parseResult);
                var exitCode = 0;
                targetClass.Run();

                return exitCode;
            });

            return command;
        }

        [System.Runtime.CompilerServices.ModuleInitializerAttribute]
        internal static void Initialize()
        {
            var commandBuilder = new TestApp.Commands.GeneratedCode.InheritanceCliCommandBuilder();

            // Register this command builder so that it can be found by the definition class
            // and it can be found by the parent definition class if it's a nested/external child.
            commandBuilder.Register();
        }
    }
}
