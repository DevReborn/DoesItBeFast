using DoesItBeFast.Attributes;

namespace TestLibrary
{
	public class TestClass_Exceptions
	{
		public string ThrowsException(string text)
		{
			if (text == null || text.Length == 0)
			{
				if (DateTime.Now.Ticks % 2 == 0)
					throw new Exception("Text cannot be null or empty");
				else throw new Exception("Different text");
			}
			return "Done!";
		}
		public string InnerMethod_ThrowsException(string text)
		{
			if (text == null || text.Length == 0)
			{
				if (DateTime.Now.Ticks % 2 == 0)
					DateIsEven(text);
				else
					DateIsOdd(text);
			}
			return "Done!";
		}


		public string MoreInnerMethod_ThrowsException(string text)
		{
			if (text == null || text.Length == 0)
			{
				if (DateTime.Now.Ticks % 2 == 0)
					ThisIsAnInnerMethod(text);
				else
					DateIsOdd(text);
			}
			return "Done!";
		}

		[IsThisFast]
		public string Throws_AndCatches_Exception(string text)
		{
			try
			{
				throw new NotImplementedException();
			} 
			catch (Exception e)
			{
				return new string(e.Message
					.Select(x => (char)x.GetHashCode())
					.ToArray());
			}
		}

		private void ThisIsAnInnerMethod(string? text)
		{
			if (text != null)
				return;
			DateIsEven(text);
			DateIsOdd(text);
		}

		private void DateIsOdd(string? text)
		{
			throw new NotImplementedException();
		}

		private void DateIsEven(string? text)
		{
			throw new NotImplementedException();
		}
	}
}