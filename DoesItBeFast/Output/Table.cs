namespace DoesItBeFast.Output
{
	public class Table : List<Row>
	{
		public Row Header { get; set; }

		public void Write()
		{
			var columnLengths = CalculateColumnLengths();
			WriteRowline(columnLengths);
			WriteRow(Header, columnLengths);
			WriteRowline(columnLengths);
			foreach (var row in this)
			{
				WriteRow(row, columnLengths);
			}
			WriteRowline(columnLengths);
		}

		private static void WriteRowline(List<int> columnLengths)
		{
			int totalWidth = columnLengths.Sum(x => x + 3);
			Console.WriteLine("|".PadRight(totalWidth, '-') + "|");
		}

		private List<int> CalculateColumnLengths()
		{
			var columnLengths = new List<int>();
			for (int i = 0; i < Header.Count; i++)
			{
				var headerCell = Header[i];
				var maxLength = headerCell.Text.Length;
				foreach (var row in this)
				{
					var columnCell = row[i];
					maxLength = Math.Max(maxLength, columnCell.Text.Length);
				}
				columnLengths.Add(maxLength);
			}

			return columnLengths;
		}

		private static void WriteRow(Row row, List<int> columnLengths)
		{
			for (int i = 0; i < row.Count; i++)
			{
				var length = columnLengths[i] + 2 + 1;
				var cell = row[i].Text;
				Console.Write($"| {cell}".PadRight(length, ' '));
			}
			Console.WriteLine('|');
		}
	}
}
