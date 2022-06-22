namespace TestLibrary
{
	public class TestClass_With3Layers
	{
		public string DoAThing()
		{
			var text = "Some Text";
			var list = new List<int>();
			foreach(var c in text)
			{
				ConvertCharacter(list, c);
			}
			return ConvertBackToString(list);
		}

		private string ConvertBackToString(List<int> list)
		{
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