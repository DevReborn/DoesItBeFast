using DoesItBeFast.Monitoring;
using System.Reflection;

namespace DoesItBeFast.Execution
{
	public class CodeRunner
	{
		private CodeParameters _codeParams;

		public CodeRunner(CodeParameters codeParams)
		{
			_codeParams = codeParams;
		}

		public RunResult Run(MonitoredCode monitored)
		{
			var method = _codeParams.EntryMethod.Method;
			var parameters = _codeParams.EntryMethod.Parameters;

			var assembly = monitored.TargetAssembly;
			var type = assembly.ExportedTypes.First(x => method.DeclaringType.IsEqual(x));
			var methodType = type.GetMethods().First(x => method.IsEqual(x));

			var monitorType = assembly.GetType("DontUseThisNamespaceOtherwiseBadThingsHappenStupid.Monitor");
			var hashes = new List<long>(8096);
			var times = new List<DateTime>(8096);
			monitorType.GetField("_hash").SetValue(null, hashes);
			monitorType.GetField("_time").SetValue(null, times);

			long hashCode = method.GetGenericHashCode();
			monitored.MonitoredMethods.Add(hashCode, method);

			var iterations = new List<Iteration>();

			for (int i = 0; i < _codeParams.Iterations + _codeParams.WarmupIterations; i++)
			{
				var callingObject = CreateCaller(methodType);

				hashes.Add(hashCode);
				times.Add(DateTime.Now);
				methodType.Invoke(callingObject, parameters);
				times.Add(DateTime.Now);
				hashes.Add(-hashCode);

				if(i >= _codeParams.WarmupIterations)
					iterations.Add(new Iteration(hashes.ToList(), times.ToList()));

				hashes.Clear();
				times.Clear();
			}

			return new RunResult(iterations);
		}

		private object? CreateCaller(MethodInfo methodType)
		{
			if (methodType.IsStatic)
				return null;
			return methodType.DeclaringType.GetConstructors()[0].Invoke(new object[0]);
		}
	}
}
