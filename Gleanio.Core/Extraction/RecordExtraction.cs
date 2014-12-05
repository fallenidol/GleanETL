using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class RecordExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        #region Constructors

        protected RecordExtraction(BaseColumn[] columns, TextFile source, TExtractTarget target)
            : base(columns, source, target)
        {
            IsFirstLineOfRecord = (line, prevLine) => true;
            ParseRecord =
                record =>
                {
                    var firstOrDefault = record.FileLines.FirstOrDefault();
                    return firstOrDefault != null ? (record.FileLines.Any()
                                  ? new[] {new TextFileLine(firstOrDefault.OriginalLine)}
                                  : null) : null;
                };
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, TextFileLine, bool> IsFirstLineOfRecord { get; set; }

        public Func<TextFileRecord, IEnumerable<TextFileLine>> ParseRecord { get; set; }

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

            var enumerator = Source.EnumerateFileLines();
            while (enumerator.MoveNext())
            {
                var line = enumerator.Current;

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

            yield return currentTextFileRecord;
        }

        public override void ExtractToTarget()
        {
            var records = EnumerateRecords();

            var lines = FlattenRecordsIntoLines(records);

            var targetFileLines = new List<object[]>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var line in lines)
            {
                var rawLineValues = line.OriginalLine.Split(new[] {line.Delimiter}, StringSplitOptions.None);
                var parsedLineValues = ParseStringValues(rawLineValues);

                if (parsedLineValues != null && parsedLineValues.Length > 0)
                {
                    targetFileLines.Add(parsedLineValues);
                }
            }

            Target.CommitData(targetFileLines);

            Trace.WriteLine(string.Format("{0} finished.", Source.FilenameWithExtension));

            //Debug.WriteLine("*** " + Source.FilenameWithExtension.ToUpperInvariant() + " FINISHED. " + recordNumber + " RECORDS SAVED!!");
        }

        public IEnumerable<TextFileLine> FlattenRecordsIntoLines(IEnumerable<TextFileRecord> records)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
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

        #endregion Methods
    }
}