﻿
using DoesItBeFast;
using DoesItBeFast.Output;

await Runner.RunAsync(new RunnerArguments
{
	TargetAssemblyPath = "TestLibrary.dll",
	Iterations = 10,
	WarmupIterations = 2,
	EntryPointOptions = new EntryPointOptions
	{
		Parameters = new object[] { "Heres a long piece of text to check the speed of this" }
	},
	CallGraphOptions = new CallGraphOutputterOptions
	{
		MergeMultiMethodCalls = true
	}
});
