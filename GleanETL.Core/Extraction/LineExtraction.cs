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

        public LineExtraction(BaseColumn[] columns, IExtractSource source, TExtractTarget target,
            bool throwParseErrors = true)
            : base(columns, source, target, throwParseErrors)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] { ',' }, StringSplitOptions.None);
        }

        public Func<TextLine, string[]> SplitLineFunc { get; set; }

        public override void ExtractToTarget()
        {
            mutipleEnumeration = false;
            var sw = Stopwatch.StartNew();
            var linesToSave = EnumerateSourceLines();

            var extractDurationMs = sw.ElapsedMilliseconds;

            var dataWithoutIgnoredColumns = linesToSave.Select((o, i) => ValuesWithoutIgnoredColumns(o));

            var lineCount = Target.CommitData(dataWithoutIgnoredColumns);

            sw.Stop();

            var commitDurationMs = sw.ElapsedMilliseconds - extractDurationMs;

            OnExtractComplete(fileLinesRead, linesExtracted, lineCount, extractDurationMs, commitDurationMs,
                sw.ElapsedMilliseconds);
        }

        private IEnumerable<object[]> EnumerateSourceLines()
        {
            lock (enumeratorLock)
            {
                if (ThrowMultipleEnumerationError)
                {
                    if (mutipleEnumeration)
                    {
                        throw new Exception("Multiple enumeration of data!");
                    }
                }

                var enumerator = Source.EnumerateLines();

                while (enumerator.MoveNext())
                {
                    fileLinesRead++;

                    var line = enumerator.Current;
                    var rawLineValues = SplitLineFunc(line);

                    var parsedLineValues = ParseStringValues(rawLineValues);
                    if (!parsedLineValues.IsNullOrEmpty())
                    {
                        linesExtracted++;
                        yield return parsedLineValues;
                    }
                }

                mutipleEnumeration = true;
            }
        }

        private object[] ValuesWithoutIgnoredColumns(object[] row)
        {
            var ic = Columns.OfType<IgnoredColumn>();
            if (ic.Any())
            {
                var iic = ic.Select(column => Array.IndexOf(Columns, column));
                var io = row.Where((o, i) => iic.Contains(i));
                var valuesWithoutIgnoredColumns = row.Except(io).ToArray();
                return valuesWithoutIgnoredColumns;
            }
            return row;
        }
    }
}