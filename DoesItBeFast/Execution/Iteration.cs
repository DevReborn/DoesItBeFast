namespace DoesItBeFast.Execution
{
	public class Iteration
	{
		public Iteration(List<long> hashes, List<DateTime> times, Exception? exception)
		{
			Hashes = hashes;
			Times = times;
			Exception = exception;
		}

		public List<long> Hashes { get; }
		public List<DateTime> Times { get; }
		public Exception? Exception { get; }
	}
}
