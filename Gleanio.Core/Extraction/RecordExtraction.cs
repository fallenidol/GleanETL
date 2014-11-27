using Gleanio.Core.Source;

namespace Gleanio.Core.Extraction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Gleanio.Core.Columns;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;

    public class RecordExtraction<TExtractTarget> : Extract<TExtractTarget> where TExtractTarget : BaseExtractTarget
    {
        #region Fields

        private readonly List<TextFileLine> _newLines = new List<TextFileLine>();

        #endregion Fields

        #region Constructors

        public RecordExtraction(BaseColumn[] columns, TextFile source, TExtractTarget target)
            : base(columns, source, target)
        {
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

            foreach (var line in Source.LinesToImport)
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
                string[] rawLineValues = line.OriginalLine.Split(new[] { line.Delimiter }, StringSplitOptions.None);
                var parsedLineValues = ParseStringValues(rawLineValues);

                if (parsedLineValues != null && parsedLineValues.Length > 0)
                {
                    targetFileLines.Add(parsedLineValues);
                }
            }

            Target.SaveRows(targetFileLines.ToArray());

            Debug.WriteLine("*** " + Source.Name.ToUpperInvariant() + " FINISHED. " + recordNumber + " RECORDS SAVED!!");
        }

        #endregion Methods
    }
}