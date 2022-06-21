using DoesItBeFast.Interpretation;
using Mono.Cecil;

namespace DoesItBeFast.Output
{
	public class ResultPrinter
	{
		public void Output(ResultIntepretation intepretation)
		{
			WriteGeneralTableResults(intepretation);
		}

		private static void WriteGeneralTableResults(ResultIntepretation intepretation)
		{
			var generalTimings = GetGeneralTimings(intepretation);

			var table = new Table();
			var header = new Row(table)
			{
				new RowCell("Method"),
				new RowCell("Mean"),
				new RowCell("Iterations"),
				new RowCell("Percentage per call"),
				new RowCell("Percentage per iteration"),
			};
			table.Header = header;

			foreach (var values in generalTimings)
			{
				var row = new Row(table)
				{
					new RowCell(values.Method.Name, header[0]),
					new RowCell(values.Average, header[1]),
					new RowCell(values.Count, header[2]),
					new RowCell(values.Percentage.ToString("P2"), header[3]),
					new RowCell($"{values.PerIterationPercentage:P2} ({values.PerIterationCount})" , header[4])
				};
				table.Add(row);
			}

			table.Write();
		}

		private static IList<GeneralTiming> GetGeneralTimings(ResultIntepretation intepretation)
		{
			var allCalls = new List<CallGraph>();
			GetCallsRecursively(intepretation.Iterations, allCalls);

			var totalAverage = (long)intepretation.Iterations.Average(x => x.TimeTaken.Ticks);

			return allCalls.GroupBy(x => x.Method).Select(x =>
			{
				var allMethodCalls = x.ToList();
				var perCallAverage = TimeSpan.FromTicks((long)allMethodCalls.Average(y => y.TimeTaken.Ticks));
				var groupedByIteration = allMethodCalls.GroupBy(x => x.Entry);
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
		public double PerIterationPercentage { get; internal set; }
		public int PerIterationCount { get; internal set; }
	}
}
