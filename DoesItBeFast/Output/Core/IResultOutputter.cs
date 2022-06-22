using DoesItBeFast.Interpretation;

namespace DoesItBeFast.Output.Core
{
	public interface IResultOutputter
	{
		public Task OutputAsync(ResultIntepretation intepretation, TextWriter writer);
	}
}
