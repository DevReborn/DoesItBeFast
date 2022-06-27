using DoesItBeFast.Attributes;

namespace TestLibrary
{
	public class TestClass_Funcs
	{
		public List<int> LocalFunction(string text)
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

		public int AssignFuncToVariable(string text)
		{
			Func<string, string, int> func = (x, y) => x.Length + y.Length; 
			return func.Invoke(text, "Hello");
		}

		private static bool ShouldReturnZero(char x)
		{
			if (x == 'h')
				return true;
			return false;
		}
	}

}