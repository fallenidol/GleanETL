namespace Glean.Core.Columns
{
    public class DerivedStringColumn : DerivedColumn<string>
    {
        public DerivedStringColumn(string columnName = null)
            : base(columnName)
        {
        }

        public int DetectedMaxLength { get; internal set; }
    }
}