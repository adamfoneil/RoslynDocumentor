using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using RoslynDocumentor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynDocumentor
{
	public static class Engine
	{
		public static async Task GetPublicMethods(this MSBuildWorkspace workspace, string solutionPath, IProgress<ProjectLoadProgress> progress)
		{
			var solution = await workspace.OpenSolutionAsync(solutionPath, progress);

			string basePath = Path.Combine(Path.GetDirectoryName(solutionPath), "Doc.md");
			if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);

			foreach (var project in solution.Projects)
			{
				foreach (var doc in project.Documents)
				{
					var tree = await doc.GetSyntaxTreeAsync();

					var classes = GetClassInfo(tree, doc.FilePath);
				}
			}
		}

		public static IEnumerable<ClassInfo> GetClassInfo(SyntaxTree syntaxTree, string sourceFile)
		{
			// thanks to https://www.filipekberg.se/2011/10/21/getting-all-methods-from-a-code-file-with-roslyn/
			// https://github.com/dotnet/roslyn/issues/22629 for line number help

			List<ClassInfo> results = new List<ClassInfo>();

			var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Where(c => IsPublic(c.Modifiers));

			foreach (var c in classes)
			{
				var classInfo = new ClassInfo();
				classInfo.Name = c.Identifier.Text;
				classInfo.IsStatic = IsStatic(c.Modifiers);
				classInfo.Description = ParseDescription(c);

				var methods = c.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => IsPublic(m.Modifiers)).ToList();
				if (methods.Any())
				{

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

			return results;
		}

		private static string ParseDescription(ClassDeclarationSyntax c)
		{
			throw new NotImplementedException();
		}

		private static bool IsStatic(SyntaxTokenList modifiers)
		{
			return HasModifier(modifiers, "static");
		}

		private static bool IsPublic(SyntaxTokenList modifiers)
		{
			return HasModifier(modifiers, "public");
		}

		private static bool HasModifier(SyntaxTokenList modifiers, string modifier)
		{
			return modifiers.Any(m => m.Value.Equals(modifier));
		}
	}
}