using System.Globalization;

namespace Gleanio.Core.Columns
{
    public class IntColumn : BaseColumn<int?>
    {
        #region Constructors

        public IntColumn(string columnName = null)
            : base(columnName)
        {
        }

        #endregion Constructors

        #region Methods

        public override int? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            int? result = null;

            int temp;
            if (!string.IsNullOrWhiteSpace(parsedValue) &&
                int.TryParse(
                    parsedValue.Trim().Replace(Constants.SingleSpace, string.Empty).Replace("\"", string.Empty),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out temp))
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