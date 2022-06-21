namespace DoesItBeFast.Output
{
	public class Row : List<RowCell>
	{
		private Table _table;

		public Row(Table table)
		{
			_table = table;
		}
	}
}
