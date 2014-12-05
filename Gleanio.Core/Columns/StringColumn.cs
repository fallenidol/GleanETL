using System;
using System.Text;
using Gleanio.Core.Enumerations;

namespace Gleanio.Core.Columns
{
    public class StringColumn : BaseColumn<string>
    {
        #region Constructors

        public StringColumn(string columnName, int maxLength = -1, bool encloseInDoubleQuotes = false,
            WhitespaceHandling whitespaceHandling = WhitespaceHandling.TrimLeadingAndTrailingWhitespace,
            StringCapitalisation stringCapitalisation = StringCapitalisation.DefaultDoNothing)
            : base(columnName)
        {
            _whitespaceHandling = whitespaceHandling;
            _stringCapitalisation = stringCapitalisation;
            _maxLength = maxLength;
            _encloseInDoubleQuotes = encloseInDoubleQuotes;
        }

        #endregion Constructors

        #region Fields

        private readonly bool _encloseInDoubleQuotes;
        private readonly int _maxLength;
        private readonly StringCapitalisation _stringCapitalisation = StringCapitalisation.DefaultDoNothing;
        private readonly WhitespaceHandling _whitespaceHandling = WhitespaceHandling.DefaultDoNothing;

        #endregion Fields

        #region Properties

        public int MaxLength
        {
            get { return _maxLength; }
        }

        public StringCapitalisation StringCapitalisation
        {
            get { return _stringCapitalisation; }
        }

        public WhitespaceHandling WhitespaceHandling
        {
            get { return _whitespaceHandling; }
        }

        #endregion Properties

        #region Methods

        public override string ParseValue(string value)
        {
            var returnValue = PreParseValue(value);

            if (value != null)
            {
                if (StringCapitalisation == StringCapitalisation.ToCamelCase &&
                    WhitespaceHandling == WhitespaceHandling.RemoveAllWhitespace)
                {
                    throw new InvalidOperationException(
                        "ToCamelCase cannot be used in conjuction with RemoveAllWhiteSpace");
                }

                switch (StringCapitalisation)
                {
                    case StringCapitalisation.ToLowerCase:
                        returnValue = returnValue.ToLowerInvariant();
                        break;
                    case StringCapitalisation.ToUpperCase:
                        returnValue = returnValue.ToUpperInvariant();
                        break;
                    case StringCapitalisation.ToCamelCase:
                        returnValue = ToPascalCase(returnValue);
                        break;
                }

                switch (WhitespaceHandling)
                {
                    case WhitespaceHandling.TrimLeadingAndTrailingWhitespace:
                        returnValue = returnValue.Trim();
                        break;
                    case WhitespaceHandling.RemoveAllWhitespace:
                        returnValue = returnValue.RemoveAllWhitespace();
                        break;
                    case WhitespaceHandling.TrimAndRemoveConsecutiveWhitespace:
                        returnValue = returnValue.TrimAndRemoveConsecutiveWhitespace();
                        break;
                }

                if (MaxLength > 0)
                {
                    returnValue = returnValue.Substring(0, Math.Min(MaxLength, returnValue.Length));
                }
                else if (MaxLength == 0)
                {
                    returnValue = string.Empty;
                }

                if (_encloseInDoubleQuotes)
                {
                    returnValue = string.Format("\"{0}\"", returnValue);
                }
                else
                {
                    returnValue = returnValue.TrimStart('"').TrimEnd('"');
                }
            }
            return returnValue;
        }

        private string ToPascalCase(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException();
            }

            var returnValue = string.Empty;

            if (input.Trim().Length > 0)
            {
                var sb = new StringBuilder();
                var arrWords = input.Split(new[] {Constants.SingleSpace}, StringSplitOptions.None);

                foreach (var word in arrWords)
                {
                    sb.Append(Char.ToUpperInvariant(word[0]));
                    sb.Append(word.Substring(1).ToLowerInvariant());
                    sb.Append(Constants.SingleSpace);
                }

                returnValue = sb.ToString();
            }

            return returnValue;
        }

        #endregion Methods
    }
}