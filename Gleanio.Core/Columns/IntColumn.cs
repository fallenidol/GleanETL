namespace Gleanio.Core.Columns
{
    using System.Globalization;

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
                    parsedValue.RemoveAllWhitespace(),
                    NumberStyles.Any, CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                OnParseError(value, typeof(int));
            }

            return result;
        }

        #endregion Methods
    }
}