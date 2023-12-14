﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration
{
	public class CliOptionInfo : CliSymbolInfo, IEquatable<CliOptionInfo>
	{
		public static readonly string AttributeFullName = typeof(DotMakeCliOptionAttribute).FullName;
		public const string AttributeNameProperty = nameof(DotMakeCliOptionAttribute.Name);
		public const string AttributeAliasesProperty = nameof(DotMakeCliOptionAttribute.Aliases);
		public const string AttributeGlobalProperty = nameof(DotMakeCliOptionAttribute.Global);
		public const string AttributeArityProperty = nameof(DotMakeCliOptionAttribute.Arity);
		public const string AttributeAllowedValuesProperty = nameof(DotMakeCliOptionAttribute.AllowedValues);
		public static readonly string[] Suffixes = CliCommandInfo.Suffixes.Select(s => s + "Option").Append("Option").ToArray();
		public const string OptionClassName = "Option";
		public const string OptionClassNamespace = "System.CommandLine";
		public const string DiagnosticName = "CLI option";
		public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
		{
			{ nameof(DotMakeCliOptionAttribute.HelpName), "ArgumentHelpName"},
			{ nameof(DotMakeCliOptionAttribute.Hidden), "IsHidden"},
			{ nameof(DotMakeCliOptionAttribute.Required), "IsRequired"}
		};

		public CliOptionInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
			: base(symbol, syntaxNode, semanticModel)
		{
			Symbol = (IPropertySymbol)symbol;
			Parent = parent;

			Analyze();

			if (HasProblem)
				return;

			AttributeArguments = attributeData.NamedArguments.Where(pair => !pair.Value.IsNull)
				.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);
			
			if (AttributeArguments.TryGetValue(AttributeGlobalProperty, out var globalTypedConstant)
			    && globalTypedConstant.Value != null)
				Global = (bool)globalTypedConstant.Value;
		}

		public CliOptionInfo(GeneratorAttributeSyntaxContext attributeSyntaxContext)
			: this(attributeSyntaxContext.TargetSymbol,
				attributeSyntaxContext.TargetNode,
				attributeSyntaxContext.Attributes[0],
				attributeSyntaxContext.SemanticModel,
				null)
		{
		}


		public new IPropertySymbol Symbol { get; }

		public ImmutableDictionary<string, TypedConstant> AttributeArguments { get; }

		public CliCommandInfo Parent { get; }

		public bool Global { get; }

		private void Analyze()
		{
			if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
			    || Symbol.IsStatic)
				AddDiagnostic(DiagnosticDescriptors.WarningPropertyNotPublicNonStatic, DiagnosticName);
			else
			{
				if (Symbol.GetMethod == null
				    || (Symbol.GetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.GetMethod.DeclaredAccessibility != Accessibility.Internal))
					AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicGetter, DiagnosticName);

				if (Symbol.SetMethod == null
				    || (Symbol.SetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.SetMethod.DeclaredAccessibility != Accessibility.Internal))
					AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicSetter, DiagnosticName);
			}
		}

		public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varDefaultValue)
		{
			var optionName = AttributeArguments.TryGetValue(AttributeNameProperty, out var nameTypedConstant)
									&& !string.IsNullOrWhiteSpace(nameTypedConstant.Value?.ToString())
				? nameTypedConstant.Value.ToString().Trim()
				: Symbol.Name.StripSuffixes(Suffixes).ToCase(Parent.Settings.NameCasingConvention)
					.AddPrefix(Parent.Settings.NamePrefixConvention);

			sb.AppendLine($"// Option for '{Symbol.Name}' property");
			using (sb.BeginBlock($"var {varName} = new {OptionClassNamespace}.{OptionClassName}<{Symbol.Type}>(\"{optionName}\")", ";"))
			{
				foreach (var kvp in AttributeArguments)
				{
					switch (kvp.Key)
					{
						case AttributeNameProperty:
						case AttributeAliasesProperty:
						case AttributeGlobalProperty:
						case AttributeAllowedValuesProperty:
							continue;
						case AttributeArityProperty:
							var arity = kvp.Value.ToCSharpString().Split('.').Last();
							sb.AppendLine($"{kvp.Key} = {CliArgumentInfo.ArgumentClassNamespace}.{CliArgumentInfo.ArgumentArityClassName}.{arity},");
							break;
						default:
							if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
								propertyName = kvp.Key;

							sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
							break;
					}
				}
			}

			if (AttributeArguments.TryGetValue(AttributeAllowedValuesProperty, out var allowedValuesTypedConstant)
			    && !allowedValuesTypedConstant.IsNull)
				sb.AppendLine($"{OptionClassNamespace}.OptionExtensions.FromAmong({varName}, new[] {allowedValuesTypedConstant.ToCSharpString()});");

			sb.AppendLine($"{varName}.SetDefaultValue({varDefaultValue});");

			var shortForm = optionName.RemovePrefix();
			if (Parent.Settings.ShortFormAutoGenerate && shortForm.Length >= 2)
			{
				shortForm = shortForm[0].ToString()
					.AddPrefix(Parent.Settings.ShortFormPrefixConvention);
				sb.AppendLine($"{varName}.AddAlias(\"{shortForm}\");");
			}

			if (AttributeArguments.TryGetValue(AttributeAliasesProperty, out var aliasesTypedConstant)
			    && !aliasesTypedConstant.IsNull)
			{
				foreach (var aliasTypedConstant in aliasesTypedConstant.Values)
				{
					sb.AppendLine($"{varName}.AddAlias({aliasTypedConstant.ToCSharpString()});");
				}
			}
		}

		public bool Equals(CliOptionInfo other)
		{
			return base.Equals(other);
		}
	}
}