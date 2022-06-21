using DoesItBeFast.Execution;
using DoesItBeFast.Interpretation;
using DoesItBeFast.Monitoring;
using DoesItBeFast.Output;
using Mono.Cecil;

namespace DoesItBeFast
{
	public class Orchestrator
	{
		public async Task RunAsync(RunnerArguments runnerArgs)
		{
			var mainAssembly = AssemblyDefinition.ReadAssembly("TestLibrary.dll");
			var entryMethod = mainAssembly.Modules.SelectMany(x => x.Types)
				.Where(x => x.Name == "TestClass2")
				.SelectMany(x => x.Methods)
				.First(x => x.Name == "Join");

			var codeParams = new CodeParameters(new EntryMethod(entryMethod, new object[] { new string[] { "Hello", "World", "Everyone" } }))
			{
				IncludedAssemblies = mainAssembly.Modules
			};
			var monitoredCode = new CodeLoader(codeParams).Load();
			var runResult = new CodeRunner(codeParams).Run(monitoredCode);
			var interpreted = new ResultInterpreter(codeParams, monitoredCode).Interpret(runResult);
			new ResultPrinter().Output(interpreted);
		}
	}
}
