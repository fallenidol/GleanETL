namespace Glean.Core.Extraction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Glean.Core.Columns;
    using Glean.Core.Source;
    using Glean.Core.Target;

    public class LineExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        private readonly object enumeratorLock = new object();

        private long fileLinesRead;

        private long linesExtracted;

        private bool mutipleEnumeration;

        public LineExtraction(BaseColumn[] columns, IExtractSource source, TExtractTarget target, bool throwParseErrors = true)
            : base(columns, source, target, throwParseErrors)
        {
            this.SplitLineFunc = line => line.OriginalLine.Split(new[] { ',' }, StringSplitOptions.None);
        }

        public Func<TextLine, string[]> SplitLineFunc { get; set; }

        public override void ExtractToTarget()
        {
            this.mutipleEnumeration = false;
            var sw = Stopwatch.StartNew();
            var linesToSave = this.EnumerateSourceLines();

            var extractDurationMs = sw.ElapsedMilliseconds;

            var dataWithoutIgnoredColumns = linesToSave.Select((o, i) => this.ValuesWithoutIgnoredColumns(o));

            var lineCount = this.Target.CommitData(dataWithoutIgnoredColumns);

            sw.Stop();

            var commitDurationMs = sw.ElapsedMilliseconds - extractDurationMs;

            this.OnExtractComplete(this.fileLinesRead, this.linesExtracted, lineCount, extractDurationMs, commitDurationMs, sw.ElapsedMilliseconds);
        }

        private IEnumerable<object[]> EnumerateSourceLines()
        {
            lock (this.enumeratorLock)
            {
                if (this.ThrowMultipleEnumerationError)
                {
                    if (this.mutipleEnumeration)
                    {
                        throw new Exception("Multiple enumeration of data!");
                    }
                }

                var enumerator = this.Source.EnumerateLines();

                while (enumerator.MoveNext())
                {
                    this.fileLinesRead++;

                    var line = enumerator.Current;
                    var rawLineValues = this.SplitLineFunc(line);

                    var parsedLineValues = this.ParseStringValues(rawLineValues);
                    if (!parsedLineValues.IsNullOrEmpty())
                    {
                        this.linesExtracted++;
                        yield return parsedLineValues;
                    }
                }

                this.mutipleEnumeration = true;
            }
        }

        private object[] ValuesWithoutIgnoredColumns(object[] row)
        {
            var ic = this.Columns.OfType<IgnoredColumn>();
            if (ic.Any())
            {
                var iic = ic.Select(column => Array.IndexOf(this.Columns, column));
                var io = row.Where((o, i) => iic.Contains(i));
                var valuesWithoutIgnoredColumns = row.Except(io).ToArray();
                return valuesWithoutIgnoredColumns;
            }
            else
            {
                return row;
            }
        }
    }
}