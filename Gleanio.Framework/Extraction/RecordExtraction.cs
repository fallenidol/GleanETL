using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using gleanio.framework.Columns;
using gleanio.framework.Source;
using gleanio.framework.Target;

namespace gleanio.framework.Extraction
{
    public class RecordExtraction : Extract
    {
        #region Fields

        private readonly List<TextFileLine> _newLines = new List<TextFileLine>();
        private readonly TextFile _source;
        private readonly IExtractTarget _target;

        #endregion Fields

        #region Constructors

        public RecordExtraction(TextFile source, IExtractTarget target)
            : base(source, target)
        {
            _source = source;
            _target = target;

            IsFirstLineOfRecord = (line, prevLine) => true;
            ParseRecord = record => record.FileLines.Any() ? new[] { new TextFileLine(record.RecordNumber, record.FileLines.FirstOrDefault().OriginalLine) } : null;
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, TextFileLine, bool> IsFirstLineOfRecord
        {
            get;
            set;
        }

        public Func<TextFileRecord, IEnumerable<TextFileLine>> ParseRecord
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public override void ExtractToTarget()
        {
            var records = new List<TextFileRecord>();
            int recordNumber = 0;
            TextFileRecord currentTextFileRecord = null;
            TextFileLine previousLine = null;

            foreach (var line in _source.LinesToImport)
            {
                if (IsFirstLineOfRecord(line, previousLine))
                {
                    recordNumber++;

                    currentTextFileRecord = new TextFileRecord(recordNumber);
                    records.Add(currentTextFileRecord);
                }

                if (currentTextFileRecord != null)
                {
                    currentTextFileRecord.AddFileLine(line);
                }

                previousLine = line;
            }

            _newLines.Clear();
            foreach (var record in records)
            {
                if (record.FileLines.Any())
                {
                    var recordLines = ParseRecord(record);
                    if (recordLines != null)
                    {
                        _newLines.AddRange(recordLines);
                    }
                }
            }

            var targetFileLines = new List<object[]>();
            foreach (var line in _newLines)
            {
                object[] values = new object[_source.Columns.Length];
                string[] colValues = line.OriginalLine.Split(new[] { line.Delimiter }, StringSplitOptions.None);
                int i = 0;

                foreach (var col in _source.Columns)
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

            _target.SaveRows(targetFileLines.ToArray());

            Debug.WriteLine("*** " + _source.Name.ToUpperInvariant() + " FINISHED. " + recordNumber + " RECORDS SAVED!!");
        }

        #endregion Methods
    }
}