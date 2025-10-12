using System;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public static class DiagnosticDescriptors
    {
        private static readonly Type Type = typeof(DiagnosticDescriptors);

        public static readonly DiagnosticDescriptor ErrorUnsupportedLanguage =
            Create(DiagnosticSeverity.Error, 10, null, Resources.UnsupportedLanguage);
        public static readonly DiagnosticDescriptor ErrorUnsupportedLanguageVersion =
            Create(DiagnosticSeverity.Error, 11, null, Resources.UnsupportedLanguageVersion);


        public static readonly DiagnosticDescriptor WarningClassNotPublicNonStatic =
            Create(DiagnosticSeverity.Warning, 20, null, Resources.ClassNotPublicNonStatic);

        public static readonly DiagnosticDescriptor ErrorClassNotNonAbstractNonGeneric =
            Create(DiagnosticSeverity.Error, 21, null, Resources.ClassNotNonAbstractNonGeneric);

        public static readonly DiagnosticDescriptor ErrorClassHasNotPublicDefaultConstructor =
            Create(DiagnosticSeverity.Error, 22, null, Resources.ClassHasNotPublicDefaultConstructor);

        public static readonly DiagnosticDescriptor ErrorParentClassHasNotAttribute =
            Create(DiagnosticSeverity.Error, 23, null, Resources.ParentClassHasNotAttribute);

        public static readonly DiagnosticDescriptor WarningClassHasNotHandler =
            Create(DiagnosticSeverity.Warning, 24, null, Resources.ClassHasNotHandler);

        public static readonly DiagnosticDescriptor ErrorClassCircularDependency =
            Create(DiagnosticSeverity.Error, 25, null, Resources.ClassCircularDependency);

        public static readonly DiagnosticDescriptor ErrorChildClassHasNotAttribute =
            Create(DiagnosticSeverity.Error, 26, null, Resources.ChildClassHasNotAttribute);


        public static readonly DiagnosticDescriptor WarningPropertyNotPublicNonStatic =
            Create(DiagnosticSeverity.Warning, 30, null, Resources.PropertyNotPublicNonStatic);

        public static readonly DiagnosticDescriptor ErrorPropertyHasNotPublicGetter =
            Create(DiagnosticSeverity.Error, 31, null, Resources.PropertyHasNotPublicGetter);

        public static readonly DiagnosticDescriptor ErrorPropertyHasNotPublicSetter =
            Create(DiagnosticSeverity.Error, 32, null, Resources.PropertyHasNotPublicSetter);

        public static readonly DiagnosticDescriptor WarningPropertyTypeIsNotBindable =
            Create(DiagnosticSeverity.Warning, 33, null, Resources.PropertyTypeIsNotBindable);

        public static readonly DiagnosticDescriptor WarningPropertyTypeEnumerableIsNotBindable =
            Create(DiagnosticSeverity.Warning, 34, null, Resources.PropertyTypeEnumerableIsNotBindable);


        public static readonly DiagnosticDescriptor WarningMethodNotPublicNonStatic =
            Create(DiagnosticSeverity.Warning, 40, null, Resources.MethodNotPublicNonStatic);

        public static readonly DiagnosticDescriptor ErrorMethodNotNonGeneric =
            Create(DiagnosticSeverity.Error, 41, null, Resources.MethodNotNonGeneric);

        public static readonly DiagnosticDescriptor ErrorDelegateNotCorrect =
            Create(DiagnosticSeverity.Error, 42, null, Resources.DelegateNotCorrect);


        public static DiagnosticDescriptor Create(DiagnosticSeverity severity, int code, string title, string messageFormat)
        {
            var id = $"DMCLI{code:D2}";

            return new DiagnosticDescriptor(
                id,
                title ?? severity.ToString(),
                $"{Type.Namespace} -> {messageFormat}",
                "DotMake.CommandLine",
                severity,
                isEnabledByDefault: true
                //helpLinkUri: $"https://dotmake.build/command-line/error#{id}"
            );
        }

        public static DiagnosticDescriptor Create(Exception exception)
        {
            return Create(DiagnosticSeverity.Error, 1, "Unknown Error", exception.ToString());
        }
    }
}
