namespace TestLibrary
{
	public class TestClass
	{
		public static string Method(string arg, string arg2)
		{
			string v = PrivateMethod1(arg);
			string v1 = PrivateMethod2(arg2);
			return v + " " + v1;
		}

		private static string PrivateMethod2(string arg)
		{
			return arg;
		}
		private static string PrivateMethod1(string arg)
		{
			return arg;
		}
	}

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