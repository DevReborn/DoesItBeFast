namespace DoesItBeFast.Output
{
	public class RowCell
	{
		public RowCell(object text) 
			: this(text, null)
		{
		}
		public RowCell(object text, RowCell? header) 
			: this(text, header, text is int)
		{
		}
		public RowCell(object text, RowCell? header, bool alignRight)
		{
			if (text == null) throw new ArgumentNullException(nameof(text));
			Text = Format(text) ?? throw new ArgumentNullException(nameof(text.ToString));
			Header = header;
			AlignRight = alignRight;
		}

		private static string? Format(object text)
		{
			if (text is TimeSpan timeSpan)
			{
				return timeSpan.ToString("%s'.'FFFFFFF's'");
			}
			return text?.ToString();
		}

		public string Text { get; }
		public RowCell? Header { get; }
		public bool AlignRight { get; }
	}
}
