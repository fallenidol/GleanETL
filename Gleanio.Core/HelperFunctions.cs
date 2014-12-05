using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gleanio.Core.Columns;
using Gleanio.Core.Source;

namespace Gleanio.Core
{
    public class HelperFunctions
    {
        #region Methods

        public static Func<TextFileRecord, IEnumerable<TextFileLine>> CharacterDelimitedSingleLineRecordParser(
            char inputDelimiter)
        {
            return record =>
            {
                List<TextFileLine> returnLines = null;

                if (record.FileLines.Count() == 1)
                {
                    var line = record.FileLines.FirstOrDefault();

                    if (line != null)
                    {
                        var values = line.OriginalLine.Split(inputDelimiter);
                        returnLines = new List<TextFileLine>();
                        returnLines.Add(new TextFileLine(String.Join("~", values), "~"));
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
            return
                line =>
                    line.Length >= numberOfCharsToTest &&
                    !line.Substring(0, numberOfCharsToTest).Trim().Equals(stringToMatch);
        }

        public static Func<string, bool> LineStartsWithDate(int numberOfCharsToTest, string[] formats)
        {
            return line =>
            {
                var validLine = false;

                if (line.Length >= numberOfCharsToTest)
                {
                    var dc = new DateColumn(string.Empty, formats);
                    var test = line.Substring(0, numberOfCharsToTest);
                    validLine = dc.ParseValue(test).HasValue;
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
            return
                line =>
                    line.Length >= numberOfCharsToTest &&
                    line.Substring(0, numberOfCharsToTest).Trim().Equals(stringToMatch);
        }

        #endregion Methods
    }
}