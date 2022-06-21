namespace DoesItBeFast.Execution
{
	public class RunResult
	{
		public IReadOnlyList<Iteration> Iterations { get; }

		public RunResult(IReadOnlyList<Iteration> iterations)
		{
			Iterations = iterations;
		}

	}
}
