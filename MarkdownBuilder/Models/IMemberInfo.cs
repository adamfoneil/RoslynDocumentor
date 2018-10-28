namespace RoslynDocumentor.Models
{
	public interface IMemberInfo
	{
		string Name { get; }
		string TypeName { get; set; }
		/// <summary>
		/// If the TypeName is defined in this solution, this is its location (otherwise null)
		/// </summary>
		Location TypeLocation { get; set; }
		bool IsStatic { get; set; }
		string Description { get; set; }
		Location Location { get; set; }
	}
}