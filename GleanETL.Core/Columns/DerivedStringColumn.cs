namespace GleanETL.Core.Columns
{
	public class DerivedStringColumn : DerivedColumn<string>
	{
		#region Constructors

		public DerivedStringColumn(string columnName = null)
			: base(columnName)
		{
		}

		#endregion Constructors

		#region Properties

		public int DetectedMaxLength
		{
			get; internal set;
		}

		#endregion Properties
	}
}