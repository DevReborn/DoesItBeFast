using DoesItBeFast.Interpretation;
using DoesItBeFast.Output.Core;
using Mono.Cecil;

namespace DoesItBeFast.Output
{
	public class GeneralTimingsOutputter : IResultOutputter
	{
		public async Task OutputAsync(ResultIntepretation intepretation, TextWriter writer)
		{
			var generalTimings = GetGeneralTimings(intepretation);

			var table = new Table("General Statistics");
			table.AddHeader("Method", "Mean", "Iterations", "Percentage per call", "Percentage per iteration");
			foreach (var values in generalTimings)
			{
				var row = new Row(table)
				{
					new RowCell(values.Method.Name),
					new RowCell(values.Average),
					new RowCell(values.Count),
					new RowCell(values.Percentage.ToString("P2"), table.Header?[3], true),
					new RowCell($"{values.PerIterationPercentage:P2} ({values.PerIterationCount})" , table.Header?[4], true)
				};
				table.Add(row);
			}

			await table.WriteAsync(writer);
		}

		private static IList<GeneralTiming> GetGeneralTimings(ResultIntepretation intepretation)
		{
			var allCalls = new List<CallGraph>();
			GetCallsRecursively(intepretation.Iterations, allCalls);

			var totalAverage = (long)intepretation.Iterations.Average(x => x.TimeTaken.Ticks);

			return allCalls.GroupBy(x => x.Method).Select(x =>
			{
				var allMethodCalls = x.ToList();
				var perCallAverage = allMethodCalls.AverageTime();
				var groupedByIteration = allMethodCalls.GroupBy(x => x.Entry, ReferenceEqualityComparer.Instance);
				var sumOfEachIteration = groupedByIteration.Select(x => x.Sum(y => y.TimeTaken.Ticks));
				var perIterationAverage = sumOfEachIteration.Average();

				return new GeneralTiming
				{
					Method = x.Key,
					Average = perCallAverage,
					Count = allMethodCalls.Count,
					Percentage = (double)perCallAverage.Ticks / totalAverage,
					PerIterationPercentage = perIterationAverage / totalAverage,
					PerIterationCount = groupedByIteration.Select(x => x.Count()).Distinct().Single()
				};
			}).ToList();
		}

		private static void GetCallsRecursively(IEnumerable<CallGraph> graph, List<CallGraph> allCalls)
		{
			foreach (var innerCall in graph)
			{
				allCalls.Add(innerCall);
				GetCallsRecursively(innerCall, allCalls);
			}
		}
	}

	public class GeneralTiming
	{
		public MethodReference Method { get; set; }
		public TimeSpan Average { get; set; }
		public int Count { get; set; }
		public double Percentage { get; set; }
		public double PerIterationPercentage { get; set; }
		public int PerIterationCount { get; set; }
	}
}
