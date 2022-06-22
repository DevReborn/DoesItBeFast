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

}