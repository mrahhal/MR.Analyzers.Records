using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MR.Analyzers.Records;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConvertClassToRecordAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
		DiagnosticDescriptors.MRR1000_ConvertClassToRecord);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
	}

	private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
	{
		var classNode = (ClassDeclarationSyntax)context.Node;

		// Report if the class has only auto properties.
		// Change the message if there's a property that can be mutated.

		var propertyDeclarations = classNode.Members
			.Where(m => m.IsKind(SyntaxKind.PropertyDeclaration))
			.ToList();

		if (propertyDeclarations.Count != classNode.Members.Count)
		{
			// There are other members beside properties.
			return;
		}

		context.ReportDiagnostic(Diagnostic.Create(
			DiagnosticDescriptors.MRR1000_ConvertClassToRecord,
			classNode.Identifier.GetLocation()));
	}
}
