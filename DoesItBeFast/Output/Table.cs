namespace DoesItBeFast.Output
{
	public class Table : List<Row>
	{
		public Table(string title)
		{
			Title = title;
		}

		public Row? Header { get; set; }
		public string Title { get; }

		public async Task WriteAsync(TextWriter writer)
		{
			Validate();
			var columnLengths = CalculateColumnLengths();

			await WriteRowline(columnLengths, writer);
			await WriteTitleLine(columnLengths, writer);
			if (Header != null)
			{
				await WriteRowline(columnLengths, writer);
				await WriteRow(Header, columnLengths, writer);
			}
			await WriteRowline(columnLengths, writer);
			foreach (var row in this)
			{
				await WriteRow(row, columnLengths, writer);
			}
			await WriteRowline(columnLengths, writer);
		}

		private void Validate()
		{
			if(Header != null && !this.All(x => x.Count == Header.Count))
			{
				throw new Exception();
			} 
			else if (this.Select(x => x.Count).Distinct().Count() != 1) 
			{ 
				throw new Exception();
			}
			else if (Count == 0)
			{
				throw new Exception();
			}
		}

		public void AddHeader(params string[] headers)
		{
			Header = new Row(this);
			Header.AddRange(headers.Select(x => new RowCell(x)));
		}

		private async Task WriteTitleLine(List<int> columnLengths, TextWriter writer)
		{
			int totalWidth = columnLengths.Sum(x => x + 3);
			await writer.WriteLineAsync($"|--- {Title} ".PadRight(totalWidth, ' ') + "|");
		}

		private static async Task WriteRowline(List<int> columnLengths, TextWriter writer)
		{
			int totalWidth = columnLengths.Sum(x => x + 3);
			await writer.WriteLineAsync("|".PadRight(totalWidth, '-') + "|");
		}

		private List<int> CalculateColumnLengths()
		{
			var headerRow = Header ?? this[0];

			var columnLengths = new List<int>();
			for (int i = 0; i < headerRow.Count; i++)
			{
				var headerCell = headerRow[i];
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

		private static async Task WriteRow(Row row, List<int> columnLengths, TextWriter writer)
		{
			for (int i = 0; i < row.Count; i++)
			{
				var length = columnLengths[i] + 2 + 1;
				var rowCell = row[i];
				var cellText = rowCell.Text;
				if(rowCell.AlignRight) await writer.WriteAsync("| " + cellText.PadLeft(length - 3, ' ') + " ");
				else await writer.WriteAsync($"| {cellText}".PadRight(length, ' '));
			}
			await writer.WriteLineAsync('|');
		}
	}
}
