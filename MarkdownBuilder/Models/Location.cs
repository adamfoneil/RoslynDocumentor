namespace RoslynDocumentor.Models
{
	public struct Location
	{
		/// <summary>
		/// Relative source file path within solution
		/// </summary>
		string SourceFile { get; set; }
		/// <summary>
		/// 1-based line number of location
		/// </summary>
		int LineNumber { get; set; }
	}
}