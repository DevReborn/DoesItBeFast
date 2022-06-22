namespace DoesItBeFast
{
	public class Runner
	{
		public static Task RunAsync(string[] arguments)
		{
			var runnerArgs = new RunnerArguments();
			return RunAsync(runnerArgs);
		}

		public static Task RunAsync(RunnerArguments arguments)
		{
			return new Orchestrator().RunAsync(arguments);
		}
	}
}
