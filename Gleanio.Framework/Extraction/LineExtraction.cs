using System;
using System.Collections.Generic;
using System.Diagnostics;
using gleanio.framework.Columns;
using gleanio.framework.Source;
using gleanio.framework.Target;

namespace gleanio.framework.Extraction
{
    public class LineExtraction : Extract
    {
        #region Constructors

        public LineExtraction(TextFile source, IExtractTarget target)
            : base(source, target)
        {
            ParseLine = line => line.OriginalLine.Split(',');
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, string[]> ParseLine
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public override void ExtractToTarget()
        {
            var targetFileLines = new List<object[]>();

            foreach (var line in Source.LinesToImport)
            {
                string[] colValues = ParseLine(line);
                var values = new object[Source.Columns.Length];
                int i = 0;

                foreach (var col in Source.Columns)
                {
                    Type colType = col.GetType();

                    if (colType == typeof(StringColumn))
                    {
                        values[i] = ((StringColumn)col).ParseValue(colValues[i]);
                    }
                    if (colType == typeof(StringNoWhitespaceColumn))
                    {
                        values[i] = ((StringNoWhitespaceColumn)col).ParseValue(colValues[i]);
                    }
                    if (colType == typeof(IntColumn))
                    {
                        values[i] = ((IntColumn)col).ParseValue(colValues[i]);
                    }
                    if (colType == typeof(DecimalColumn))
                    {
                        values[i] = ((DecimalColumn)col).ParseValue(colValues[i]);
                    }
                    if (colType == typeof(MoneyColumn))
                    {
                        values[i] = ((MoneyColumn)col).ParseValue(colValues[i]);
                    }
                    if (colType == typeof(DateColumn))
                    {
                        values[i] = ((DateColumn)col).ParseValueAndFormat(colValues[i]);
                    }

                    i++;
                }

                targetFileLines.Add(values);
            }

            Target.SaveRows(targetFileLines.ToArray());

            Debug.WriteLine("*** " + Source.Name.ToUpperInvariant() + " FINISHED. " + targetFileLines.Count + " LINES SAVED!!");
        }

        #endregion Methods
    }
}