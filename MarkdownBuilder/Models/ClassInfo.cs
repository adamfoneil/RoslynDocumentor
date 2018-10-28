using System.Collections.Generic;

namespace RoslynDocumentor.Models
{
	public class ClassInfo
	{
		public string Name { get; set; }
		public bool IsStatic { get; set; }
		/// <summary>
		/// XML summary comments
		/// </summary>
		public string Description { get; set; }
		public Location Location { get; set; }
		public IEnumerable<MethodInfo> Methods { get; set; }
		public IEnumerable<PropertyInfo> Properties { get; set; }
	}
}
