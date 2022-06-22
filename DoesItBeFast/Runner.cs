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
