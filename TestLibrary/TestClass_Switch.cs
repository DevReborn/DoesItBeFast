using DoesItBeFast.Attributes;

namespace TestLibrary
{
	public class TestClass_Switch
	{
		//[IsThisFast]
		public string DoAThing(string text)
		{
			switch (text)
			{
				case "sgsdg":
					return "fsdgsy";
				case "sdgsd":
					return "dfhryd";
				case "fhdrydj":
					return "fsdgfhdfdrsy";
				case "sdsdgsdg":
					return "fsdgfhdfdrsy";
				case "sdsg":
					return "3n46";
				case "shrtn6n46":
					return "n346";
				case "3n4634n6":
					return "63";
				case "n346n346m3":
					return "4n63";
				default:
					return "sdgyy";
			}
		}

		private string ConvertBackToString(List<int> list)
		{
			AddCapatalisedVersion(list, 'h');
			return string.Join("", list.Select(x => char.ConvertFromUtf32(x)).ToArray());
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