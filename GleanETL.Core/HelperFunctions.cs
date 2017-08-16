namespace Glean.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Glean.Core.Columns;
    using Glean.Core.Extraction;
    using Glean.Core.Source;

    public static class HelperFunctions
    {
        public static Func<TextFileRecord, IEnumerable<TextFileRecordLine>>
            CharacterDelimitedSingleLineRecordParser(char inputDelimiter)
        {
            return record =>
            {
                List<TextFileRecordLine> returnLines = null;

                if (record.FileLines.Count() == 1)
                {
                    var line = record.FileLines.FirstOrDefault();

                    if (line != null)
                    {
                        var values = line.OriginalLine.Split(inputDelimiter);
                        returnLines = new List<TextFileRecordLine>
                        {
                            TextFileRecordLine.New(string.Join(Constants.RecordLineDelimiter, values),
                                Constants.RecordLineDelimiter)
                        };
                    }
                }
                else
                {
                    throw new InvalidProgramException("There can be only 1!!!");
                }

                return returnLines;
            };
        }

        public static Func<string, bool> LineDoesNotStartsWithStringValue(int numberOfCharsToTest, string stringToMatch)
        {
            return line => line.Length >= numberOfCharsToTest &&
                           !line.Substring(0, numberOfCharsToTest).Trim().Equals(stringToMatch);
        }

        public static Func<string, bool> LineStartsWithDate(int numberOfCharsToTest, string[] formats)
        {
            return line =>
            {
                var validLine = false;

                if (line.Length >= numberOfCharsToTest)
                {
                    var test = line.Substring(0, numberOfCharsToTest);
                    validLine = DateColumn.ParseValue(test, formats).HasValue;
                }
                return validLine;
            };
        }

        public static Func<string, bool> LineStartsWithInteger(int numberOfCharsToTest)
        {
            int xx;

            return
                line =>
                    line.Length >= numberOfCharsToTest &&
                    int.TryParse(line.Substring(0, numberOfCharsToTest).Trim(), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out xx);
        }

        public static Func<string, bool> LineStartsWithStringValue(int numberOfCharsToTest, string stringToMatch)
        {
            return line => line.Length >= numberOfCharsToTest &&
                           line.Substring(0, numberOfCharsToTest).Trim().Equals(stringToMatch);
        }
    }
}