using Mono.Cecil;

namespace DoesItBeFast.Interpretation
{
	public class CallGraph : List<CallGraph>
	{
		public CallGraph(MethodReference method, long hash, CallGraph? parent)
		{
			Method = method;
			Hash = hash;
			Parent = parent;
		}

		public MethodReference Method { get; }
		public long Hash { get; }
		public CallGraph? Parent { get; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public Exception? Exception { get; set; }

		public TimeSpan TimeTaken => EndTime.Value - StartTime.Value;
		public CallGraph Entry => Parent?.Entry ?? this;


		public override bool Equals(object? obj)
		{
			return obj is CallGraph graph 
				&& graph.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 19;
				foreach (var innerGraph in this)
				{
					hash = hash * 31 + innerGraph.GetHashCode();
				}
				return HashCode.Combine(Method, hash);
			}
		}
	}
}
