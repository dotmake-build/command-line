# Commands

A *command* in command-line input is a token that specifies an action or defines a group of related actions. For example:

* In `dotnet run`, `run` is a command that specifies an action.
* In `dotnet tool install`, `install` is a command that specifies an action, and `tool` is a command that specifies a group of related commands. There are other tool-related commands, such as `tool uninstall`, `tool list`, and `tool update`.

## Root commands

The *root command* is the one that specifies the name of the app's executable. For example, the `dotnet` command specifies the *dotnet.exe* executable.

## Subcommands

Most command-line apps support *subcommands*, also known as *verbs*. For example, the `dotnet` command has a `run` subcommand that you invoke by entering `dotnet run`.

Subcommands can have their own subcommands. In `dotnet tool install`, `install` is a subcommand of `tool`.

### Command Hierarchy

Defining sub-commands in DotMake.Commandline is very easy. We simply use nested classes to create a hierarchy.
Just make sure you apply `[CliCommand]` attribute to the nested classes as well:
```c#
/*
    Command hierarchy in below example is:
        
     TestApp
     └╴level-1
       └╴level-2
*/

[CliCommand(Description = "A root cli command with nested children")]
public class RootWithNestedChildrenCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; } = "DefaultForArgument1";

    public void Run(CliContext context)
    {
        if (!context.Result.HasArgs)
            context.ShowHierarchy();
        else
            context.ShowValues();
    }

    [CliCommand(Description = "A nested level 1 sub-command")]
    public class Level1SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run(CliContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Description = "A nested level 2 sub-command")]
        public class Level2SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            public void Run(CliContext context)
            {
                context.ShowValues();
            }
        }
    }
}
```

Another way to create hierarchy between commands, especially if you want to use standalone classes,  
is to;

### Use `Children` property of `[CliCommand]` attribute to specify array of `typeof` child classes:
```c#
/*
    Command hierarchy in below example is:

     TestApp
     └╴external-level-1
       └╴external-level-2
*/

[CliCommand(
    Description = "A root cli command with external children",
    Children = new []
    {
        typeof(ExternalLevel1SubCliCommand)
    }
)]
public class RootWithExternalChildrenCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; } = "DefaultForArgument1";

    public void Run(CliContext context)
    {
        if (!context.Result.HasArgs)
            context.ShowHierarchy();
        else
            context.ShowValues();
    }
}

[CliCommand(
    Description = "An external level 1 sub-command",
    Children = new[]
    {
        typeof(ExternalLevel2SubCliCommand)
    }
)]
public class ExternalLevel1SubCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}

[CliCommand(Description = "An external level 2 sub-command")]
public class ExternalLevel2SubCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}
```

### Or use `Parent` property of `[CliCommand]` attribute to specify `typeof` parent class:
```c#
/*
    Command hierarchy in below example is:

     TestApp
     └╴external-level-1-with-parent
       └╴external-level-2-with-parent
*/

[CliCommand(
    Description = "A root cli command with external children"
)]
public class RootAsExternalParentCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; } = "DefaultForArgument1";

    public void Run(CliContext context)
    {
        if (!context.Result.HasArgs)
            context.ShowHierarchy();
        else
            context.ShowValues();
    }
}

[CliCommand(
    Description = "An external level 1 sub-command",
    Parent = typeof(RootAsExternalParentCliCommand)
)]
public class ExternalLevel1WithParentSubCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}

[CliCommand(
    Description = "An external level 2 sub-command",
    Parent = typeof(ExternalLevel1WithParentSubCliCommand)
)]
public class ExternalLevel2WithParentSubCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}
```

The class that `[CliCommand]` attribute is applied to,
- will be a root command if the class is not a nested class and other's `Children` property and self's `Parent` property is not set.
- will be a sub command if the class is a nested class or other's `Children` property or self's `Parent` property is set.

You can create a complex hierarchy like this by mixing nested classes and external classes:
```
 TestApp
 ├╴external-level-1-with-nested
 │ └╴level-2
 └╴level_1
   └╴external_level_2_with_nested
     └╴level_3
```
`Parent` property can even refer to a nested class in another class, `Children` property can not because having a nested parent
is higher priority. A nested child can use `Children` property to refer non-nested classes though.

### Accessing parent commands

Sub-commands can get a reference to the parent command by adding a property of the parent command type.  
Alternatively `CliContext.Result.Bind<TDefinition>` method can be called to manually get reference to a parent command.  
Note that binding will be done only once per definition class, so calling this method consecutively
for the same definition class will return the cached result.

```c#
// Sub-commands can get a reference to the parent command by adding a property of the parent command type.

[CliCommand(Description = "A root cli command with children that can access parent commands")]
public class ParentCommandAccessorCliCommand
{
    [CliOption(
        Description = "This is a global option (Recursive option on the root command), it can appear anywhere on the command line",
        Recursive = true)]
    public string GlobalOption1 { get; set; } = "DefaultForGlobalOption1";

    [CliArgument(Description = "Description for RootArgument1")]
    public string RootArgument1 { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }

    [CliCommand(Description = "A nested level 1 sub-command which accesses the root command")]
    public class Level1SubCliCommand
    {
        [CliOption(
            Description = "This is global for all sub commands (it can appear anywhere after the level-1 verb)",
            Recursive = true)]
        public string Level1RecursiveOption1 { get; set; } = "DefaultForLevel1RecusiveOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        // The parent command gets automatically injected
        public ParentCommandAccessorCliCommand RootCommand { get; set; }

        public void Run(CliContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Description = "A nested level 2 sub-command which accesses its parent commands")]
        public class Level2SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            // All ancestor commands gets injected
            public ParentCommandAccessorCliCommand RootCommand { get; set; }
            public Level1SubCliCommand ParentCommand { get; set; }

            public void Run(CliContext context)
            {
                context.ShowValues();

                Console.WriteLine();
                Console.WriteLine(@$"Level1RecursiveOption1 = {ParentCommand.Level1RecursiveOption1}");
                Console.WriteLine(@$"parent Argument1 = {ParentCommand.Argument1}");
                Console.WriteLine(@$"GlobalOption1 = {RootCommand.GlobalOption1}");
                Console.WriteLine(@$"RootArgument1 = {RootCommand.RootArgument1}");
            }
        }
    }
}
```

Command accessor properties can also be safely used for child commands and not only parent commands.
Circular dependency errors will be prevented, for example when parent and child has command accessors that point to each other.

```c#
[CliCommand(Description = "A root cli command")]
public class RootCliCommand
{
    //This will be non-null only when the called command was this sub-command
    //For example you can check sub-command accessors for null to determine
    //which one was called
    public SubCliCommand SubCliCommandAccessor { get; set; }

    [CliCommand(Description = "A sub-command")]
    public class SubCliCommand
    {
        //This will be always non-null because if sub-command was called,
        //its parent-command should also have been called
        public RootCliCommand RootCliCommandAccessor { get; set; }
    }
}
```

## Command Inheritance

When you have repeating/common options and arguments for your commands, you can define them once in a base class and then 
share them by inheriting that base class in other command classes. Interfaces are also supported !

```c#
[CliCommand]
public class InheritanceCliCommand : CredentialCommandBase, IDepartmentCommand
{
    public string Department { get; set; } = "Accounting";
}

public abstract class CredentialCommandBase
{
    [CliOption(Description = "Username of the identity performing the command")]
    public string Username { get; set; } = "admin";

    [CliOption(Description = "Password of the identity performing the command")]
    public string Password { get; set; }

    public void Run()
    {
        Console.WriteLine($"I am {Username}");
    }
}

public interface IDepartmentCommand
{
    [CliOption(Description = "Department of the identity performing the command (interface)")]
    string Department { get; set; }
}
```

The property attribute and the property initializer from the most derived class in the hierarchy will be used 
(they will override the base ones). The command handler (Run or RunAsync) will be also inherited.
So in the above example, `InheritanceCliCommand` inherits options `Username`, `Password` from a base class and
option `Department` from an interface. Note that the property initializer for `Department` is in the derived class, 
so that default value will be used.

So you can use interfaces to group your options and inherit them in your cli commands. 
`c#` allows inheriting only one base class but it allows inheriting multiple interfaces:

```c#
[CliCommand]
public interface IOptionsGroup1
{
    [CliOption(Description = "Username of the identity performing the command")]
    public string Username { get; set; }

    [CliOption(Description = "Password of the identity performing the command")]
    public string Password { get; set; }
}

public interface IOptionsGroup2
{
    [CliOption(Description = "Department of the identity performing the command (interface)")]
    string Department { get; set; }
}

public class MyCliCommand : IOptionsGroup1, IOptionsGroup2
{
    public string Username { get; set; } = "admin";

    public string Password { get; set; }

    public string Department { get; set; } = "Accounting";
}

public class My2CliCommand : IOptionsGroup1
{
    public string Username { get; set; } = "admin";

    public string Password { get; set; }
}
```

---
The properties for `[CliCommand]` attribute (see [CliCommandAttribute](https://dotmake.build/command-line/api/DotMake.CommandLine.CliCommandAttribute.html) docs for more info):
- Name
- Description
- Hidden
- Order
- Alias
- Aliases
- Parent
- TreatUnmatchedTokensAsErrors
- NameAutoGenerate *(inherited by subcommands, used for child commands, directives, options and arguments)*
- NameCasingConvention *(inherited by subcommands, used for child commands, directives, options and arguments)*
- NamePrefixConvention *(inherited by subcommands, used for child options)*
- ShortFormAutoGenerate *(inherited by subcommands, used for child commands and options)*
- ShortFormPrefixConvention *(inherited by subcommands, used for child options)*
- RequiredGroups
