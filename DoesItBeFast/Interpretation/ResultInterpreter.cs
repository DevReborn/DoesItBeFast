using DoesItBeFast.Execution;
using DoesItBeFast.Monitoring;
using Mono.Cecil;

namespace DoesItBeFast.Interpretation
{
	public class ResultInterpreter
	{
		private CodeParameters _codeParams;
		private MonitoredCode _monitoredCode;

		public ResultInterpreter(CodeParameters codeParams, MonitoredCode monitoredCode)
		{
			_codeParams = codeParams;
			_monitoredCode = monitoredCode;
		}

		public ResultIntepretation Interpret(RunResult result)
		{
			var iteratedCallGraphs = new List<CallGraph>();

			foreach (var iteration in result.Iterations)
			{
				CallGraph? graph = null;

				if (iteration.Hashes.Count != iteration.Times.Count)
					throw new Exception();

				for (int i = 0; i < iteration.Hashes.Count; i++)
				{
					var hash = iteration.Hashes[i];
					var time = iteration.Times[i];
					if (hash >= 0)
					{
						var innnerGraph = PushCall(graph, hash, time);
						graph = innnerGraph;
					}
					else
					{
						var outerGraph = PopCall(graph, -hash, time);
						if (outerGraph is null)
						{
							if (i != iteration.Hashes.Count - 1)
								throw new Exception();
						}
						else graph = outerGraph;
					}
				}

				iteratedCallGraphs.Add(graph ?? throw new Exception());
			}

			return new ResultIntepretation(iteratedCallGraphs);
		}

		private static CallGraph? PopCall(CallGraph? graph, long hash, DateTime time)
		{
			if (graph == null)
				throw new Exception();
			if (graph.Hash == hash)
			{
				graph.EndTime = time;
				return graph.Parent;
			}
			else throw new Exception();
		}

		private CallGraph PushCall(CallGraph? graph, long hash, DateTime time)
		{
			var methodInfo = FindMethodInfo(hash);
			if (graph == null)
			{
				return new CallGraph(methodInfo, hash, null)
				{
					StartTime = time
				};
			}
			else
			{
				var nextCall = new CallGraph(methodInfo, hash, graph)
				{
					StartTime = time
				};
				graph.Add(nextCall);
				return nextCall;
			}
		}

		private MethodReference FindMethodInfo(long hash)
		{
			if (_monitoredCode.MonitoredMethods.TryGetValue(hash, out var method))
				return method;
			throw new Exception("Method cannot be found");
		}
	}
}
