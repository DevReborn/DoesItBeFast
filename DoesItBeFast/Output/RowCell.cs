namespace DoesItBeFast.Output
{
	public class RowCell
	{
		public RowCell(object text) 
			: this(text, null)
		{
		}
		public RowCell(object text, RowCell? header) 
			: this(text, header, text is int || text is TimeSpan)
		{
		}
		public RowCell(object text, RowCell? header, bool alignRight)
		{
			if (text == null) throw new ArgumentNullException(nameof(text));
			Text = text.ToString() ?? throw new ArgumentNullException(nameof(text.ToString));
			Header = header;
			AlignRight = alignRight;
		}

		public string Text { get; }
		public RowCell? Header { get; }
		public bool AlignRight { get; }
	}
}
