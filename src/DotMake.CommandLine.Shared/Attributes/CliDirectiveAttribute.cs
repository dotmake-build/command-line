using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Specifies a class property that represents a directive which is a syntactic element, that is used on the command line.
    /// <code>
    /// [CliDirective]
    /// public bool SomeCliDirective { get; set; }
    /// </code>
    /// <para>Currently only <c>bool</c>, <c>string</c> and <c>string[]</c> types are supported for <c>[CliDirective]</c> properties.</para>
    /// <para>
    /// <b>Directives:</b> <c>System.CommandLine</c> introduces a syntactic element called a directive. The <c>[diagram]</c> directive is an example.
    /// When you include <c>[diagram]</c> after the app's name, <c>System.CommandLine</c> displays a diagram of the parse result instead of
    /// invoking the command-line app:
    /// <code language="console">
    /// dotnet [diagram] build --no-restore --output ./build-output/
    ///        ^-------^
    /// </code>
    /// Output:
    /// <code language="console">
    /// [ dotnet [ build [ --no-restore &lt;True&gt; ] [ --output &lt;./build-output/&gt; ] ] ]
    /// </code>
    /// The purpose of directives is to provide cross-cutting functionality that can apply across command-line apps.
    /// Because directives are syntactically distinct from the app's own syntax, they can provide functionality that applies across apps.
    /// </para>
    /// <para>
    /// A directive must conform to the following syntax rules:
    /// <list type="bullet">
    ///     <item>It's a token on the command line that comes after the app's name but before any subcommands or options.</item>
    ///     <item>It's enclosed in square brackets.</item>
    ///     <item>It doesn't contain spaces.</item>
    /// </list>
    /// </para>
    /// <para>An unrecognized directive is ignored without causing a parsing error.</para>
    /// <para>
    /// A directive can include an argument, separated from the directive name by a colon (<c>:</c>):
    /// <code language="console">
    /// myapp [directive:value]
    /// </code>
    /// <code language="console">
    /// myapp [directive:value1] [directive:value2]
    /// </code>
    /// </para>
    /// <para>The following directives are built in (can be enabled/disabled via <see cref="CliSettings"/>): <c>[diagram]</c>, <c>[suggest]</c>, <c>[env]</c></para>
    /// </summary>
    /// <example>
    ///     <code source="..\TestApp\Commands\DirectiveCliCommand.cs" region="DirectiveCliCommand" language="cs" />
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class CliDirectiveAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the directive that will be used on the command line to specify the directive.
        /// <para>
        /// If not set (or is empty/whitespace), the name of the property that this attribute is applied to, will be used to generate directive name automatically:
        /// These suffixes will be stripped from the property name: <c>RootCliCommandDirective, RootCommandDirective, SubCliCommandDirective, SubCommandDirective, CliCommandDirective, CommandDirective, CliDirective, Directive</c>.
        /// Then the name will be converted to kebab-case, for example:
        /// <list type="bullet">
        ///     <item>If property name is <c>Debug</c> or <c>DebugDirective</c> or <c>DebugCliDirective</c> -> directive name will be <c>debug</c></item>
        ///     <item>If property name is <c>NoRestore</c> or <c>NoRestoreDirective</c> or <c>NoRestoreCliDirective</c> -> directive name will be <c>no-restore</c></item>
        /// </list>
        /// </para>
        /// <para>Default conventions can be changed via parent command's <see cref="CliCommandAttribute.NameCasingConvention"/> and <see cref="CliCommandAttribute.NamePrefixConvention"/> properties.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the directive. This will be displayed in usage help of the command line application.
        /// <para>This is not used for directives currently, but it's reserved for future use.</para>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the directive is hidden.
        /// <para>
        /// You might want to support a command, option, or argument, but avoid making it easy to discover.
        /// For example, it might be a deprecated or administrative or preview feature.
        /// Use the <see cref="Hidden"/> property to prevent users from discovering such features by using tab completion or help.
        /// </para>
        /// <para>This is not used for directives currently, but it's reserved for future use.</para>
        /// </summary>
        public bool Hidden { get; set; }


        internal static CliDirectiveAttribute Default { get; } = new CliDirectiveAttribute();
    }
}
