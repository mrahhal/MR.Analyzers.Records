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
		var propertyDeclarations = classNode.Members.Cast<PropertyDeclarationSyntax>();
		var indentationTrivia = propertyDeclarations
			.FirstOrDefault()
			?.DescendantTrivia()
			.First(t => t.IsKind(SyntaxKind.WhitespaceTrivia)) ?? SyntaxFactory.Tab;

		var ConvertPropertyDeclarationToParameter = (PropertyDeclarationSyntax p) =>
			SyntaxFactory.Parameter(
				new SyntaxList<AttributeListSyntax>(
					p.AttributeLists.Select(a => a.WithTrailingTrivia(SyntaxFactory.Space))),
				default,
				p.Type,
				p.Identifier,
				default)
				.WithLeadingTrivia(indentationTrivia);

		var parameters = propertyDeclarations
			.Select(p => ConvertPropertyDeclarationToParameter(p))
			.ToArray();

		var parameterList = SyntaxFactory.ParameterList()
			.AddParameters(parameters)
			.WithOpenParenToken(SyntaxFactory.Token(SyntaxKind.OpenParenToken)
				.WithTrailingTrivia(EndOfLineHelper.EndOfLine));

		var commaTokens = parameterList.ChildTokens().Where(t => t.IsKind(SyntaxKind.CommaToken));
		parameterList = parameterList.ReplaceTokens(commaTokens, (t, _) =>
		{
			return t.WithTrailingTrivia(EndOfLineHelper.EndOfLine);
		});

		var recordNode = SyntaxFactory.RecordDeclaration(
			SyntaxKind.RecordDeclaration,
			classNode.AttributeLists, classNode.Modifiers,
			SyntaxFactory.Token(SyntaxKind.RecordKeyword),
			default,
			classNode.Identifier.WithoutTrivia(),
			classNode.TypeParameterList, parameterList,
			classNode.BaseList, classNode.ConstraintClauses,
			default,
			default,
			default,
			SyntaxFactory.Token(SyntaxKind.SemicolonToken));

		var newRoot = root.ReplaceNode(classNode, recordNode);
		var newDocument = document.WithSyntaxRoot(newRoot);

		return Task.FromResult(newDocument);
	}
}
