namespace Glean.Core.Columns
{
    using System;
    using System.Globalization;
    using System.Text;

    public class MoneyColumn : BaseColumn<decimal?>
    {
        public MoneyColumn(string columnName = null)
            : base(columnName)
        {
        }

        public override decimal? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            decimal? result = null;

            var hex = ToHex(parsedValue.Trim()).Replace("24", string.Empty) // $
                    .Replace("a5", string.Empty) // ¥
                    .Replace("a3", string.Empty) // £
                    .Replace("20ac", string.Empty) // €
                ;
            var parsedValueWithoutCurrencySymbol = FromHex(hex);

            decimal temp;
            if (!string.IsNullOrWhiteSpace(parsedValueWithoutCurrencySymbol) &&
                decimal.TryParse(parsedValueWithoutCurrencySymbol.Trim(), NumberStyles.Currency,
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

        private static string FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            var raw = new byte[hex.Length / 2];
            for (var i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return Encoding.ASCII.GetString(raw);
        }

        private static string ToHex(string input)
        {
            var values = input.ToCharArray();
            string hexOutput = null;
            foreach (var letter in values)
            {
                var value = Convert.ToInt32(letter);
                hexOutput += string.Format("{0:X}", value);
            }
            return hexOutput.ToLowerInvariant();
        }
    }
}