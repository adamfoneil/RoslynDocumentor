using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynDocumentor
{
	public static class Engine
	{
		public static async Task GenerateMarkdownDocAsync(this MSBuildWorkspace workspace, string solutionPath, IProgress<ProjectLoadProgress> progress)
		{
			var solution = await workspace.OpenSolutionAsync(solutionPath, progress);

			string basePath = Path.Combine(Path.GetDirectoryName(solutionPath), "Doc.md");
			if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

			foreach (var project in solution.Projects)
			{
				foreach (var doc in project.Documents)
				{
					var tree = await doc.GetSyntaxTreeAsync();

					// thanks to https://www.filipekberg.se/2011/10/21/getting-all-methods-from-a-code-file-with-roslyn/
					// https://github.com/dotnet/roslyn/issues/22629 for line number help

					var methods = tree.GetRoot()
						.DescendantNodes()
						.OfType<MethodDeclarationSyntax>()
						.Where(method => method.Modifiers.Any(mod => mod.Value.Equals("public"))).ToList();

					if (methods.Any())
					{
						string outputPath = Path.Combine(basePath, project.Name);
						if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

						//Console.WriteLine(doc.Name);
						foreach (var method in methods)
						{
							Console.WriteLine($"- {method.Identifier}, starts on line {method.GetLocation().GetMappedLineSpan().StartLinePosition.Line + 1}");
							var parameters = method.ParameterList.ChildNodes().OfType<ParameterSyntax>();
							Console.WriteLine($"    {string.Join(", ", parameters.Select(p => $"{p.Type} {p.Identifier}"))}");
							//Console.WriteLine("  " + string.Join(", ", method.TypeParameterList)
						}
					}
				}
			}
		}
	}
}