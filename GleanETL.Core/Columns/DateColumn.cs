namespace Glean.Core.Columns
{
    using System;
    using System.Globalization;

    using Glean.Core.Enumerations;

    public class DateColumn : BaseColumn<DateTime?>
    {
        private const string DefaultOutputFormat = "yyyy-MM-dd";

        private readonly string[] inputFormats;

        private readonly DateTime? invalidDateValue;

        public DateColumn(string columnName = null, string[] inputFormats = null, string outputFormat = DefaultOutputFormat, DateTime? invalidDateValue = null)
            : base(columnName)
        {
            this.invalidDateValue = invalidDateValue;
            this.inputFormats = inputFormats ?? GetStandardDateFormats();
            this.OutputFormat = outputFormat;
        }

        public string OutputFormat { get; }

        public static string[] GetStandardDateFormats(StandardDateFormats format = StandardDateFormats.Default)
        {
            string[] formats = null;

            switch (format)
            {
                case StandardDateFormats.Default:
                case StandardDateFormats.Australia:
                case StandardDateFormats.UnitedKingdom:
                    formats = new[]
                        { "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "ddMMyyyy", "dd/M/yy", "d/MM/yy", "d/M/yy", "yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd" };
                    break;
                case StandardDateFormats.UnitedStates:
                    formats = new[]
                        { "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MMddyyyy", "M/dd/yy", "MM/d/yy", "M/d/yy", "yyyy-MM-dd", "yyyy-M-d", "yyyy-MM-d", "yyyy-M-dd" };
                    break;
            }
            return formats;
        }

        //[DebuggerHidden]
        public static DateTime? ParseValue(string value, string[] validFormats, DateTime? invalidDateValue = null)
        {
            var result = invalidDateValue;

            if (value != null)
            {
                var trimmedValue = value.TrimAndRemoveConsecutiveWhiteSpace();

                if (trimmedValue.Length > 0)
                {
                    DateTime temp;
                    if (DateTime.TryParseExact(trimmedValue, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out temp))
                    {
                        result = temp;
                    }
                    else
                    {
                        throw new ParseException(value, typeof(DateTime));
                    }
                }
            }

            return result;
        }

        public override DateTime? ParseValue(string value)
        {
            try
            {
                var parsedValue = this.PreParseValue(value);

                return ParseValue(parsedValue, this.inputFormats);
            }
            catch (ParseException pe)
            {
                this.OnParseError(pe.ValueBeingParsed, typeof(DateTime), pe.Message);

                return this.invalidDateValue;
            }
        }

        public string ParseValueAndFormat(string value)
        {
            var dt = this.ParseValue(value);
            string stringValue = null;

            if (dt.HasValue)
            {
                stringValue = dt.Value.ToString(this.OutputFormat);
            }

            return stringValue;
        }
    }
}