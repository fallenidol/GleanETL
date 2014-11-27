namespace Gleanio.Framework.Extraction
{
    using Gleanio.Framework.Columns;
    using Gleanio.Framework.Source;
    using Gleanio.Framework.Target;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    public class LineExtraction : Extract
    {
        #region Constructors

        public LineExtraction(TextFile source, IExtractTarget target)
            : base(source, target)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] { ',' }, StringSplitOptions.None);
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, string[]> SplitLineFunc { get; set; }

        #endregion Properties

        #region Methods

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

        public override void ExtractToTarget()
        {
            var targetFileLines = new List<object[]>();

            int columnCount = Source.Columns.Length;

            Source.LinesToImport.ForEach(line =>
            {
                var rawLineValues = SplitLineFunc(line);
                var parsedLineValues = new object[columnCount];

                Source.Columns.ForEach((i, column) => 
                {
                    Type colType = column.GetType();

                    if (colType == typeof(StringNoWhitespaceColumn))
                    {
                        parsedLineValues[i] = ((StringNoWhitespaceColumn)column).ParseValue(rawLineValues[i]);
                    }
                    else if (colType == typeof(StringColumn))
                    {
                        parsedLineValues[i] = ((StringColumn)column).ParseValue(rawLineValues[i]);
                    }
                    else if (colType == typeof(IntColumn))
                    {
                        parsedLineValues[i] = ((IntColumn)column).ParseValue(rawLineValues[i]);
                    }
                    else if (colType == typeof(DecimalColumn))
                    {
                        parsedLineValues[i] = ((DecimalColumn)column).ParseValue(rawLineValues[i]);
                    }
                    else if (colType == typeof(MoneyColumn))
                    {
                        parsedLineValues[i] = ((MoneyColumn)column).ParseValue(rawLineValues[i]);
                    }
                    else if (colType == typeof(DateColumn))
                    {
                        parsedLineValues[i] = ((DateColumn)column).ParseValueAndFormat(rawLineValues[i]);
                    }
                    else
                    {
                        throw new NotImplementedException("Column type not implemented.");
                    }
                });

                targetFileLines.Add(parsedLineValues);
            });

            Target.SaveRows(targetFileLines.ToArray());

            OnExtractComplete();

            Debug.WriteLine("*** " + Source.Name.ToUpperInvariant() + " FINISHED. " + targetFileLines.Count + " LINES SAVED!!");
        }

        #endregion Methods
    }
}