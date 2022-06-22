namespace DoesItBeFast.Interpretation
{
	public static class CallGraphExtensions
	{
		public static TimeSpan AverageTime(this IEnumerable<CallGraph> callGraphs)
		{
			return TimeSpan.FromTicks((long)callGraphs.Average(x => x.TimeTaken.Ticks));
		}
		public static TimeSpan TotalPerIteration(this IEnumerable<CallGraph> callGraphs)
		{
			return TimeSpan.FromTicks((long)callGraphs.GroupBy(x => x.Entry, ReferenceEqualityComparer.Instance)
				.Select(x => x.SumTime().Ticks).Average());
		}
		public static TimeSpan SumTime(this IEnumerable<CallGraph> callGraphs)
		{
			return TimeSpan.FromTicks(callGraphs.Sum(x => x.TimeTaken.Ticks));
		}
	}
}
