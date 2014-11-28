namespace Gleanio.Core.Extraction
{
    using Gleanio.Core.Columns;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ExtractRecordsToDatabase : RecordExtraction<DatabaseTableTarget>
    {
        #region Constructors

        public ExtractRecordsToDatabase(BaseColumn[] columns, TextFile source, DatabaseTableTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }

    public class ExtractRecordsToSeparatedValueFile : RecordExtraction<SeparatedValueFileTarget>
    {
        #region Constructors

        public ExtractRecordsToSeparatedValueFile(BaseColumn[] columns, TextFile source, SeparatedValueFileTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }

    public class ExtractRecordsToTrace : RecordExtraction<TraceOutputTarget>
    {
        #region Constructors

        public ExtractRecordsToTrace(BaseColumn[] columns, TextFile source, TraceOutputTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }

    public class RecordExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        #region Constructors

        protected RecordExtraction(BaseColumn[] columns, TextFile source, TExtractTarget target)
            : base(columns, source, target)
        {
            IsFirstLineOfRecord = (line, prevLine) => true;
            ParseRecord = record => record.FileLines.Any() ? new[] { new TextFileLine(record.FileLines.FirstOrDefault().OriginalLine) } : null;
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

        public override void AfterExtract()
        {
            throw new NotImplementedException();
        }

        public override void BeforeExtract()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TextFileRecord> EnumerateRecords()
        {
            TextFileRecord currentTextFileRecord = null;
            TextFileLine previousLine = null;

            foreach (var line in Source.EnumerateFileLines())
            {
                if (IsFirstLineOfRecord(line, previousLine))
                {
                    if (currentTextFileRecord != null)
                    {
                        yield return currentTextFileRecord;
                    }

                    currentTextFileRecord = new TextFileRecord();
                }

                if (currentTextFileRecord != null)
                {
                    currentTextFileRecord.AddFileLine(line);
                }

                previousLine = line;
            }
        }

        public IEnumerable<TextFileLine> FlattenRecordsIntoLines(IEnumerable<TextFileRecord> records)
        {
            foreach (var record in records)
            {
                if (record.FileLines.Any())
                {
                    var recordLines = ParseRecord(record);
                    if (recordLines != null)
                    {
                        foreach (var line in recordLines)
                        {
                            yield return line;
                        }
                    }
                }
            }
        }

        public override void ExtractToTarget()
        {
            var records = EnumerateRecords();

            var lines = FlattenRecordsIntoLines(records);

            var targetFileLines = new List<object[]>();
            foreach (var line in lines)
            {
                string[] rawLineValues = line.OriginalLine.Split(new[] { line.Delimiter }, StringSplitOptions.None);
                var parsedLineValues = ParseStringValues(rawLineValues);

                if (parsedLineValues != null && parsedLineValues.Length > 0)
                {
                    targetFileLines.Add(parsedLineValues);
                }
            }

            Target.CommitData(targetFileLines);

            //Debug.WriteLine("*** " + Source.FilenameWithExtension.ToUpperInvariant() + " FINISHED. " + recordNumber + " RECORDS SAVED!!");
        }

        #endregion Methods
    }
}