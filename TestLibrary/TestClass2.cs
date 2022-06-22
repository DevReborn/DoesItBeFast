namespace TestLibrary
{
	public class TestClass2
	{
		public string Join(string[] args)
		{
			var result = "";
			foreach(var arg in args)
			{
				result = Capitalise(result, arg);
			}
			TestClass testClass = new TestClass();
			return ThenDoAThing(result) + TestClass.Method("and", "stuff");
		}

		private string ThenDoAThing(string result)
		{
			Thread.Sleep(10);
			return result;
		}

		private static string Capitalise(string result, string arg)
		{
			return result + " " + arg.ToUpper();
		}
	}

}