using DoesItBeFast.Interpretation;

namespace DoesItBeFast.Output.Core
{
	public interface IResultOutputter
	{
		public Task<bool> OutputAsync(ResultIntepretation intepretation, TextWriter writer);
	}
}
