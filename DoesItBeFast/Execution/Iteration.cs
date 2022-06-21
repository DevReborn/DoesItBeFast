namespace DoesItBeFast.Execution
{
	public class Iteration
	{
		public Iteration(List<long> hashes, List<DateTime> times)
		{
			Hashes = hashes;
			Times = times;
		}

		public List<long> Hashes { get; }
		public List<DateTime> Times { get; }
	}
}
