
using DoesItBeFast;
using DoesItBeFast.Output;

await Runner.RunAsync(new RunnerArguments
{
	TargetAssemblyPath = "TestLibrary.dll",
	Iterations = 100,
	WarmupIterations = 20,
	EntryPointOptions = new EntryPointOptions
	{
		Parameters = new object[] { null }
	},
	CallGraphOptions = new CallGraphOutputterOptions
	{
		MergeMultiMethodCalls = true
	}
});
