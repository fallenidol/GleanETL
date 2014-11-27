namespace Gleanio.Framework.Columns
{
    using System.Globalization;

    public class DecimalColumn : BaseColumn<decimal?>
    {
        #region Constructors

        public DecimalColumn(string columnName)
            : base(columnName)
        {
        }

        #endregion Constructors

        #region Methods

        public override decimal? ParseValue(string value)
        {
            string parsedValue = PreParseValue(value);

            decimal? result = null;

            decimal temp;
            if (!string.IsNullOrWhiteSpace(parsedValue) && decimal.TryParse(parsedValue.Trim().Replace(Constants.SingleSpace, string.Empty).Replace("\"", string.Empty), NumberStyles.Any, CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                OnParseError(value);
            }

            return result;
        }

        #endregion Methods
    }
}