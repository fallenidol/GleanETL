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

        public RecordExtraction(BaseColumn[] columns, IExtractSource source, TExtractTarget target, bool throwParseErrors = true)
            : base(columns, source, target, throwParseErrors)
        {
            IsFirstLineOfRecord = (line, prevLine) => true;
            ParseRecord =
                record =>
                {
                    var firstOrDefault = record.FileLines.FirstOrDefault();
                    return firstOrDefault != null
                        ? (record.FileLines.Any()
                            ? new[] {TextFileRecordLine.New(firstOrDefault.OriginalLine, Constants.RecordLineDelimiter)}
                            : null)
                        : null;
                };
        }

        #endregion Constructors

        #region Properties

        public Func<TextLine, TextLine, bool> IsFirstLineOfRecord { get; set; }

        public Func<TextFileRecord, IEnumerable<TextFileRecordLine>> ParseRecord { get; set; }

        #endregion Properties

        #region Methods

        private long _fileLinesRead;
        private long _linesExtracted;

        public IEnumerable<TextFileRecord> EnumerateRecords()
        {
            TextFileRecord currentTextFileRecord = null;
            TextLine previousLine = null;

            var enumerator = Source.EnumerateLines();
            while (enumerator.MoveNext())
            {
                _fileLinesRead++;

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
            var sw = Stopwatch.StartNew();

            var records = EnumerateRecords();

            var lines = FlattenRecordsIntoLines(records);

            var targetFileLines = new List<object[]>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var line in lines)
            {
                var rawLineValues = line.Text.Split(new[] {line.Delimiter}, StringSplitOptions.None);
                var parsedLineValues = ParseStringValues(rawLineValues);

                if (parsedLineValues != null && parsedLineValues.Length > 0)
                {
                    _linesExtracted++;
                    targetFileLines.Add(parsedLineValues);
                }
            }

            var extractDurationMs = sw.ElapsedMilliseconds;

            var lineCount = Target.CommitData(targetFileLines);

            sw.Stop();
            var commitDurationMs = sw.ElapsedMilliseconds - extractDurationMs;

            OnExtractComplete(_fileLinesRead, _linesExtracted, lineCount, extractDurationMs, commitDurationMs,
                sw.ElapsedMilliseconds);
        }

        public IEnumerable<TextFileRecordLine> FlattenRecordsIntoLines(IEnumerable<TextFileRecord> records)
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
                            if (line != null)
                            {
                                yield return line;
                            }
                        }
                    }
                }
            }
        }

        #endregion Methods
    }
}