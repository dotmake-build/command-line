namespace DotMake.CommandLine
{
    /// <summary>
    /// Defines the prefix conventions to use for option names and aliases.
    /// </summary>
    public enum CliNamePrefixConvention
    {
        /// <summary>
        /// Option name is prefixed with one hyphen (<c>-</c>) which is POSIX prefix convention, usually used for short form option aliases (e.g. <c>-o</c> or <c>-option</c>).
        /// </summary>
        SingleHyphen,

        /// <summary>
        /// Option name is prefixed with two hyphens (<c>--</c>) which is POSIX prefix convention, usually used for long form option names (e.g. <c>--option</c>).
        /// </summary>
        DoubleHyphen,

        /// <summary>
        /// Option name is prefixed with a forward slash (<c>/</c>) which is Windows prefix convention (e.g. <c>/o</c> or <c>/option</c>).
        /// </summary>
        ForwardSlash,
    }
}
