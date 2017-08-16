namespace Glean.Core.Columns
{
    using System.Globalization;

    public class DecimalColumn : BaseColumn<decimal?>
    {
        public DecimalColumn(string columnName = null)
            : base(columnName)
        {
        }

        public override decimal? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            decimal? result = null;

            decimal temp;
            if (!string.IsNullOrWhiteSpace(parsedValue) &&
                decimal.TryParse(parsedValue.Trim().Replace(Constants.SingleSpace, string.Empty), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                OnParseError(value, typeof(decimal));
            }

            return result;
        }
    }
}