namespace DoesItBeFast.Interpretation
{
	public static class CallGraphExtensions
	{
		public static TimeSpan AverageTime(this IEnumerable<CallGraph> callGraphs)
		{
			return TimeSpan.FromTicks((long)callGraphs.Average(x => x.TimeTaken.Ticks));
		}
	}
}
