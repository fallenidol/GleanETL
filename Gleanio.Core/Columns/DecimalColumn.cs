namespace Gleanio.Core.Columns
{
    using System.Globalization;

    public class DecimalColumn : BaseColumn<decimal?>
    {
        #region Constructors

        public DecimalColumn(string columnName = null)
            : base(columnName)
        {
        }

        #endregion Constructors

        #region Methods

        public override decimal? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            decimal? result = null;

            decimal temp;
            if (!string.IsNullOrWhiteSpace(parsedValue) &&
                decimal.TryParse(
                    parsedValue.Trim().Replace(Constants.SingleSpace, string.Empty),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                OnParseError(value, typeof(decimal));
            }

            return result;
        }

        #endregion Methods
    }
}