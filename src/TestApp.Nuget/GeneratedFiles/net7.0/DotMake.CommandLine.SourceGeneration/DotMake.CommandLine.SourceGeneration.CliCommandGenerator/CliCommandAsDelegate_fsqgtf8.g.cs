﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v1.8.4.0
// Roslyn (Microsoft.CodeAnalysis) v4.900.24.8111
// Generation: 1

namespace GeneratedCode
{
    /// <inheritdoc />
    [DotMake.CommandLine.CliCommandAttribute]
    public class CliCommandAsDelegate_fsqgtf8 : DotMake.CommandLine.CliCommandAsDelegate
    {
        /// <inheritdoc />
        [DotMake.CommandLine.CliArgumentAttribute]
        public string argument2 { get; set; }

        /// <inheritdoc />
        [DotMake.CommandLine.CliOptionAttribute]
        public bool option2 { get; set; }

        /// <inheritdoc />
        public async System.Threading.Tasks.Task RunAsync()
        {
            await (System.Threading.Tasks.Task)InvokeDelegate
            (
                "fsqgtf8",
                new object[]
                {
                    argument2, 
                    option2, 
                }
            );
        }

        [System.Runtime.CompilerServices.ModuleInitializerAttribute]
        internal static void Initialize()
        {
            // Register this definition class so that it can be found by the command as delegate hash.
            Register<GeneratedCode.CliCommandAsDelegate_fsqgtf8>("fsqgtf8");
        }
    }
}
