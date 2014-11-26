using System;
using System.Text;
using gleanio.framework.Enumerations;

namespace gleanio.framework.Columns
{
    public class StringColumn : BaseColumn<string>
    {
        #region Fields

        private readonly Capitalisation _capitalisation = Capitalisation.DefaultDoNothing;
        private readonly bool _encloseInDoubleQuotes;
        private readonly int _maxLength;
        private readonly WhitespaceHandling _whitespaceHandling = WhitespaceHandling.DefaultDoNothing;

        #endregion Fields

        #region Constructors

        public StringColumn(string columnName, int maxLength, bool encloseInDoubleQuotes = false, WhitespaceHandling whitespaceHandling = WhitespaceHandling.TrimLeadingAndTrailingWhitespace, Capitalisation capitalisation = Capitalisation.DefaultDoNothing)
            : base(columnName)
        {
            _whitespaceHandling = whitespaceHandling;
            _capitalisation = capitalisation;
            _maxLength = maxLength;
            _encloseInDoubleQuotes = encloseInDoubleQuotes;
        }

        #endregion Constructors

        #region Properties

        public Capitalisation Capitalisation
        {
            get { return _capitalisation; }
        }

        public int MaxLength
        {
            get { return _maxLength; }
        }

        public WhitespaceHandling WhitespaceHandling
        {
            get { return _whitespaceHandling; }
        }

        #endregion Properties

        #region Methods

        public override string ParseValue(string value)
        {
            string returnValue = PreParseValue(value);

            if (value != null)
            {
                if (Capitalisation == Capitalisation.ToCamelCase && WhitespaceHandling == WhitespaceHandling.RemoveAllWhitespace)
                {
                    throw new InvalidOperationException("ToCamelCase cannot be used in conjuction with RemoveAllWhiteSpace");
                }

                switch (Capitalisation)
                {
                    case Capitalisation.ToLowerCase:
                        returnValue = returnValue.ToLowerInvariant();
                        break;
                    case Capitalisation.ToUpperCase:
                        returnValue = returnValue.ToUpperInvariant();
                        break;
                    case Capitalisation.ToCamelCase:
                        returnValue = ToPascalCase(returnValue);
                        break;
                }

                switch (WhitespaceHandling)
                {
                    case WhitespaceHandling.TrimLeadingAndTrailingWhitespace:
                        returnValue = returnValue.Trim();
                        break;
                    case WhitespaceHandling.RemoveAllWhitespace:
                        while (returnValue.Contains(Constants.SingleSpace))
                        {
                            returnValue = returnValue.Replace(Constants.SingleSpace, string.Empty);
                        }
                        break;
                    case WhitespaceHandling.RemoveAllButOneSpace:
                        while (returnValue.Contains("  "))
                        {
                            returnValue = returnValue.Replace("  ", Constants.SingleSpace);
                        }
                        break;
                }

                if (MaxLength > 0)
                {
                    returnValue = returnValue.Substring(0, Math.Min(MaxLength, returnValue.Length));
                }

                if (_encloseInDoubleQuotes)
                {
                    returnValue = string.Format("\"{0}\"", returnValue);
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

            string returnValue = string.Empty;

            if (input.Trim().Length > 0)
            {
                var sb = new StringBuilder();
                string[] arrWords = input.Split(new[] { Constants.SingleSpace }, StringSplitOptions.None);

                foreach (string word in arrWords)
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