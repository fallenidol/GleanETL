namespace Glean.Core.Extraction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Glean.Core.Columns;
    using Glean.Core.Source;
    using Glean.Core.Target;

    public class RecordExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        private readonly object enumeratorLock1 = new object();

        private readonly object enumeratorLock2 = new object();

        private long fileLinesRead;

        private long linesExtracted;

        private bool mutipleEnumeration1;

        private bool mutipleEnumeration2;

        public RecordExtraction(BaseColumn[] columns, IExtractSource source, TExtractTarget target, bool throwParseErrors = true)
            : base(columns, source, target, throwParseErrors)
        {
            this.IsFirstLineOfRecord = (line, prevLine) => true;
            this.ParseRecord = record =>
            {
                var firstOrDefault = record.FileLines.FirstOrDefault();
                return firstOrDefault != null
                    ? (record.FileLines.Any() ? new[] { TextFileRecordLine.New(firstOrDefault.OriginalLine, Constants.RecordLineDelimiter) } : null)
                    : null;
            };
        }

        public Func<TextLine, TextLine, bool> IsFirstLineOfRecord { get; set; }

        public Func<TextFileRecord, IEnumerable<TextFileRecordLine>> ParseRecord { get; set; }

        public IEnumerable<TextFileRecord> EnumerateRecords()
        {
            lock (this.enumeratorLock1)
            {
                if (this.ThrowMultipleEnumerationError)
                {
                    if (this.mutipleEnumeration1)
                    {
                        throw new Exception("Multiple enumeration of data!");
                    }
                }

                TextFileRecord currentTextFileRecord = null;
                TextLine previousLine = null;

                var enumerator = this.Source.EnumerateLines();
                while (enumerator.MoveNext())
                {
                    this.fileLinesRead++;

                    var line = enumerator.Current;

                    if (this.IsFirstLineOfRecord(line, previousLine))
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

                this.mutipleEnumeration1 = true;
            }
        }

        public override void ExtractToTarget()
        {
            this.mutipleEnumeration1 = false;
            this.mutipleEnumeration2 = false;

            var sw = Stopwatch.StartNew();

            var records = this.EnumerateRecords();

            var lines = this.FlattenRecordsIntoLines(records);

            var targetFileLines = new List<object[]>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var line in lines)
            {
                var rawLineValues = line.Text.Split(new[] { line.Delimiter }, StringSplitOptions.None);
                var parsedLineValues = this.ParseStringValues(rawLineValues);

                if ((parsedLineValues != null) && (parsedLineValues.Length > 0))
                {
                    this.linesExtracted++;
                    targetFileLines.Add(parsedLineValues);
                }
            }

            var extractDurationMs = sw.ElapsedMilliseconds;

            var lineCount = this.Target.CommitData(targetFileLines);

            sw.Stop();
            var commitDurationMs = sw.ElapsedMilliseconds - extractDurationMs;

            this.OnExtractComplete(this.fileLinesRead, this.linesExtracted, lineCount, extractDurationMs, commitDurationMs, sw.ElapsedMilliseconds);
        }

        public IEnumerable<TextFileRecordLine> FlattenRecordsIntoLines(IEnumerable<TextFileRecord> records)
        {
            lock (this.enumeratorLock2)
            {
                if (this.ThrowMultipleEnumerationError)
                {
                    if (this.mutipleEnumeration2)
                    {
                        throw new Exception("Multiple enumeration of data!");
                    }
                }

                foreach (var record in records)
                {
                    if (record.FileLines.Any())
                    {
                        var recordLines = this.ParseRecord(record);
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

                this.mutipleEnumeration2 = true;
            }
        }
    }
}