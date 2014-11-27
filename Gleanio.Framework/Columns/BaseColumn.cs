namespace Gleanio.Framework.Columns
{
    using System;

    using Gleanio.Framework.EventArgs;

    public abstract class BaseColumn
    {
        #region Constructors

        protected BaseColumn(string columnName, Type dataType)
        {
            if (!dataType.IsClass && Nullable.GetUnderlyingType(dataType) == null)
            {
                throw new ArgumentException(string.Format("[{0}] is not a nullable type. Please supply a nullable type, such as [DateTime?]", dataType.Name));
            }

            DataType = dataType;
            ColumnName = columnName;
            ColumnDisplayName = columnName;
        }

        #endregion Constructors

        #region Events

        public event EventHandler<ParseErrorEventArgs> ParseError;

        #endregion Events

        #region Properties

        public string ColumnDisplayName
        {
            get;
            private set;
        }

        public string ColumnName
        {
            get;
            private set;
        }

        public Type DataType
        {
            get;
            private set;
        }

        public Func<string, string> PreParseFunction
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        protected void OnParseError(string valueBeingParsed, Exception exception)
        {
            OnParseError(new ParseErrorEventArgs(valueBeingParsed, exception));
        }

        protected void OnParseError(string valueBeingParsed, string message = "The value could not be parsed.")
        {
            OnParseError(new ParseErrorEventArgs(valueBeingParsed, message));
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

        #endregion Methods
    }

    public abstract class BaseColumn<T> : BaseColumn
    {
        #region Constructors

        protected BaseColumn(string columnName)
            : base(columnName, typeof(T))
        {
        }

        #endregion Constructors

        #region Methods

        public abstract T ParseValue(string value);

        #endregion Methods
    }
}