using System;
using System.Collections.Generic;
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

            ParentSymbol = (Parent != null)
                ? Parent.Symbol //Nested class for sub-command
                : ParentArgument; //External class for sub-command
            
            Analyze(symbol);

            if (HasProblem)
                return;


            // If type implements/extends IEnumerable<T>
            HasAddCompletionsInterface = Symbol.AllInterfaces.Any(i => i.ToReferenceString() == "DotMake.CommandLine.ICliAddCompletions");

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

                    //Property with a type that refers to a parent command
                    if (!addedPropertyNames.Contains(property.Name))
                    {
                        //Calculating parent tree is hard (across projects) and expensive in generator
                        //so we only check if property type has CliCommand attribute
                        var hasAttribute = property.Type.GetAttributes()
                            .Any(a => a.AttributeClass.ToCompareString() == AttributeFullName);

                        if (hasAttribute)
                        {
                            parentCommandAccessors.Add(new CliParentCommandAccessorInput(property, null, SemanticModel));
                            addedPropertyNames.Add(property.Name);
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

        public INamedTypeSymbol ParentSymbol { get; }

       
        public AttributeArguments AttributeArguments { get; }

        public INamedTypeSymbol ParentArgument { get; }

        public INamedTypeSymbol[] ChildrenArgument { get; }

        public CliNameCasingConvention? NameCasingConvention { get; }

        public CliNamePrefixConvention? NamePrefixConvention { get; }

        public CliNamePrefixConvention? ShortFormPrefixConvention { get; }

        public bool? ShortFormAutoGenerate { get; }


        public CliCommandHandlerInput Handler { get; }

        public bool HasAddCompletionsInterface { get; }

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

                //Calculating parent tree is hard (across projects) and expensive in generator so we will do some simple checks
                //Full circular dependency can be detected at runtime
                if (ParentArgument != null && Parent == null) //only check for external (non-nested) commands
                {
                    var hasAttribute = ParentArgument.GetAttributes()
                        .Any(a => a.AttributeClass.ToCompareString() == AttributeFullName);

                    if (!hasAttribute)
                        AddDiagnostic(DiagnosticDescriptors.ErrorParentClassHasNotAttribute, DiagnosticName, nameof(CliCommandAttribute));

                    if (ParentArgument.Equals(Symbol, SymbolEqualityComparer.Default))
                        AddDiagnostic(DiagnosticDescriptors.ErrorClassCircularDependency, ParentArgument.Name);
                }

                if (ChildrenArgument != null)
                {
                    foreach (var child in ChildrenArgument)
                    {
                        var hasAttribute = child.GetAttributes()
                            .Any(a => a.AttributeClass.ToCompareString() == AttributeFullName);

                        if (!hasAttribute)
                            AddDiagnostic(DiagnosticDescriptors.ErrorChildClassHasNotAttribute, DiagnosticName, nameof(CliCommandAttribute));

                        if (child.Equals(Symbol, SymbolEqualityComparer.Default))
                            AddDiagnostic(DiagnosticDescriptors.ErrorClassCircularDependency, child.Name);

                        if (ParentArgument != null && Parent == null //only check for external (non-nested) commands
                            && child.Equals(ParentArgument, SymbolEqualityComparer.Default))
                            AddDiagnostic(DiagnosticDescriptors.ErrorClassCircularDependency, child.Name);
                    }
                }
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

        public bool Equals(CliCommandInput other)
        {
            return base.Equals(other);
        }
    }
}
