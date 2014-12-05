using System;
using System.Globalization;
using Gleanio.Core.Enumerations;

namespace Gleanio.Core.Columns
{
    public class DateColumn : BaseColumn<DateTime?>
    {
        #region Constructors

        public DateColumn(string columnName, string[] inputFormats = null, string outputFormat = _defaultOutputFormat,
            DateTime? invalidDateValue = null)
            : base(columnName)
        {
            _invalidDateValue = invalidDateValue;
            _inputFormats = inputFormats ?? GetStandardDateFormats();
            _outputFormat = outputFormat;
        }

        #endregion Constructors

        #region Properties

        public string OutputFormat
        {
            get { return _outputFormat; }
        }

        #endregion Properties

        #region Fields

        private const string _defaultOutputFormat = "yyyy-MM-dd";

        private readonly string[] _inputFormats;
        private readonly DateTime? _invalidDateValue;
        private readonly string _outputFormat;

        #endregion Fields

        #region Methods

        public static string[] GetStandardDateFormats(StandardDateFormats format = StandardDateFormats.Default)
        {
            string[] formats = null;

            switch (format)
            {
                case StandardDateFormats.Default:
                case StandardDateFormats.Australia:
                case StandardDateFormats.UnitedKingdom:
                    formats = new[]
                    {
                        "M/yyyy", "MM/yyyy", "/M/yyyy", "/MM/yyyy", "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy",
                        "ddMMyyyy", "dd/M/yy", "d/MM/yy", "d/M/yy", "/M/yy", "/MM/yy", "M/yy", "MM/yy", "/yy", "yy",
                        "yyyy"
                    };
                    break;
                case StandardDateFormats.UnitedStates:
                    formats = new[]
                    {
                        "M/yyyy", "MM/yyyy", "M/yyyy", "MM/yyyy", "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy",
                        "MMddyyyy", "M/dd/yy", "MM/d/yy", "M/d/yy", "M/yy", "MM/yy", "M/yy", "MM/yy", "/yy", "yy",
                        "yyyy"
                    };
                    break;
            }
            return formats;
        }

        public override DateTime? ParseValue(string value)
        {
            var parsedValue = PreParseValue(value);

            DateTime? result = null;

            if (value != null)
            {
                var trimmedValue = parsedValue.TrimAndRemoveConsecutiveWhitespace();

                if (trimmedValue.Length > 0)
                {
                    trimmedValue = trimmedValue
                        .Replace("/20/", "/02/")
                        .Replace("/30/", "/03/")
                        .Replace("/40/", "/04/")
                        .Replace("/50/", "/05/")
                        .Replace("/60/", "/06/")
                        .Replace("/70/", "/07/")
                        .Replace("/80/", "/08/")
                        .Replace("/90/", "/09/")
                        .Replace("30/2/", "01/03/")
                        .Replace("31/4/", "30/04/")
                        .Replace("31/6/", "01/07/")
                        .Replace("31/9/", "01/10/")
                        .Replace("31/11/", "01/12/")
                        .Replace("29/2/2013", "01/03/2013")
                        .Replace("29/2/2014", "01/03/2014")
                        .Replace("29/2/2015", "01/03/2015")
                        .Replace("29/2/1997", "01/03/1997")
                        ;

                    DateTime temp;
                    if (DateTime.TryParseExact(trimmedValue, _inputFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out temp))
                    {
                        result = temp;
                    }
                    else
                    {
                        OnParseError(value);
                        result = _invalidDateValue;
                    }
                }
            }

            return result;
        }

        public string ParseValueAndFormat(string value)
        {
            var dt = ParseValue(value);
            string stringValue = null;

            if (dt.HasValue)
            {
                stringValue = dt.Value.ToString(_outputFormat);
            }

            return stringValue;
        }

        #endregion Methods
    }
}