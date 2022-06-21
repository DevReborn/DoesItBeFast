using Mono.Cecil;

namespace DoesItBeFast
{
	public class EntryMethod
	{
		public MethodDefinition Method { get; }
		public object[] Parameters { get; }

		public EntryMethod(MethodDefinition method, object[] parameters)
		{
			Method = method;
			Parameters = parameters;
		}
	}
}
