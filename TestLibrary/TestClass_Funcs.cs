using DoesItBeFast.Attributes;

namespace TestLibrary
{
	public class TestClass_Funcs
	{
		[IsThisFast]
		public List<int> DoAThing(string text)
		{
			static List<int> PrivateFunc(string text)
			{
				return text.Select(x =>
				{
					if (ShouldReturnZero(x))
						return 0;
					return 10;
				}).ToList();
			}
			return PrivateFunc(text.Length < 3 ? "Hey" : text);
		}

		private static bool ShouldReturnZero(char x)
		{
			if (x == 'h')
				return true;
			return false;
		}
	}

}