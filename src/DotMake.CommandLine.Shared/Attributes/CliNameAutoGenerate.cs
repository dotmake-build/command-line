using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Defines the CLI symbol types for auto-generated names or short form aliases.
    /// </summary>
    [Flags]
    public enum CliNameAutoGenerate
    {
        /// <summary>
        /// Do not auto-generate names or short form aliases.
        /// </summary>
        None = 0,

        /// <summary>
        /// Auto-generate names for directives.
        /// </summary>
        Directives = 1 << 0,   // 1

        /// <summary>
        /// Auto-generate names or short form aliases for commands.
        /// </summary>
        Commands = 1 << 1,   // 2

        /// <summary>
        /// Auto-generate names or short form aliases for options.
        /// </summary>
        Options = 1 << 2,   // 4

        /// <summary>
        /// Auto-generate names for arguments.
        /// </summary>
        Arguments = 1 << 3,   // 8

        /// <summary>
        /// Auto-generate names or short form aliases (if supported) for all CLI symbol types.
        /// </summary>
        All = Directives | Commands | Options | Arguments
    }
}
