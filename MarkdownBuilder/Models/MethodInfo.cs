using System.Collections.Generic;

namespace RoslynDocumentor.Models
{
	public class MethodInfo : IMemberInfo
	{
		public string Name { get; set; }
		public string TypeName { get; set; }
		public Location TypeLocation { get; set; }
		public bool IsStatic { get; set; }
		public string Description { get; set; }		
		public Location Location { get; set; }
		public IEnumerable<Parameter> Parameters { get; set; }

		public class Parameter
		{
			public string Name { get; set; }
			public string TypeName { get; set; }
			public Location TypeLocation { get; set; }
			public string DefaultValue { get; set; }
		}
	}
}