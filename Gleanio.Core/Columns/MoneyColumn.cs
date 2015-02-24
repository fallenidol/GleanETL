namespace Gleanio.Core.Columns
{
    using System;
    using System.Globalization;
    using System.Text;

    public class MoneyColumn : BaseColumn<decimal?>
    {
        #region Constructors

        public MoneyColumn(string columnName = null)
            : base(columnName)
        {
        }

        #endregion Constructors

        #region Methods

        public override decimal? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            decimal? result = null;

            var hex = ToHex(parsedValue.Trim())
                .Replace("24", string.Empty) // $
                .Replace("a5", string.Empty) // ¥
                .Replace("a3", string.Empty) // £
                .Replace("20ac", string.Empty) // €
                ;
            var parsedValueWithoutCurrencySymbol = FromHex(hex);

            decimal temp;
            if (!string.IsNullOrWhiteSpace(parsedValueWithoutCurrencySymbol) &&
                decimal.TryParse(parsedValueWithoutCurrencySymbol.Trim(), NumberStyles.Currency, CultureInfo.InvariantCulture, out temp))
            {
                result = temp;
            }
            else
            {
                OnParseError(value, typeof(decimal));
            }

            return result;
        }

        private static string FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return Encoding.ASCII.GetString(raw); ;
        }

        private static string ToHex(string input)
        {
            char[] values = input.ToCharArray();
            string hexOutput = null;
            foreach (char letter in values)
            {
                int value = Convert.ToInt32(letter);
                hexOutput += String.Format("{0:X}", value);
            }
            return hexOutput.ToLowerInvariant();
        }

        #endregion Methods
    }
}