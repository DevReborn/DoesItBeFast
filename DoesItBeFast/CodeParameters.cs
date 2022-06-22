using Mono.Cecil;

namespace DoesItBeFast
{
	public class CodeParameters
	{
		public IList<ModuleDefinition>? IncludedAssemblies { get; init; }
		public int Iterations { get; set; }
		public int WarmupIterations { get; set; }
		public EntryMethod EntryMethod { get; }

		public CodeParameters(EntryMethod entryMethod)
		{
			EntryMethod = entryMethod;
		}
	}
}
