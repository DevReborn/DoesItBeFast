using DoesItBeFast.Attributes;
using DoesItBeFast.Interpretation;
using DoesItBeFast.Output.Core;

namespace DoesItBeFast.Output
{
	public class ExceptionOutputter : IResultOutputter
	{
		public async Task<bool> OutputAsync(ResultIntepretation intepretation, TextWriter writer)
		{
			var distinctErrorIterations = intepretation.Iterations
				.GroupBy(x => HashCode.Combine(x.Exception?.GetType(), x.Exception?.Message))
				.ToList();

			var actualErrors = distinctErrorIterations
				.Select(x => x.First().Exception)
				.ToList();

			if (actualErrors.All(x => x is null))
				return false;

			await writer.WriteLineAsync("|------------------|");
			await writer.WriteLineAsync("|--- Exceptions ---|");
			await writer.WriteLineAsync("|------------------|");
			await writer.WriteLineAsync();

			if (actualErrors.All(x => x is not null))
				// TODO maybe have a 'ExpectedException' Flag on setup
				await writer.WriteLineAsync("Every iteration threw an unhandled exception. (Is this what you expected?!?)");
			else if (actualErrors.Any(x => x is not null))
				await writer.WriteLineAsync("Some (but not all) iterations threw an unhandled exception. :(");

			if(actualErrors.Where(x => x is not null).Count() > 1)
				await writer.WriteLineAsync("Multiple types of exceptions were thrown.");

			if (actualErrors.Count > 1)
				await WriteNonDistinctMessage(writer);

			foreach (var error in distinctErrorIterations)
			{
				var exception = error.First().Exception;
				if (exception == null)
					continue;

				await writer.WriteLineAsync($"{exception.GetType()} was thrown in {error.Count()} iterations");
				await writer.WriteLineAsync($"    - Message: {exception.Message}");
			}

			return true;
		}

		private static async Task WriteNonDistinctMessage(TextWriter writer)
		{
			// TODO: link to github for info
			// TODO maybe have a flag to turn this off if different exception messages are expected
			// ie. a guid is created on each run
			await writer.WriteLineAsync(
@$"Note: Try to test methods that produce the same output for every iteration.
      If iterations with the same input produce different outputs on subsequent iterations it can make the results unreliable.
      A smart person would realise this...
      
      Anyway, If you really need to rely on the environment being setup in a specific way before each iteration, consider using the {nameof(BeforeEachThingAttribute)} and {nameof(AfterEachThingAttribute)} attributes.");
		}
	}
}
