namespace Gleanio.Framework.Extraction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Gleanio.Framework.Columns;
    using Gleanio.Framework.Source;
    using Gleanio.Framework.Target;

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

            OnExtractComplete();

            Debug.WriteLine("*** " + _source.Name.ToUpperInvariant() + " FINISHED. " + recordNumber + " RECORDS SAVED!!");
        }

        #endregion Methods
    }
}