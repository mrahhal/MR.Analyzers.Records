using Microsoft.CodeAnalysis;

namespace MR.Analyzers.Records;

public static class DiagnosticDescriptors
{
	public static class Categories
	{
		public const string Conversion = nameof(Conversion);
	}

	public static readonly DiagnosticDescriptor MRR1000_ConvertClassToRecord = new(
		"MRR1000",
		"Convert class to record",
		"Convert class to record",
		Categories.Conversion,
		DiagnosticSeverity.Hidden,
		isEnabledByDefault: true);
}
