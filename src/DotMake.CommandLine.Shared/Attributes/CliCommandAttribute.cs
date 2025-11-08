using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Specifies a class that represents a command which is a specific action that the command line application performs.
    /// <code>
    /// [CliCommand]
    /// public class SomeCliCommand
    /// </code>
    /// The class that this attribute is applied to, 
    /// <list type="bullet">
    ///     <item>will be a root command if the class is not a nested class and other's <see cref="Children"/> property and self's <see cref="Parent"/> property is not set.</item>
    ///     <item>will be a sub command if the class is a nested class or other's <see cref="Children"/> property or self's <see cref="Parent"/> property is set.</item>
    /// </list>
    /// <para>
    /// <b>Commands:</b> A command in command-line input is a token that specifies an action or defines a group of related actions. For example:
    /// <list type="bullet">
    ///     <item>In <c>dotnet run</c>, <c>run</c> is a command that specifies an action.</item>
    ///     <item>In <c>dotnet tool install</c>, <c>install</c> is a command that specifies an action, and <c>tool</c> is a command that specifies a <br/>
    ///         group of related commands. There are other tool-related commands, such as <c>tool uninstall</c>, <c>tool list</c>,<br/>
    ///         and <c>tool update</c>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Root commands:</b> The root command is the one that specifies the name of the app's executable. For example, the <c>dotnet</c> command specifies the <c>dotnet.exe</c> executable.
    /// </para>
    /// <para>
    /// <b>Subcommands:</b> Most command-line apps support subcommands, also known as verbs. For example, the <c>dotnet</c> command has a <c>run</c> subcommand that you invoke by entering <c>dotnet run</c>.
    /// Subcommands can have their own subcommands. In <c>dotnet tool install</c>, <c>install</c> is a <c>subcommand</c> of tool.
    /// </para>
    /// <para>
    /// <b>Inheritance:</b> When you have repeating/common options and arguments for your commands, you can define them once in a base class and then 
    /// share them by inheriting that base class in other command classes.Interfaces are also supported !
    /// </para>
    /// <para>
    /// <b>Handler:</b> Add a method with name Run or RunAsync to make it the handler for the CLI command. The method can have one of the following signatures:
    /// <list type="bullet">
    ///     <item><c>void Run()</c></item>
    ///     <item><c>int Run()</c></item>
    ///     <item><c>async Task RunAsync()</c></item>
    ///     <item><c>async Task&lt;int&gt; RunAsync()</c></item>
    /// </list>
    /// Optionally the method signature can have a <see cref="CliContext"/> parameter in case you need to access it:
    /// <list type="bullet">
    ///     <item><c>Run(CliContext context)</c></item>
    ///     <item><c>RunAsync(CliContext context)</c></item>
    /// </list>
    /// </para>
    /// <para>
    /// We also provide interfaces <see cref="ICliRun"/>, <see cref="ICliRunWithReturn"/>, <see cref="ICliRunWithContext"/>, <see cref="ICliRunWithContextAndReturn"/>
    /// and async versions <see cref="ICliRunAsync"/>, <see cref="ICliRunAsyncWithReturn"/>, <see cref="ICliRunAsyncWithContext"/>, <see cref="ICliRunAsyncWithContextAndReturn"/> 
    /// that you can inherit in your command class.
    /// Normally you don't need an interface for a handler method as the source generator can detect it automatically,
    /// but the interfaces can be used to prevent your IDE complain about unused method in class.
    /// </para>
    /// <para>
    /// The signatures which return int value, sets the ExitCode of the app.
    /// If no handler method is provided, then by default it will show help for the command.
    /// This can be also controlled manually by extension method <see cref="CliContext.ShowHelp"/>.
    /// Other extension methods <see cref="CliContext.IsEmptyCommand"/>, <see cref="CliContext.ShowValues"/> and <see cref="CliContext.ShowHierarchy"/> are also useful.
    ///  </para>
    /// </summary>
    /// <example>
    ///     <inheritdoc cref="Cli" path="/example/code[@id='gettingStartedClass']" />
    ///     <inheritdoc cref="Cli" path="/example/code[@id='gettingStartedClass2']" />
    ///     <code>
    ///         <code source="../TestApp/Commands/RunAsyncCliCommand.cs" region="RunAsyncCliCommand" language="cs" />
    ///         <code source="../TestApp/Commands/RunAsyncWithReturnCliCommand.cs" region="RunAsyncWithReturnCliCommand" language="cs" />
    ///     </code>
    ///     <code source="../TestApp/Commands/WriteFileCliCommand.cs" region="WriteFileCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/ArgumentConverterCliCommand.cs" region="ArgumentConverterCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/EnumerableCliCommand.cs" region="EnumerableCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/RootSnakeSlashCliCommand.cs" region="RootSnakeSlashCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/RootWithNestedChildrenCliCommand.cs" region="RootWithNestedChildrenCliCommand" language="cs" />
    ///     <code>
    ///         <code source="../TestApp/Commands/RootWithExternalChildrenCliCommand.cs" region="RootWithExternalChildrenCliCommand" language="cs" />
    ///         <code source="../TestApp/Commands/External\ExternalLevel1SubCliCommand.cs" region="ExternalLevel1SubCliCommand" language="cs" />
    ///         <code source="../TestApp/Commands/External\ExternalLevel2SubCliCommand.cs" region="ExternalLevel2SubCliCommand" language="cs" />
    ///     </code>
    ///     <code>
    ///         <code source="../TestApp/Commands/RootAsExternalParentCliCommand.cs" region="RootAsExternalParentCliCommand" language="cs" />
    ///         <code source="../TestApp/Commands/External\ExternalLevel1WithParentSubCliCommand.cs" region="ExternalLevel1WithParentSubCliCommand" language="cs" />
    ///         <code source="../TestApp/Commands/External\ExternalLevel2WithParentSubCliCommand.cs" region="ExternalLevel2WithParentSubCliCommand" language="cs" />
    ///     </code>
    ///     <code source="../TestApp/Commands/InheritanceCliCommand.cs" region="InheritanceCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/LocalizedCliCommand.cs" region="LocalizedCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/HelpCliCommand.cs" region="HelpCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/ValidationCliCommand.cs" region="ValidationCliCommand" language="cs" />
    ///     <code source="../TestApp/Commands/GetCompletionsCliCommand.cs" region="GetCompletionsCliCommand" language="cs" />
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CliCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the command that will be used on the command line to specify the command.
        /// This will be displayed in usage help of the command line application.
        /// <para>
        /// If not set (or is empty/whitespace), the name of the class that this attribute is applied to, will be used to generate command name automatically:
        /// These suffixes will be stripped from the class name: <c>RootCliCommand, RootCommand, SubCliCommand, SubCommand, CliCommand, Command, Cli</c>.
        /// Then the name will be converted to kebab-case, for example:
        /// <list type="bullet">
        ///     <item>If class name is <c>Build</c> or <c>BuildCommand</c> or <c>BuildRootCliCommand</c> -> command name will be <c>build</c></item>
        ///     <item>If class name is <c>BuildText</c> or <c>BuildTextCommand</c> or <c>BuildTextSubCliCommand</c> -> command name will be <c>build-text</c></item>
        /// </list>
        /// </para>
        /// <para>Default convention can be changed via command's <see cref="NameCasingConvention"/> property.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the command. This will be displayed in usage help of the command line application.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command is hidden.
        /// <para>
        /// You might want to support a command, option, or argument, but avoid making it easy to discover.
        /// For example, it might be a deprecated or administrative or preview feature.
        /// Use the <see cref="Hidden"/> property to prevent users from discovering such features by using tab completion or help.
        /// </para>
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the order of the command.
        /// <para>The order is used when printing the symbols in help and for arguments additionally effects the parsing order.</para>
        /// <para>When not set (or is <c>0</c> - the default value), the symbol order is determined based on source code ordering.</para>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the set of an alternative string that can be used on the command line to specify the command.
        /// When set, this will override the auto-generated short form alias.
        /// <para>If you want to set multiple aliases, you can use <see cref="Aliases"/>.</para>
        /// <para>The aliases will be also displayed in usage help of the command line application.</para>
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the set of alternative strings that can be used on the command line to specify the command.
        /// <para>If you want to set a single alias, you can use <see cref="Alias"/>.</para>
        /// <para>The aliases will be also displayed in usage help of the command line application.</para>
        /// </summary>
        public string[] Aliases { get; set; }
        
        /// <summary>
        /// Gets or sets the parent of the command. This property is used when you prefer to use a non-nested class for a subcommand,
        /// i.e. when you want to separate root command and subcommands into different classes/files.
        /// If the class that this attribute is applied to, is already a nested class, then this property will be ignored.
        /// </summary>
        public Type Parent { get; set; }

        /// <summary>
        /// Gets or sets the children of the command. This property is used when you prefer to use a non-nested classes for subcommands,
        /// i.e. when you want to separate root command and subcommands into different classes/files.
        /// If a class in the list, is already a nested class, then that class will be ignored.
        /// </summary>
        public Type[] Children { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether unmatched tokens should be treated as errors. For example,
        /// if set to <see langword="true" /> and an extra command or argument is provided, validation will fail.
        /// <para>Default is <see langword="true" />.</para>
        /// </summary>
        public bool TreatUnmatchedTokensAsErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets a value which indicates whether names are automatically generated for commands, directives, options and arguments.
        /// <para>Names are converted according to <see cref="NameCasingConvention"/>.</para>
        /// <para>
        /// For options, names typically have a leading delimiter (e.g. <c>--option</c>, <c>-option</c> or <c>/option</c>).
        /// Default delimiter (e.g. <c>--option</c>) is changed via <see cref="NamePrefixConvention"/>.
        /// </para>
        /// <para>
        /// This setting will be inherited by subcommands.
        /// This setting can be overriden by a subcommand in the inheritance chain.
        /// </para>
        /// <para>Default is <see cref="CliNameAutoGenerate.All"/>.</para>
        /// </summary>
        public CliNameAutoGenerate NameAutoGenerate { get; set; } = CliNameAutoGenerate.All;

        /// <summary>
        /// Gets or sets the character casing convention to use for automatically generated names of commands, directives, options and arguments.
        /// <para>
        /// This setting will be inherited by subcommands.
        /// This setting can be overriden by a subcommand in the inheritance chain.
        /// </para>
        /// <para>Default is <see cref="CliNameCasingConvention.KebabCase"/> (e.g. <c>kebab-case</c>).</para>
        /// </summary>
        public CliNameCasingConvention NameCasingConvention { get; set; } = CliNameCasingConvention.KebabCase;

        /// <summary>
        /// Gets or sets the prefix convention to use for automatically generated names of options.
        /// <para>
        /// For options, names typically have a leading delimiter (e.g. <c>--option</c>, <c>-option</c> or <c>/option</c>).
        /// </para>
        /// <para>
        /// This setting will be inherited by subcommands.
        /// This setting can be overriden by a subcommand in the inheritance chain.
        /// </para>
        /// <para>Default is <see cref="CliNamePrefixConvention.DoubleHyphen"/> (e.g. <c>--option</c>).</para>
        /// </summary>
        public CliNamePrefixConvention NamePrefixConvention { get; set; } = CliNamePrefixConvention.DoubleHyphen;

        /// <summary>
        /// Gets or sets a value which indicates whether short form aliases are automatically generated names of commands and options.
        /// <para>
        /// First letters of every word in the name will be used to create short form to reduce conflicts.
        /// These first letters are converted according to <see cref="NameCasingConvention"/>.
        /// </para>
        /// <para>
        /// For options, short forms typically have a leading delimiter (e.g. <c>-o</c> or <c>--o</c> or <c>/o</c>).
        /// Default delimiter (e.g. <c>-o</c>) is changed via <see cref="ShortFormPrefixConvention"/>.
        /// </para>
        /// <para>
        /// This setting will be inherited by subcommands.
        /// This setting can be overriden by a subcommand in the inheritance chain.
        /// </para>
        /// <para>Default is <see cref="CliNameAutoGenerate.All"/>.</para>
        /// </summary>
        public CliNameAutoGenerate ShortFormAutoGenerate { get; set; } = CliNameAutoGenerate.All;

        /// <summary>
        /// Gets or sets the prefix convention to use for automatically generated short form aliases of options.
        /// <para>
        /// For options, short forms typically have a leading delimiter (e.g. <c>-o</c> or <c>--o</c> or <c>/o</c>).
        /// </para>
        /// <para>
        /// This setting will be inherited by subcommands.
        /// This setting can be overriden by a subcommand in the inheritance chain.
        /// </para>
        /// <para>Default is <see cref="CliNamePrefixConvention.SingleHyphen"/> (e.g. <c>-o</c>).</para>
        /// </summary>
        public CliNamePrefixConvention ShortFormPrefixConvention { get; set; } = CliNamePrefixConvention.SingleHyphen;
        
        internal static CliCommandAttribute Default { get; } = new();
    }
}
