using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace CodeAnalysisApp1
{
	class Program
	{
		static async Task Main(string[] args)
		{
			// Attempt to set the version of MSBuild.
			var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
			var instance = visualStudioInstances.Length == 1
				// If there is only one instance of MSBuild on this machine, set that as the one to use.
				? visualStudioInstances[0]
				// Handle selecting the version of MSBuild you want to use.
				: SelectVisualStudioInstance(visualStudioInstances);

			Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

			// NOTE: Be sure to register an instance with the MSBuildLocator 
			//       before calling MSBuildWorkspace.Create()
			//       otherwise, MSBuildWorkspace won't MEF compose.
			MSBuildLocator.RegisterInstance(instance);

			using (var workspace = MSBuildWorkspace.Create())
			{
				// Print message for WorkspaceFailed event to help diagnosing project load failures.
				workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

				var solutionPath = args[0];
				Console.WriteLine($"Loading solution '{solutionPath}'");

				// Attach progress reporter so we print projects as they are loaded.
				var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
				Console.WriteLine($"Finished loading solution '{solutionPath}'");

				foreach (var project in solution.Projects)
				{
					if (project.Name.Equals(args[1]))
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
								Console.WriteLine(doc.Name);
							}

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

			Console.WriteLine("done");
			Console.ReadLine();
		}

		private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
		{
			Console.WriteLine("Multiple installs of MSBuild detected please select one:");
			for (int i = 0; i < visualStudioInstances.Length; i++)
			{
				Console.WriteLine($"Instance {i + 1}");
				Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
				Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
				Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
			}

			while (true)
			{
				var userResponse = Console.ReadLine();
				if (int.TryParse(userResponse, out int instanceNumber) &&
					instanceNumber > 0 &&
					instanceNumber <= visualStudioInstances.Length)
				{
					return visualStudioInstances[instanceNumber - 1];
				}
				Console.WriteLine("Input not accepted, try again.");
			}
		}

		private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
		{
			public void Report(ProjectLoadProgress loadProgress)
			{
				var projectDisplay = Path.GetFileName(loadProgress.FilePath);
				if (loadProgress.TargetFramework != null)
				{
					projectDisplay += $" ({loadProgress.TargetFramework})";
				}

				Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
			}
		}
	}
}
