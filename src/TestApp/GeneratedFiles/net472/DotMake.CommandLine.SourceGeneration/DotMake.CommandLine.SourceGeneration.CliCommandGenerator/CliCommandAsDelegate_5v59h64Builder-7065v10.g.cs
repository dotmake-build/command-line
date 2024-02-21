﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v1.8.1.0
// Roslyn (Microsoft.CodeAnalysis) v4.900.24.8111
// Generation: 1

namespace GeneratedCode
{
    /// <inheritdoc />
    public class CliCommandAsDelegate_5v59h64Builder : DotMake.CommandLine.CliCommandBuilder
    {
        /// <inheritdoc />
        public CliCommandAsDelegate_5v59h64Builder()
        {
            DefinitionType = typeof(GeneratedCode.CliCommandAsDelegate_5v59h64);
            ParentDefinitionType = null;
            NameCasingConvention = DotMake.CommandLine.CliNameCasingConvention.KebabCase;
            NamePrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.DoubleHyphen;
            ShortFormPrefixConvention = DotMake.CommandLine.CliNamePrefixConvention.SingleHyphen;
            ShortFormAutoGenerate = true;
        }

        private GeneratedCode.CliCommandAsDelegate_5v59h64 CreateInstance()
        {
            return new GeneratedCode.CliCommandAsDelegate_5v59h64();
        }

        /// <inheritdoc />
        public override System.CommandLine.CliCommand Build()
        {
            // Command for 'CliCommandAsDelegate_5v59h64' class
            var rootCommand = new System.CommandLine.CliRootCommand()
            {
            };

            var defaultClass = CreateInstance();

            // Option for 'option1' property
            var option0 = new System.CommandLine.CliOption<bool>
            (
                "--option-1"
            )
            {
                Required = false,
            };
            option0.CustomParser = GetParseArgument<bool>
            (
                null
            );
            option0.DefaultValueFactory = _ => defaultClass.option1;
            option0.Aliases.Add("-o");
            rootCommand.Add(option0);

            // Argument for 'argument1' property
            var argument0 = new System.CommandLine.CliArgument<string>
            (
                "argument-1"
            )
            {
            };
            argument0.CustomParser = GetParseArgument<string>
            (
                null
            );
            rootCommand.Add(argument0);

            // Add nested or external registered children
            foreach (var child in Children)
            {
                rootCommand.Add(child.Build());
            }

            BindFunc = (parseResult) =>
            {
                var targetClass = CreateInstance();

                //  Set the parsed or default values for the options
                targetClass.option1 = GetValueForOption(parseResult, option0);

                //  Set the parsed or default values for the arguments
                targetClass.argument1 = GetValueForArgument(parseResult, argument0);

                return targetClass;
            };

            rootCommand.SetAction(parseResult =>
            {
                var targetClass = (GeneratedCode.CliCommandAsDelegate_5v59h64) BindFunc(parseResult);

                //  Call the command handler
                var cliContext = new DotMake.CommandLine.CliContext(parseResult);
                var exitCode = 0;
                targetClass.Run();
                return exitCode;
            });

            return rootCommand;
        }

        [System.Runtime.CompilerServices.ModuleInitializerAttribute]
        internal static void Initialize()
        {
            var commandBuilder = new GeneratedCode.CliCommandAsDelegate_5v59h64Builder();

            // Register this command builder so that it can be found by the definition class
            // and it can be found by the parent definition class if it's a nested/external child.
            commandBuilder.Register();
        }
    }
}
