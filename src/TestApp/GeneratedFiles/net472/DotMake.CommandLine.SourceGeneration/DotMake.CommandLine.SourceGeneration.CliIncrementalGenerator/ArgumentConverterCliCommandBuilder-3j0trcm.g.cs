﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v2.4.3.0
// Roslyn (Microsoft.CodeAnalysis) v4.1400.25.27905
// Generation: 1

namespace TestApp.Commands.GeneratedCode
{
    /// <inheritdoc />
    public class ArgumentConverterCliCommandBuilder : DotMake.CommandLine.CliCommandBuilder
    {
        /// <inheritdoc />
        public ArgumentConverterCliCommandBuilder()
        {
            DefinitionType = typeof(TestApp.Commands.ArgumentConverterCliCommand);
            ParentDefinitionType = null;
            ChildDefinitionTypes = null;
            NameCasingConvention = null;
            NamePrefixConvention = null;
            ShortFormPrefixConvention = null;
            ShortFormAutoGenerate = null;
        }

        private TestApp.Commands.ArgumentConverterCliCommand CreateInstance()
        {
            return new TestApp.Commands.ArgumentConverterCliCommand();
        }

        /// <inheritdoc />
        public override System.CommandLine.Command Build()
        {
            // Command for 'ArgumentConverterCliCommand' class
            var command = IsRoot
                ? new System.CommandLine.RootCommand()
                : new System.CommandLine.Command(GetCommandName("ArgumentConverter"));

            // Option for 'Opt' property
            var option0 = new System.CommandLine.Option<TestApp.Commands.ClassWithConstructor>
            (
                GetOptionName("Opt")
            )
            {
                Required = false,
                CustomParser = GetArgumentParser<TestApp.Commands.ClassWithConstructor>
                (
                    input => new TestApp.Commands.ClassWithConstructor(input)
                ),
            };
            AddShortFormAlias(option0);
            command.Add(option0);

            // Option for 'OptArray' property
            var option1 = new System.CommandLine.Option<TestApp.Commands.ClassWithConstructor[]>
            (
                GetOptionName("OptArray")
            )
            {
                AllowMultipleArgumentsPerToken = true,
                Required = false,
                CustomParser = GetArgumentParser<TestApp.Commands.ClassWithConstructor[], TestApp.Commands.ClassWithConstructor>
                (
                    array => (TestApp.Commands.ClassWithConstructor[])array,
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            AddShortFormAlias(option1);
            command.Add(option1);

            // Option for 'OptNullable' property
            var option2 = new System.CommandLine.Option<TestApp.Commands.CustomStruct?>
            (
                GetOptionName("OptNullable")
            )
            {
                Required = false,
                CustomParser = GetArgumentParser<TestApp.Commands.CustomStruct?>
                (
                    input => new TestApp.Commands.CustomStruct(input)
                ),
            };
            AddShortFormAlias(option2);
            command.Add(option2);

            // Option for 'OptEnumerable' property
            var option3 = new System.CommandLine.Option<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithConstructor>>
            (
                GetOptionName("OptEnumerable")
            )
            {
                Required = false,
                CustomParser = GetArgumentParser<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithConstructor>, TestApp.Commands.ClassWithConstructor>
                (
                    array => (TestApp.Commands.ClassWithConstructor[])array,
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            AddShortFormAlias(option3);
            command.Add(option3);

            // Option for 'OptList' property
            var option4 = new System.CommandLine.Option<System.Collections.Generic.List<TestApp.Commands.ClassWithConstructor>>
            (
                GetOptionName("OptList")
            )
            {
                Required = false,
                CustomParser = GetArgumentParser<System.Collections.Generic.List<TestApp.Commands.ClassWithConstructor>, TestApp.Commands.ClassWithConstructor>
                (
                    array => new System.Collections.Generic.List<TestApp.Commands.ClassWithConstructor>((TestApp.Commands.ClassWithConstructor[])array),
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            AddShortFormAlias(option4);
            command.Add(option4);

            // Option for 'OptCustomList' property
            var option5 = new System.CommandLine.Option<TestApp.Commands.CustomList<TestApp.Commands.ClassWithConstructor>>
            (
                GetOptionName("OptCustomList")
            )
            {
                Required = false,
                CustomParser = GetArgumentParser<TestApp.Commands.CustomList<TestApp.Commands.ClassWithConstructor>, TestApp.Commands.ClassWithConstructor>
                (
                    array => new TestApp.Commands.CustomList<TestApp.Commands.ClassWithConstructor>((TestApp.Commands.ClassWithConstructor[])array),
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            AddShortFormAlias(option5);
            command.Add(option5);

            // Argument for 'Arg' property
            var argument0 = new System.CommandLine.Argument<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithParser>>
            (
                GetArgumentName("Arg")
            )
            {
                CustomParser = GetArgumentParser<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithParser>, TestApp.Commands.ClassWithParser>
                (
                    array => (TestApp.Commands.ClassWithParser[])array,
                    item => TestApp.Commands.ClassWithParser.Parse(item)
                ),
            };
            argument0.Arity = System.CommandLine.ArgumentArity.OneOrMore;
            command.Add(argument0);

            Binder = (parseResult) =>
            {
                var targetClass = CreateInstance();

                //  Set the parsed or default values for the options
                targetClass.Opt = GetValueForOption(parseResult, option0);
                targetClass.OptArray = GetValueForOption(parseResult, option1);
                targetClass.OptNullable = GetValueForOption(parseResult, option2);
                targetClass.OptEnumerable = GetValueForOption(parseResult, option3);
                targetClass.OptList = GetValueForOption(parseResult, option4);
                targetClass.OptCustomList = GetValueForOption(parseResult, option5);

                //  Set the parsed or default values for the arguments
                targetClass.Arg = GetValueForArgument(parseResult, argument0);

                //  Set the values for the parent command accessors

                return targetClass;
            };

            command.SetAction(parseResult =>
            {
                var targetClass = (TestApp.Commands.ArgumentConverterCliCommand) Bind(parseResult);

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
            var commandBuilder = new TestApp.Commands.GeneratedCode.ArgumentConverterCliCommandBuilder();

            // Register this command builder so that it can be found by the definition class
            // and it can be found by the parent definition class if it's a nested/external child.
            commandBuilder.Register();
        }
    }
}
