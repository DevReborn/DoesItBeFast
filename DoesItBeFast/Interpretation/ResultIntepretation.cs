namespace DoesItBeFast.Interpretation
{
	public class ResultIntepretation
	{
		public ResultIntepretation(IReadOnlyList<CallGraph> iterations)
		{
			Iterations = iterations;
		}

		public IReadOnlyList<CallGraph> Iterations { get; }
	}
}
