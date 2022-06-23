using DoesItBeFast.Attributes;

namespace TestLibrary
{
	public class TestClass_With3Layers
	{
		[IsThisFast]
		public string DoAThing(string text)
		{
			var list = new List<int>();
			foreach(var c in text)
			{
				list.Add(c);
				ConvertCharacter(list, c);
			}
			return text + " add some text";
			//return ConvertBackToString(list);
		}

		private string ConvertBackToString(List<int> list)
		{
			AddCapatalisedVersion(list, 'h');
			return string.Join("", list.Select(x => char.ConvertFromUtf32(x)));
		}

		private void ConvertCharacter(List<int> list, char c)
		{
			list.Add(c);
			AddCapatalisedVersion(list, c);
		}

		private void AddCapatalisedVersion(List<int> list, char c)
		{
			var upper = c.ToString().ToUpper()[0];
			list.Add(upper);
		}
	}

}