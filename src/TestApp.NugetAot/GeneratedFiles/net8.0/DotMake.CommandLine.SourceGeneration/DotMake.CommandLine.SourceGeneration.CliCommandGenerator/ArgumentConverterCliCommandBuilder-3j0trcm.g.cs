﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v1.8.8.0
// Roslyn (Microsoft.CodeAnalysis) v4.1000.24.32408
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
            NameCasingConvention = DotMake.CommandLine.CliNameCasingConvention.KebabCase;
            NamePrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.DoubleHyphen;
            ShortFormPrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.SingleHyphen;
            ShortFormAutoGenerate = true;
        }

        private TestApp.Commands.ArgumentConverterCliCommand CreateInstance()
        {
            return new TestApp.Commands.ArgumentConverterCliCommand();
        }

        /// <inheritdoc />
        public override System.CommandLine.CliCommand Build()
        {
            // Command for 'ArgumentConverterCliCommand' class
            var rootCommand = new System.CommandLine.CliRootCommand()
            {
            };

            var defaultClass = CreateInstance();

            // Option for 'Opt' property
            var option0 = new System.CommandLine.CliOption<TestApp.Commands.ClassWithConstructor>
            (
                "--opt"
            )
            {
                Required = false,
                DefaultValueFactory = _ => defaultClass.Opt,
                CustomParser = GetArgumentParser<TestApp.Commands.ClassWithConstructor>
                (
                    input => new TestApp.Commands.ClassWithConstructor(input)
                ),
            };
            option0.Aliases.Add("-o");
            rootCommand.Add(option0);

            // Option for 'OptArray' property
            var option1 = new System.CommandLine.CliOption<TestApp.Commands.ClassWithConstructor[]>
            (
                "--opt-array"
            )
            {
                AllowMultipleArgumentsPerToken = true,
                Required = false,
                DefaultValueFactory = _ => defaultClass.OptArray,
                CustomParser = GetArgumentParser<TestApp.Commands.ClassWithConstructor[], TestApp.Commands.ClassWithConstructor>
                (
                    array => (TestApp.Commands.ClassWithConstructor[])array,
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            rootCommand.Add(option1);

            // Option for 'OptNullable' property
            var option2 = new System.CommandLine.CliOption<TestApp.Commands.CustomStruct?>
            (
                "--opt-nullable"
            )
            {
                Required = false,
                DefaultValueFactory = _ => defaultClass.OptNullable,
                CustomParser = GetArgumentParser<TestApp.Commands.CustomStruct?>
                (
                    input => new TestApp.Commands.CustomStruct(input)
                ),
            };
            rootCommand.Add(option2);

            // Option for 'OptEnumerable' property
            var option3 = new System.CommandLine.CliOption<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithConstructor>>
            (
                "--opt-enumerable"
            )
            {
                Required = false,
                DefaultValueFactory = _ => defaultClass.OptEnumerable,
                CustomParser = GetArgumentParser<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithConstructor>, TestApp.Commands.ClassWithConstructor>
                (
                    array => (TestApp.Commands.ClassWithConstructor[])array,
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            rootCommand.Add(option3);

            // Option for 'OptList' property
            var option4 = new System.CommandLine.CliOption<System.Collections.Generic.List<TestApp.Commands.ClassWithConstructor>>
            (
                "--opt-list"
            )
            {
                Required = false,
                DefaultValueFactory = _ => defaultClass.OptList,
                CustomParser = GetArgumentParser<System.Collections.Generic.List<TestApp.Commands.ClassWithConstructor>, TestApp.Commands.ClassWithConstructor>
                (
                    array => new System.Collections.Generic.List<TestApp.Commands.ClassWithConstructor>((TestApp.Commands.ClassWithConstructor[])array),
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            rootCommand.Add(option4);

            // Option for 'OptCustomList' property
            var option5 = new System.CommandLine.CliOption<TestApp.Commands.CustomList<TestApp.Commands.ClassWithConstructor>>
            (
                "--opt-custom-list"
            )
            {
                Required = false,
                DefaultValueFactory = _ => defaultClass.OptCustomList,
                CustomParser = GetArgumentParser<TestApp.Commands.CustomList<TestApp.Commands.ClassWithConstructor>, TestApp.Commands.ClassWithConstructor>
                (
                    array => new TestApp.Commands.CustomList<TestApp.Commands.ClassWithConstructor>((TestApp.Commands.ClassWithConstructor[])array),
                    item => new TestApp.Commands.ClassWithConstructor(item)
                ),
            };
            rootCommand.Add(option5);

            // Argument for 'Arg' property
            var argument0 = new System.CommandLine.CliArgument<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithParser>>
            (
                "arg"
            )
            {
                CustomParser = GetArgumentParser<System.Collections.Generic.IEnumerable<TestApp.Commands.ClassWithParser>, TestApp.Commands.ClassWithParser>
                (
                    array => (TestApp.Commands.ClassWithParser[])array,
                    item => TestApp.Commands.ClassWithParser.Parse(item)
                ),
            };
            argument0.Arity = System.CommandLine.ArgumentArity.OneOrMore;
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

            rootCommand.SetAction(parseResult =>
            {
                var targetClass = (TestApp.Commands.ArgumentConverterCliCommand) Bind(parseResult);

                //  Call the command handler
                var cliContext = new DotMake.CommandLine.CliContext(parseResult);
                var exitCode = 0;
                targetClass.Run(cliContext);
                return exitCode;
            });

            return rootCommand;
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
