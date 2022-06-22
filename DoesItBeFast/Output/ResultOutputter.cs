using DoesItBeFast.Interpretation;
using DoesItBeFast.Output.Core;

namespace DoesItBeFast.Output
{
	public class ResultOutputter : IResultOutputter
	{
		private IResultOutputter[] _outputters;

		public ResultOutputter(IEnumerable<IResultOutputter> outputters)
		{
			_outputters = outputters.ToArray();
		}
		public async Task OutputAsync(ResultIntepretation intepretation, TextWriter writer)
		{
			for (int i = 0; i < _outputters.Length; i++)
			{
				var outputter = _outputters[i];
				await outputter.OutputAsync(intepretation, writer);
				if(i + 1 < _outputters.Length)
				{
					await writer.WriteLineAsync();
				}
			}
		}
	}
}
