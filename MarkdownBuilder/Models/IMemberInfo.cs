namespace RoslynDocumentor.Models
{
	public interface IMemberInfo
	{
		string Name { get; }
		string TypeName { get; set; }
		bool IsStatic { get; set; }
		string Description { get; set; }
		string SourceFile { get; set; }
		int LineNumber { get; set; }
	}
}