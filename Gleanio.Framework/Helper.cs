using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using gleanio.framework.Columns;
using gleanio.framework.Source;

namespace gleanio.framework
{
    public class Helper
    {
        #region Methods

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
                        returnLines.Add(new TextFileLine(record.RecordNumber, string.Join("~", values), "~"));
                    }
                }
                else
                {
                    throw new ArgumentException("There can be only 1!!!");
                }

                return returnLines;
            };
        }

        public static DataColumn GetDataColumn(BaseColumn col)
        {
            var dc = new DataColumn(col.ColumnName);
            dc.Caption = col.ColumnName;
            dc.AllowDBNull = true;
            dc.ReadOnly = true;

            Type colType = col.GetType();

            if (colType == typeof(StringColumn))
            {
                dc.DataType = typeof(string);

                var c = col as StringColumn;
                dc.MaxLength = c.MaxLength;
            }
            if (colType == typeof(StringNoWhitespaceColumn))
            {
                dc.DataType = typeof(string);

                var c = col as StringColumn;
                dc.MaxLength = c.MaxLength;
            }
            if (colType == typeof(IntColumn))
            {
                dc.DataType = typeof(int);
            }
            if (colType == typeof(DecimalColumn))
            {
                dc.DataType = typeof(decimal);
            }
            if (colType == typeof(MoneyColumn))
            {
                dc.DataType = typeof(decimal);
            }
            if (colType == typeof(DateColumn))
            {
                dc.DataType = typeof(DateTime);
                dc.DateTimeMode = DataSetDateTime.Local;
            }

            return dc;
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

                    var dc = new DateColumn("", formats);
                    string test = line.OriginalLine.Substring(0, numberOfCharsToTest);
                    validLine = dc.ParseValue(test).HasValue;

                    //if (!validLine && test.Contains("/"))
                    //{
                    //    Debug.WriteLine(string.Format("LineStartsWithDate: FAILED TO PARSE DATE [{0}]", test));
                    //}
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

        #endregion Methods
    }
}