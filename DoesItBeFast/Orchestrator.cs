using DoesItBeFast.Execution;
using DoesItBeFast.Interpretation;
using DoesItBeFast.Monitoring;
using DoesItBeFast.Output;
using DoesItBeFast.Output.Core;
using Mono.Cecil;

namespace DoesItBeFast
{
	public class Orchestrator
	{
		public async Task RunAsync(RunnerArguments runnerArgs)
		{
			var mainAssembly = AssemblyDefinition.ReadAssembly("TestLibrary.dll");
			var entryMethod = mainAssembly.Modules.SelectMany(x => x.Types)
				.Where(x => x.Name == "TestClass_With3Layers")
				.SelectMany(x => x.Methods)
				.First(x => x.Name == "DoAThing");

			var codeParams = new CodeParameters(new EntryMethod(entryMethod, new object[] {}))
			{
				IncludedAssemblies = mainAssembly.Modules
			};
			var monitoredCode = new CodeLoader(codeParams).Load();
			var runResult = new CodeRunner(codeParams).Run(monitoredCode);
			var interpreted = new ResultInterpreter(codeParams, monitoredCode).Interpret(runResult);
			await new ResultOutputter(new List<IResultOutputter>
			{
				new CallGraphOutputter(new CallGraphOutputterOptions()),
				new GeneralTimingsOutputter()
			}).OutputAsync(interpreted, Console.Out);
		}
	}
}
