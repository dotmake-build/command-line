using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Defines validation rules for a CLI argument and a CLI option's argument.
    /// </summary>
    [Flags]
    public enum CliValidationRules
    {
        /// <summary>None of the validation rules.</summary>
        None = 0,

        /// <summary>Specifies that argument value(s) must be a path to a file that already exists.</summary>
        ExistingFile = 1 << 0,   // 1

        /// <summary>Specifies that argument value(s) must be a path to a file that does not already exist.</summary>
        NonExistingFile = 1 << 1,   // 2

        /// <summary>Specifies that argument value(s) must be a path to a directory that already exists.</summary>
        ExistingDirectory = 1 << 2,   // 4

        /// <summary>Specifies that argument value(s) must be a path to a directory that does not already exist.</summary>
        NonExistingDirectory = 1 << 3,   // 8

        /// <summary>Specifies that argument value(s) must be a path to a file or a directory that already exists.</summary>
        ExistingFileOrDirectory = 1 << 4,

        /// <summary>Specifies that argument value(s) must be a path to a file or a directory that does not already exist.</summary>
        NonExistingFileOrDirectory = 1 << 5,

        /// <summary>Specifies that argument value(s) must be a legal file or directory path (i.e. must not have invalid path characters).</summary>
        LegalPath = 1 << 6,

        /// <summary>Specifies that argument value(s) must be a legal file name (i.e. must not have invalid file name characters e.g. path separators).</summary>
        LegalFileName = 1 << 7,

        /// <summary>
        /// Specifies that argument value(s) must be a legal URI.
        /// <para>
        /// Valid examples:
        /// <code>
        /// http://www.google.com
        /// ftp://ftp.is.co.za/rfc/rfc1808.txt 
        /// file:///c:/directory/filename
        /// </code>
        /// </para>
        /// </summary>
        LegalUri = 1 << 8,

        /// <summary>
        /// Specifies that argument value(s) must be a legal URL.
        /// <para>
        /// Valid examples:
        /// <code>
        /// https://www.google.com
        /// http://www.google.com
        /// www.google.com
        /// google.com
        /// </code>
        /// </para>
        /// </summary>
        LegalUrl = 1 << 9
    }
}

