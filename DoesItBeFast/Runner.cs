using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoesItBeFast
{
	public class Runner
	{
		public static Task RunAsync(string[] arguments)
		{
			var runnerArgs = new RunnerArguments();

			return new Orchestrator().RunAsync(runnerArgs);
		}
	}
}
