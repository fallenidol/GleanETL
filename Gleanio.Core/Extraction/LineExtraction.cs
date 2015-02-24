namespace Gleanio.Core.Extraction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Gleanio.Core.Columns;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;

    public class LineExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        #region Fields

        private readonly object _enumeratorLock = new object();

        private long _fileLinesRead;
        private long _linesExtracted;
        private bool _mutipleEnumeration = false;

        #endregion Fields

        #region Constructors

        public LineExtraction(BaseColumn[] columns, IExtractSource source, TExtractTarget target, bool throwParseErrors = true)
            : base(columns, source, target, throwParseErrors)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] { ',' }, StringSplitOptions.None);
        }

        #endregion Constructors

        #region Properties

        public Func<TextLine, string[]> SplitLineFunc
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public override void ExtractToTarget()
        {
            _mutipleEnumeration = false;
            var sw = Stopwatch.StartNew();
            var linesToSave = EnumerateSourceLines();

            var extractDurationMs = sw.ElapsedMilliseconds;

            var dataWithoutIgnoredColumns = Enumerable.Select<object[], object[]>(linesToSave, (o, i) => ValuesWithoutIgnoredColumns(o));

            var lineCount = Target.CommitData(dataWithoutIgnoredColumns);

            sw.Stop();

            var commitDurationMs = sw.ElapsedMilliseconds - extractDurationMs;

            OnExtractComplete(_fileLinesRead, _linesExtracted, lineCount, extractDurationMs, commitDurationMs,
                sw.ElapsedMilliseconds);
        }

        private IEnumerable<object[]> EnumerateSourceLines()
        {
            lock (_enumeratorLock)
            {
                if (ThrowMultipleEnumerationError)
                {
                    if (_mutipleEnumeration)
                    {
                        throw new Exception("Multiple enumeration of data!");
                    }
                }

                var enumerator = Source.EnumerateLines();

                while (enumerator.MoveNext())
                {
                    _fileLinesRead++;

                    var line = enumerator.Current;
                    var rawLineValues = SplitLineFunc(line);

                    var parsedLineValues = ParseStringValues(rawLineValues);
                    if (!parsedLineValues.IsNullOrEmpty())
                    {
                        _linesExtracted++;
                        yield return parsedLineValues;
                    }
                }

                _mutipleEnumeration = true;
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
            else
            {
                return row;
            }
        }

        #endregion Methods
    }
}