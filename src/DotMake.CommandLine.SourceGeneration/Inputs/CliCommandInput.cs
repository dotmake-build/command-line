using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliCommandInput : InputBase, IEquatable<CliCommandInput>
    {
        public static readonly string AttributeFullName = typeof(CliCommandAttribute).FullName;
        public const string DiagnosticName = "CLI command";
        public const string GeneratedSubNamespace = "GeneratedCode";
        public const string GeneratedClassSuffix = "Builder";

        public CliCommandInput(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInput parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (INamedTypeSymbol)symbol;
            Parent = parent;

            AttributeArguments = new AttributeArguments(attributeData, semanticModel);
            if (AttributeArguments.TryGetValue(nameof(CliCommandAttribute.Parent), out var parentValue))
                ParentArgument = (INamedTypeSymbol)parentValue;
            if (AttributeArguments.TryGetValues(nameof(CliCommandAttribute.Children), out var childrenValue))
                ChildrenArgument = childrenValue.Cast<INamedTypeSymbol>().ToArray();
            if (AttributeArguments.TryGetValue(nameof(CliCommandAttribute.NameCasingConvention), out var nameCasingValue))
                NameCasingConvention = (CliNameCasingConvention)nameCasingValue;
            if (AttributeArguments.TryGetValue(nameof(CliCommandAttribute.NamePrefixConvention), out var namePrefixValue))
                NamePrefixConvention = (CliNamePrefixConvention)namePrefixValue;
            if (AttributeArguments.TryGetValue(nameof(CliCommandAttribute.ShortFormPrefixConvention), out var shortFormPrefixValue))
                ShortFormPrefixConvention = (CliNamePrefixConvention)shortFormPrefixValue;
            if (AttributeArguments.TryGetValue(nameof(CliCommandAttribute.ShortFormAutoGenerate), out var shortFormAutoGenerateArgumentValue))
                ShortFormAutoGenerate = (bool)shortFormAutoGenerateArgumentValue;

            GeneratedClassName = symbol.Name + GeneratedClassSuffix;
            GeneratedClassNamespace = symbol.GetNamespaceOrEmpty();
            if (!GeneratedClassNamespace.EndsWith(GeneratedSubNamespace))
                GeneratedClassNamespace = SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedSubNamespace);
            GeneratedClassFullName = (symbol.ContainingType != null)
                ? SymbolExtensions.CombineNameParts(
                    symbol.RenameContainingTypesFullName(GeneratedSubNamespace, GeneratedClassSuffix),
                    GeneratedClassName)
                : SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedClassName);
            
            CliReferenceDependantInput = (parent != null)
                ? parent.CliReferenceDependantInput
                : new CliReferenceDependantInput(semanticModel.Compilation);

            Analyze(symbol);

            if (HasProblem)
                return;

            var addedPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            var visitedProperties = new Dictionary<string, ISymbol>(StringComparer.Ordinal);

            //Loop through all own and then inherited members (not distinct)
            foreach (var member in Symbol.GetAllMembers())
            {
                if (member is IPropertySymbol property)
                {
                    if (addedPropertyNames.Contains(property.Name))
                        continue;

                    //Property with [CliOption] or [CliArgument] attribute
                    foreach (var propertyAttributeData in property.GetAttributes())
                    {
                        var attributeFullName = propertyAttributeData.AttributeClass?.ToCompareString();

                        //visitedProperty is used to inherit [CliOption] or [CliArgument] attribute
                        //for example, property does not have an attribute in child class but has one in a parent class.
                        //The property attribute and the property initializer from the most derived class in the hierarchy
                        //will be used (they will override the base ones).
                        //future note: non-existing property initializer in derived may be overriding an existing one in base
                        visitedProperties.TryGetValue(property.Name, out var visitedProperty);

                        //having both attributes doesn't make sense as the binding would override the previous one's value
                        //user should better have separate properties for each attribute
                        //so stop (break) when one of the attributes found first on a property.
                        if (attributeFullName == CliOptionInput.AttributeFullName)
                        {
                            options.Add(new CliOptionInput(visitedProperty ?? property, null, propertyAttributeData, semanticModel, this));
                            addedPropertyNames.Add(property.Name);

                            break; 
                        }
                        if (attributeFullName == CliArgumentInput.AttributeFullName)
                        {
                            arguments.Add(new CliArgumentInput(visitedProperty ?? property, null, propertyAttributeData, semanticModel, this));
                            addedPropertyNames.Add(property.Name);

                            break;
                        }
                    }

                    if (!visitedProperties.ContainsKey(property.Name))
                        visitedProperties.Add(property.Name, property);
                }
                else if (member is IMethodSymbol method)
                {
                    if (CliCommandHandlerInput.HasCorrectName(method))
                    {
                        var possibleHandler = new CliCommandHandlerInput(method, null, semanticModel, this);
                        if (possibleHandler.HasCorrectSignature
                            && (Handler == null || possibleHandler.SignaturePriority > Handler.SignaturePriority))
                            Handler = possibleHandler;
                    }
                }
            }

            //Disable warning for missing handler, instead show help when no handler
            //if (Handler == null)
            //    AddDiagnostic(DiagnosticDescriptors.WarningClassHasNotHandler, false, CliCommandHandlerInfo.DiagnosticName);

            foreach (var nestedType in Symbol.GetTypeMembers())
            {
                foreach (var memberAttributeData in nestedType.GetAttributes())
                {
                    if (memberAttributeData.AttributeClass?.ToCompareString() == AttributeFullName)
                        subcommands.Add(new CliCommandInput(nestedType, null, memberAttributeData, semanticModel, this));
                }
            }
        }

        public static bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is ClassDeclarationSyntax
                   //skip nested classes as they will be handled by the parent classes
                   && !(syntaxNode.Parent is TypeDeclarationSyntax);
        }

        public static CliCommandInput From(GeneratorAttributeSyntaxContext attributeSyntaxContext)
        {
            return new(attributeSyntaxContext.TargetSymbol,
                attributeSyntaxContext.TargetNode,
                attributeSyntaxContext.Attributes[0],
                attributeSyntaxContext.SemanticModel,
                null);
        }

        public new INamedTypeSymbol Symbol { get; }

        public CliCommandInput Parent { get; } //Nested parent

        //Nested and external parents
        //ParentTree will take into account, both ParentArgument and parent's ChildrenArgument
        public List<CliCommandInput> ParentTree { get; private set; } = new();

        public bool IsParentCircular { get; private set; }

        public INamedTypeSymbol NestedOrExternalParentSymbol => (Parent != null)
            ? Parent.Symbol //Nested class for sub-command
            : ParentTree.FirstOrDefault()?.Symbol; //External class for sub-command

        public bool IsRoot => (Parent == null) && (ParentTree.Count == 0);

        public bool IsExternalChild => (Parent == null) && (ParentTree.Count > 0);

        public bool IsGenerated { get; internal set; }

        
        public AttributeArguments AttributeArguments { get; }

        public INamedTypeSymbol ParentArgument { get; }

        public INamedTypeSymbol[] ChildrenArgument { get; }

        public CliNameCasingConvention? NameCasingConvention { get; private set; }

        public CliNamePrefixConvention? NamePrefixConvention { get; private set; }

        public CliNamePrefixConvention? ShortFormPrefixConvention { get; private set; }

        public bool? ShortFormAutoGenerate { get; private set; }


        public CliCommandHandlerInput Handler { get; }

        public string GeneratedClassName { get; }

        public string GeneratedClassNamespace { get; }

        public string GeneratedClassFullName { get; }

        public CliReferenceDependantInput CliReferenceDependantInput { get; }

        public IReadOnlyList<CliOptionInput> Options => options;
        private readonly List<CliOptionInput> options = new();

        public IReadOnlyList<CliArgumentInput> Arguments => arguments;
        private readonly List<CliArgumentInput> arguments = new();

        public IReadOnlyList<CliCommandInput> Subcommands => subcommands; //Only nested subcommands
        private readonly List<CliCommandInput> subcommands = new();

        public IReadOnlyList<CliParentCommandAccessorInput> ParentCommandAccessors => parentCommandAccessors;
        private readonly List<CliParentCommandAccessorInput> parentCommandAccessors = new();

        public sealed override void Analyze(ISymbol symbol)
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningClassNotPublicNonStatic, DiagnosticName);
            else
            {
                if (Symbol.IsAbstract || Symbol.IsGenericType)
                    AddDiagnostic(DiagnosticDescriptors.ErrorClassNotNonAbstractNonGeneric, DiagnosticName);

                if (!CliReferenceDependantInput.HasMsDependencyInjectionAbstractions
                    && !Symbol.InstanceConstructors.Any(c =>
                        c.Parameters.IsEmpty
                        && (c.DeclaredAccessibility == Accessibility.Public || c.DeclaredAccessibility == Accessibility.Internal)
                    ))
                    AddDiagnostic(DiagnosticDescriptors.ErrorClassHasNotPublicDefaultConstructor, DiagnosticName);
            }
        }

        public override IEnumerable<Diagnostic> GetAllDiagnostics()
        {
            return base.GetAllDiagnostics() //self
                .Concat(Handler?.GetAllDiagnostics() ?? Enumerable.Empty<Diagnostic>())
                .Concat(Options.SelectMany(c => c.GetAllDiagnostics()))
                .Concat(Arguments.SelectMany(c => c.GetAllDiagnostics()))
                .Concat(Subcommands.SelectMany(c => c.GetAllDiagnostics()))
                .Concat(ParentCommandAccessors.SelectMany(c => c.GetAllDiagnostics()));
        }

        public void UpdateParentTree(Dictionary<CliCommandInput, CliCommandInput> parentMap)
        {
            //When attribute's Parent property is changed we will already get a new CliCommandInput.
            //But when attribute's Children property is changed in a parent class, we will not get a new CliCommandInput
            //for external child so we need to update parent tree and re-generate if parent tree is different.
            //Parent map includes everything, collected commands and their nested sub-commands.

            //Parent tree starts with immediate parent and ends with top parent, for example:
            //Level3Parent -> Level2Parent -> Level1Parent

            var newParentTree = new List<CliCommandInput>();
            var visited = new HashSet<CliCommandInput>();

            var currentCommand = this;
            while (currentCommand != null)
            {
                visited.Add(currentCommand);
                /*
                CliCommandInput parentCommand;

                if (currentCommand.Parent != null) //if nested, ignore external parent and children and add nested parent
                    parentCommand = currentCommand.Parent;
                else if (!parentMap.TryGetValue(currentCommand, out parentCommand))
                    break;
                */

                if (!parentMap.TryGetValue(currentCommand, out var parentCommand))
                    break;

                if (visited.Contains(parentCommand)) //prevent circular dependency
                {
                    currentCommand.IsParentCircular = true;
                    break;
                }

                newParentTree.Add(parentCommand);

                currentCommand = parentCommand;
            }

            UpdateParentTree(newParentTree);

            //Propagate tree change to nested sub-commands
            foreach (var subcommand in subcommands)
                subcommand.UpdateParentTree(parentMap);
        }

        public void UpdateParentTree(List<CliCommandInput> newParentTree)
        {
            if (!ParentTree.SequenceEqual(newParentTree))
            {
                ParentTree = newParentTree;
                OnParentTreeUpdated();
            }
        }

        private void OnParentTreeUpdated()
        {
            //ParentTree is changed so we will force re-generation by setting IsGenerated to false.
            IsGenerated = false;

            //Update inherited properties
            NameCasingConvention ??= ParentTree.Select(p => p.NameCasingConvention).FirstOrDefault(v => v.HasValue)
                                     ?? CliCommandAttribute.Default.NameCasingConvention;

            NamePrefixConvention ??= ParentTree.Select(p => p.NamePrefixConvention).FirstOrDefault(v => v.HasValue)
                                     ?? CliCommandAttribute.Default.NamePrefixConvention;

            ShortFormPrefixConvention ??= ParentTree.Select(p => p.ShortFormPrefixConvention).FirstOrDefault(v => v.HasValue)
                                          ?? CliCommandAttribute.Default.ShortFormPrefixConvention;

            ShortFormAutoGenerate ??= ParentTree.Select(p => p.ShortFormAutoGenerate).FirstOrDefault(v => v.HasValue)
                                      ?? CliCommandAttribute.Default.ShortFormAutoGenerate;

            //We can be re-generating now, so remove parent related diagnostics and re-add new ones if required
            RemoveDiagnostic(DiagnosticDescriptors.ErrorClassCircularDependency);
            RemoveDiagnostic(DiagnosticDescriptors.ErrorParentClassHasNotAttribute);
            var circularParent = ParentTree.FirstOrDefault(s => s.IsParentCircular);
            if (circularParent != null)
                AddDiagnostic(DiagnosticDescriptors.ErrorClassCircularDependency, circularParent.Symbol.Name);
            else if (ParentArgument != null && ParentTree.Count == 0)
                AddDiagnostic(DiagnosticDescriptors.ErrorParentClassHasNotAttribute, DiagnosticName, nameof(CliCommandAttribute));

            //We can be re-generating now, so remove old parent accessors and re-add them
            parentCommandAccessors.Clear();
            var addedPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            var parentCommandsByType = ParentTree
                .Select(p => p.Symbol)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
            //Loop through all own and then inherited members (not distinct)
            foreach (var member in Symbol.GetAllMembers())
            {
                if (member is IPropertySymbol property)
                {
                    //Property with a type that refers to a parent command
                    if (!addedPropertyNames.Contains(property.Name)
                        && parentCommandsByType.Contains(property.Type))
                    {
                        parentCommandAccessors.Add(new CliParentCommandAccessorInput(property, null, SemanticModel));
                        addedPropertyNames.Add(property.Name);
                    }
                }
            }
        }

        public bool Equals(CliCommandInput other)
        {
            return base.Equals(other);
        }
    }
}
