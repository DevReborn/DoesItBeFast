using DoesItBeFast.Interpretation;
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

		public async Task OutputAsync(ResultIntepretation intepretation, TextWriter writer)
		{
			var distinctInterpretations = intepretation.Iterations
				.Select(x => x.GetHashCode()).Distinct().Count();

			if (distinctInterpretations != 1)
				throw new Exception();

			var timings = new List<CallGraphTiming>();
			GetTiming(timings, intepretation.Iterations, 0);
			await RecursivelyGetCallGraphTimings(timings, intepretation.Iterations, 1);

			var table = new Table("Call Graph");
			table.AddHeader("Method", "Mean", "Depth");
			foreach(var timing in timings)
			{
				string methodName = $"`-> {timing.Method.Name} ";
				table.Add(new Row(table)
				{
					new RowCell(methodName.PadLeft(methodName.Length + (timing.Depth * 3), ' ')),
					new RowCell(timing.Average),
					new RowCell(timing.Depth),
				});
			}
			await table.WriteAsync(writer);
		}

		private static async Task RecursivelyGetCallGraphTimings(List<CallGraphTiming> timings, 
			IReadOnlyList<CallGraph> calls, int depth)
		{
			for (int i = 0; i < calls[0].Count; i++)
			{
				var innerMethod = calls.Select(x => x[i]).ToList();
				GetTiming(timings, innerMethod, depth);
				await RecursivelyGetCallGraphTimings(timings, innerMethod, depth + 1);
			}
		}

		private static void GetTiming(List<CallGraphTiming> timings, 
			IReadOnlyList<CallGraph> calls, int depth)
		{
			var innerGraphTiming = new CallGraphTiming
			{
				Method = calls[0].Method,
				Average = calls.AverageTime(),
				Depth = depth
			};
			timings.Add(innerGraphTiming);
		}
	}

	public class CallGraphOutputterOptions
	{
	}

	public class CallGraphTiming
	{
		public MethodReference Method { get; set; }
		public TimeSpan Average { get; set; }
		public int Depth { get; set; }
	}
}
