using DoesItBeFast.Attributes;
using DoesItBeFast.Execution;
using DoesItBeFast.Interpretation;
using DoesItBeFast.Monitoring;
using DoesItBeFast.Output;
using DoesItBeFast.Output.Core;
using Mono.Cecil;
using System.Reflection;

namespace DoesItBeFast
{
	public class Orchestrator
	{
		public async Task RunAsync(RunnerArguments runnerArgs)
		{
			var mainAssembly = AssemblyDefinition.ReadAssembly(runnerArgs.TargetAssemblyPath);
			var entryMethod = mainAssembly.Modules
				.SelectMany(x => x.Types)
				.SelectMany(x => x.Methods)
				.Single(x =>
				{
					return x.CustomAttributes.Any(x => x.AttributeType.IsEqual(typeof(IsThisFastAttribute)));
				});

			var codeParams = new CodeParameters(new EntryMethod(entryMethod, runnerArgs.EntryPointOptions.Parameters))
			{
				IncludedAssemblies = mainAssembly.Modules,
				Iterations = runnerArgs.Iterations,
				WarmupIterations = runnerArgs.WarmupIterations
			};
			var monitoredCode = new CodeLoader(codeParams).Load();
			var runResult = new CodeRunner(codeParams).Run(monitoredCode);
			var interpreted = new ResultInterpreter(codeParams, monitoredCode).Interpret(runResult);
			await new ResultOutputter(new List<IResultOutputter>
			{
				new CallGraphOutputter(runnerArgs.CallGraphOptions),
				new GeneralTimingsOutputter()
			}).OutputAsync(interpreted, Console.Out);
		}
	}
}
