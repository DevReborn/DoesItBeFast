using Mono.Cecil;

namespace DoesItBeFast
{
	public class CodeParameters
	{
		public IList<ModuleDefinition>? IncludedAssemblies { get; init; }
		public EntryMethod EntryMethod { get; }

		public CodeParameters(EntryMethod entryMethod)
		{
			EntryMethod = entryMethod;
		}
	}
}
