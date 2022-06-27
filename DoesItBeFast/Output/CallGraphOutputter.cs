using DoesItBeFast.Interpretation;
using DoesItBeFast.Output.Common;
using DoesItBeFast.Output.Core;
using Mono.Cecil;

namespace DoesItBeFast.Output
{
	public class CallGraphOutputter : IResultOutputter
	{
		private readonly CallGraphOutputterOptions _options;

		public CallGraphOutputter(CallGraphOutputterOptions options)
		{
			_options = options;
		}

		public async Task<bool> OutputAsync(ResultIntepretation intepretation, TextWriter writer)
		{
			var distinctInterpretations = intepretation.Iterations
				.GroupBy(x => x).ToList();

			if (distinctInterpretations.Count > 1)
			{
				// TODO: link to github for info
				await writer.WriteLineAsync("2 distinct call graphs were detected.");
			}

			for (int i = 0; i < distinctInterpretations.Count; i++)
			{
				var iterations = distinctInterpretations[i];
				var table = CreateCallGraphTable(iterations.ToList());
				await table.WriteAsync(writer);
				if (i < distinctInterpretations.Count - 1)
					await writer.WriteLineAsync();
			}

			return true;
		}

		private Table CreateCallGraphTable(IReadOnlyList<CallGraph> iterations)
		{
			var timings = new List<CallGraphTiming>();
			GetTiming(timings, iterations, 0);
			RecursivelyGetCallGraphTimings(timings, iterations, 1);

			var table = new Table("Call Graph");
			table.AddHeader("Method", "Mean", "Total", "Calls");
			foreach (var timing in timings)
			{
				string methodName = $"`-> {timing.Method.Name}";
				table.Add(new Row(table)
				{
					new RowCell(methodName.PadLeft(methodName.Length + (timing.Depth * 3), ' ')),
					new RowCell(timing.Average),
					new RowCell(timing.Total),
					new RowCell(timing.Count),
				});
			}

			return table;
		}

		private void RecursivelyGetCallGraphTimings(List<CallGraphTiming> timings, 
			IReadOnlyList<CallGraph> iterations, int depth)
		{
			var visitedGraphs = new HashSet<CallGraph>();

			int innerCallCount = iterations[0].Count;
			for (int i = 0; i < innerCallCount; i++)
			{
				if (_options.MergeMultiMethodCalls)
				{
					var innerIterationsMerged = iterations
						.SelectMany(x => x.Where(y => y.Equals(x[i])))
						.ToList();
					if (innerIterationsMerged.Any(visitedGraphs.Contains))
						continue;

					foreach(var iter in innerIterationsMerged)
					{
						visitedGraphs.Add(iter);
					}

					GetTiming(timings, innerIterationsMerged, depth);
					RecursivelyGetCallGraphTimings(timings, innerIterationsMerged, depth + 1);
				}
				else
				{
					var innerIterations = iterations.Select(x => x[i]).ToList();
					GetTiming(timings, innerIterations, depth);
					RecursivelyGetCallGraphTimings(timings, innerIterations, depth + 1);
				}
			}
		}

		private static void GetTiming(List<CallGraphTiming> timings, 
			IReadOnlyList<CallGraph> calls, int depth)
		{
			var innerGraphTiming = new CallGraphTiming
			{
				Method = calls[0].Method,
				Average = calls.AverageTime(),
				Total = calls.TotalPerIteration(),
				Depth = depth,
				Count = calls.Count
			};
			timings.Add(innerGraphTiming);
		}
	}

	public class CallGraphOutputterOptions
	{
		public bool MergeMultiMethodCalls { get; set; } = true;
	}

	public class CallGraphTiming
	{
		public MethodReference Method { get; set; }
		public TimeSpan Average { get; set; }
		public int Depth { get; set; }
		public int Count { get; set; }
		public TimeSpan Total { get; set; }
	}
}
