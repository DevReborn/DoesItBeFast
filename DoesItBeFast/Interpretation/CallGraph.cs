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
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; internal set; }

		public TimeSpan TimeTaken => EndTime - StartTime;
		public CallGraph Entry => Parent?.Entry ?? this;
	}
}
