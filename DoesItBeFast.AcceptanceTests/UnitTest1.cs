using Xunit;

namespace DoesItBeFast.AcceptanceTests
{
	public class UnitTest1
	{
		[Fact]
		public async Task Test()
		{
			await Runner.RunAsync(new string[0]);
		}
	}
}