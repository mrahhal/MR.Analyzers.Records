using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MR.Analyzers.Records;

[ExportCodeFixProvider(LanguageNames.CSharp)]
[Shared]
public class ConvertClassToRecordCodeFixProvider : CodeFixProvider
{
	private const string Title = "Convert class to record";

	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
		DiagnosticDescriptors.MRR1000_ConvertClassToRecord.Id);

	public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root == null) return;

		foreach (var diagnostic in context.Diagnostics)
		{
			var span = diagnostic.Location.SourceSpan;
			var classNode = (ClassDeclarationSyntax)root.FindToken(span.Start).Parent!;

			var title = Title;
			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					ct => ConvertClassToRecordAsync(context.Document, root, classNode, ct),
					equivalenceKey: title),
				context.Diagnostics);
		}
	}

	private Task<Document> ConvertClassToRecordAsync(Document document, SyntaxNode root, ClassDeclarationSyntax classNode, CancellationToken cancellationToken)
	{
		var parameterListO = classNode.Members.Cast<PropertyDeclarationSyntax>().Select(m =>
		{
			return SyntaxFactory.Parameter(m.AttributeLists, m.Modifiers, m.Type, m.Identifier, null);
		}).ToArray();
		var parameterList = SyntaxFactory.ParameterList().AddParameters(parameterListO);

		var recordNode = SyntaxFactory.RecordDeclaration(
			SyntaxKind.RecordKeyword,
			classNode.AttributeLists, classNode.Modifiers,
			classNode.Keyword, classNode.Identifier,
			classNode.TypeParameterList, parameterList,
			classNode.BaseList, classNode.ConstraintClauses,
			 new SyntaxList<MemberDeclarationSyntax>());

		var newRoot = root.ReplaceNode(classNode, recordNode);
		var newDocument = document.WithSyntaxRoot(newRoot);

		return Task.FromResult(newDocument);
	}
}
