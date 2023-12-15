namespace DotMake.CommandLine
{
	/// <summary>
	/// Defines the character casing conventions to use for command, option and argument names.
	/// </summary>
	public enum DotMakeCliCasingConvention
	{
		/// <summary>
		/// All characters are kept as they are (same case).
		/// </summary>
		None = 0,

		/// <summary>
		/// All characters are lowered cased (e.g. lower case).
		/// </summary>
		LowerCase,

		/// <summary>
		/// All characters are upper cased (e.g. UPPER CASE).
		/// </summary>
		UpperCase,

		/// <summary>
		/// The first character of every word is upper cased, the rest lower cased (e.g. Title Case).
		/// </summary>
		TitleCase,

		/// <summary>
		/// The first character of every word is upper cased, the rest lower cased and all spaces between words are removed (e.g. PascalCase).
		/// </summary>
		PascalCase,

		/// <summary>
		/// The first character of every word except the first one is upper cased, the rest lower cased and all spaces between words are removed (e.g. camelCase).
		/// </summary>
		CamelCase,

		/// <summary>
		/// All characters are lowered cased and all spaces between words are converted to hyphens (e.g. kebab-case).
		/// </summary>
		KebabCase,

		/// <summary>
		/// All characters are lowered cased and all spaces between words are converted to underscores (e.g. snake_case).
		/// </summary>
		SnakeCase
	}
}