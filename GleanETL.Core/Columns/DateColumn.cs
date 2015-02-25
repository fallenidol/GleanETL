namespace GleanETL.Core.Columns
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using GleanETL.Core.Enumerations;

    public class DateColumn : BaseColumn<DateTime?>
    {
        #region Fields

        private const string DefaultOutputFormat = "yyyy-MM-dd";

        private readonly string[] _inputFormats;
        private readonly DateTime? _invalidDateValue;
        private readonly string _outputFormat;

        private static DateColumn _dc = new DateColumn();

        #endregion Fields

        #region Constructors

        public DateColumn(string columnName = null, string[] inputFormats = null,
            string outputFormat = DefaultOutputFormat,
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
                        "dd/MM/yyyy", 
                        "d/M/yyyy", 
                        "dd/M/yyyy", 
                        "d/MM/yyyy",
                        "ddMMyyyy", 
                        "dd/M/yy", 
                        "d/MM/yy", 
                        "d/M/yy",
                        "yyyy-MM-dd",
                        "yyyy-M-d",
                        "yyyy-MM-d",
                        "yyyy-M-dd"
                    };
                    break;
                case StandardDateFormats.UnitedStates:
                    formats = new[]
                    {
                        "MM/dd/yyyy", 
                        "M/d/yyyy", 
                        "M/dd/yyyy", 
                        "MM/d/yyyy",
                        "MMddyyyy", 
                        "M/dd/yy", 
                        "MM/d/yy", 
                        "M/d/yy",
                        "yyyy-MM-dd",
                        "yyyy-M-d",
                        "yyyy-MM-d",
                        "yyyy-M-dd"
                    };
                    break;
            }
            return formats;
        }

        //[DebuggerHidden]
        public static DateTime? ParseValue(string value, string[] validFormats, DateTime? invalidDateValue = null)
        {
            DateTime? result = invalidDateValue;

            if (value != null)
            {
                var trimmedValue = value.TrimAndRemoveConsecutiveWhitespace();

                if (trimmedValue.Length > 0)
                {
                    DateTime temp;
                    if (DateTime.TryParseExact(trimmedValue, validFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out temp))
                    {
                        result = temp;
                    }
                    else
                    {
                        throw new ParseException(value, typeof (DateTime));
                    }
                }
            }

            return result;
        }

        public override DateTime? ParseValue(string value)
        {
            try
            {
                var parsedValue = PreParseValue(value);

                return ParseValue(parsedValue, _inputFormats);
            }
            catch (ParseException pe)
            {
                OnParseError(pe.ValueBeingParsed, typeof(DateTime), pe.Message);

                return _invalidDateValue;
            }
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

    [Serializable]
    public class ParseException : Exception
    {
        #region Constructors

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public ParseException(string valueBeingParsed, Type targetType)
            : base(string.Format("'{0}' could not be parsed to [{1}].", valueBeingParsed, targetType.Name))
        {
            TargetType = targetType;
            ValueBeingParsed = valueBeingParsed;
        }

        #endregion Constructors

        #region Properties

        public Type TargetType
        {
            get;
            private set;
        }

        public string ValueBeingParsed
        {
            get;
            private set;
        }

        #endregion Properties
    }
}