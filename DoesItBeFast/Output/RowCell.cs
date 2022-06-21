namespace DoesItBeFast.Output
{
	public class RowCell
	{
		public RowCell(object text) : this(text, null)
		{
		}
		public RowCell(object text, RowCell? header)
		{
			if (text == null) throw new ArgumentNullException(nameof(text));
			Text = text.ToString() ?? throw new ArgumentNullException(nameof(text.ToString));
			Header = header;
		}

		public string Text { get; }
		public RowCell? Header { get; }
	}
}
