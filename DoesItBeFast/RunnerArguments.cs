using DoesItBeFast.Output;

namespace DoesItBeFast
{
	public class RunnerArguments
	{
		public string TargetAssemblyPath { get; set; }
		public int Iterations { get; set; }
		public int WarmupIterations { get; set; }
		public CallGraphOutputterOptions CallGraphOptions { get; set; } = new CallGraphOutputterOptions();
		public EntryPointOptions EntryPointOptions { get; set; } = new EntryPointOptions();
	}
}
