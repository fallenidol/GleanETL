namespace Glean.Core.Columns
{
    using System.Globalization;

    public class IntColumn : BaseColumn<int?>
    {
        public IntColumn(string columnName = null)
            : base(columnName)
        {
        }

        public override int? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            int? result = null;

            int temp;
            if (!string.IsNullOrWhiteSpace(parsedValue) && int.TryParse(parsedValue.Trim(), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                OnParseError(value, typeof(int));
            }

            return result;
        }
    }
}