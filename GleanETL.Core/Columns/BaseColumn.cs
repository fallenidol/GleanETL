namespace Glean.Core.Columns
{
    using System;
    using Glean.Core.EventArgs;

    public abstract class BaseColumn
    {
        protected BaseColumn(string columnName, Type dataType)
        {
            if (dataType != null && !dataType.IsClass && Nullable.GetUnderlyingType(dataType) == null)
            {
                throw new ArgumentException(string.Format(
                    "[{0}] is not a nullable type. Please supply a nullable type, such as [DateTime?]", dataType.Name));
            }

            DataType = dataType;
            ColumnName = columnName;
            ColumnDisplayName = columnName;
        }

        public string ColumnDisplayName { get; }

        public string ColumnName { get; }

        public Type DataType { get; }

        public Func<string, string> PreParseFunction { get; set; }

        public event EventHandler<ParseErrorEventArgs> ParseError;

        protected void OnParseError(string valueBeingParsed, Type targetType, Exception exception)
        {
            OnParseError(new ParseErrorEventArgs(valueBeingParsed, exception, targetType));
        }

        protected void OnParseError(string valueBeingParsed, Type targetType,
            string message = "The value could not be parsed.")
        {
            OnParseError(new ParseErrorEventArgs(valueBeingParsed, message, targetType));
        }

        protected void OnParseError(ParseErrorEventArgs args)
        {
            if (ParseError != null)
            {
                ParseError.Invoke(this, args);
            }
        }

        protected string PreParseValue(string value)
        {
            if (PreParseFunction != null)
            {
                return PreParseFunction(value);
            }

            return value;
        }
    }

    public abstract class BaseColumn<T> : BaseColumn
    {
        protected BaseColumn(string columnName)
            : base(columnName, typeof(T))
        {
        }

        public abstract T ParseValue(string value);
    }
}