using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Gleanio.Core.Columns;
using Gleanio.Core.Source;

namespace Gleanio.Core
{
    public class HelperFunctions
    {
        public static Func<TextFileRecord, IEnumerable<TextFileLine>> CharacterDelimitedSingleLineRecordParser(char inputDelimiter)
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
                        returnLines.Add(new TextFileLine(record.RecordNumber, String.Join("~", values), "~"));
                    }
                }
                else
                {
                    throw new InvalidProgramException("There can be only 1!!!");
                }

                return returnLines;
            };
        }

        public static Func<TextFileLine, bool> LineDoesNotStartsWithStringValue(int numberOfCharsToTest, string stringToMatch)
        {
            return line => line.LineLength >= numberOfCharsToTest && !line.OriginalLine.Substring(0, numberOfCharsToTest).Trim().Equals(stringToMatch);
        }

        public static Func<TextFileLine, bool> LineStartsWithDate(int numberOfCharsToTest, string[] formats)
        {
            return line =>
            {
                bool validLine = false;

                if (line.LineLength >= numberOfCharsToTest)
                {

                    var dc = new DateColumn(string.Empty, formats);
                    string test = line.OriginalLine.Substring(0, numberOfCharsToTest);
                    validLine = dc.ParseValue(test).HasValue;
                }
                return validLine;

            };
        }

        public static Func<TextFileLine, bool> LineStartsWithInteger(int numberOfCharsToTest)
        {
            int xx;

            return line => line.LineLength >= numberOfCharsToTest && int.TryParse(line.OriginalLine.Substring(0, numberOfCharsToTest).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out xx);
        }

        public static Func<TextFileLine, bool> LineStartsWithStringValue(int numberOfCharsToTest, string stringToMatch)
        {
            return line => line.LineLength >= numberOfCharsToTest && line.OriginalLine.Substring(0, numberOfCharsToTest).Trim().Equals(stringToMatch);
        }
    }
}
