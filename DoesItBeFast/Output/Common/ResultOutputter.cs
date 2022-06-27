using DoesItBeFast.Interpretation;
using DoesItBeFast.Output.Core;

namespace DoesItBeFast.Output.Common
{
	public class ResultOutputter : IResultOutputter
	{
		private readonly IResultOutputter[] _outputters;

		public ResultOutputter(IEnumerable<IResultOutputter> outputters)
		{
			_outputters = outputters.ToArray();
		}
		public async Task<bool> OutputAsync(ResultIntepretation intepretation, TextWriter writer)
		{
			for (int i = 0; i < _outputters.Length; i++)
			{
				var outputter = _outputters[i];
				var producedOuput = await outputter.OutputAsync(intepretation, writer);
				if (i + 1 < _outputters.Length && producedOuput)
				{
					await writer.WriteLineAsync();
					await writer.WriteLineAsync();
				}
			}
			return true;
		}
	}
}
