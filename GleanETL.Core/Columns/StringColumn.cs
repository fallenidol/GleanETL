namespace Glean.Core.Columns
{
    using System;
    using System.Text;
    using Glean.Core.Enumerations;

    public class StringColumn : BaseColumn<string>
    {
        private readonly bool encloseInDoubleQuotes;

        public StringColumn(
            string columnName = null,
            int maxLength = -1,
            bool encloseInDoubleQuotes = false,
            WhiteSpaceHandling whitespaceHandling = WhiteSpaceHandling.TrimLeadingAndTrailingWhiteSpace,
            StringCapitalisation stringCapitalisation = StringCapitalisation.DefaultDoNothing)
            : base(columnName)
        {
            WhiteSpaceHandling = whitespaceHandling;
            StringCapitalisation = stringCapitalisation;
            MaxLength = maxLength;
            this.encloseInDoubleQuotes = encloseInDoubleQuotes;
            DetectedMaxLength = -1;
        }

        public int DetectedMaxLength { get; internal set; }

        public int MaxLength { get; }

        public StringCapitalisation StringCapitalisation { get; }

        public WhiteSpaceHandling WhiteSpaceHandling { get; }

        public override string ParseValue(string value)
        {
            var returnValue = PreParseValue(value);

            if (value != null)
            {
                if (StringCapitalisation == StringCapitalisation.ToCamelCase &&
                    WhiteSpaceHandling == WhiteSpaceHandling.RemoveAllWhiteSpace)
                {
                    throw new InvalidOperationException(
                        "ToCamelCase cannot be used in conjuction with RemoveAllWhiteSpace");
                }

                switch (StringCapitalisation)
                {
                    case StringCapitalisation.ToLowercase:
                        returnValue = returnValue.ToLowerInvariant();
                        break;
                    case StringCapitalisation.ToUppercase:
                        returnValue = returnValue.ToUpperInvariant();
                        break;
                    case StringCapitalisation.ToCamelCase:
                        returnValue = ToPascalCase(returnValue);
                        break;
                }

                switch (WhiteSpaceHandling)
                {
                    case WhiteSpaceHandling.TrimLeadingAndTrailingWhiteSpace:
                        returnValue = returnValue.Trim();
                        break;
                    case WhiteSpaceHandling.RemoveAllWhiteSpace:
                        returnValue = returnValue.RemoveAllWhiteSpace();
                        break;
                    case WhiteSpaceHandling.TrimAndRemoveConsecutiveWhiteSpace:
                        returnValue = returnValue.TrimAndRemoveConsecutiveWhiteSpace();
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

                returnValue = encloseInDoubleQuotes
                    ? string.Format("\"{0}\"", returnValue)
                    : returnValue.TrimStart('"').TrimEnd('"');
            }
            return returnValue;
        }

        private string ToPascalCase(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var returnValue = string.Empty;

            if (input.Trim().Length > 0)
            {
                var sb = new StringBuilder();
                var arrWords = input.Split(new[] { Constants.SingleSpace }, StringSplitOptions.None);

                foreach (var word in arrWords)
                {
                    sb.Append(char.ToUpperInvariant(word[0]));
                    sb.Append(word.Substring(1).ToLowerInvariant());
                    sb.Append(Constants.SingleSpace);
                }

                returnValue = sb.ToString();
            }

            return returnValue;
        }
    }
}