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
            var parsedValue = this.PreParseValue(value);

            decimal? result = null;

            decimal temp;
            if (!string.IsNullOrWhiteSpace(parsedValue) &&
                decimal.TryParse(parsedValue.Trim().Replace(Constants.SingleSpace, string.Empty), NumberStyles.Any, CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                this.OnParseError(value, typeof(decimal));
            }

            return result;
        }
    }
}