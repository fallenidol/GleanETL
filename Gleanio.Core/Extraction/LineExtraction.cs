using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class LineExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        #region Constructors

        public LineExtraction(BaseColumn[] columns, IExtractSource source, TExtractTarget target, bool throwParseErrors = true)
            : base(columns, source, target, throwParseErrors)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] {','}, StringSplitOptions.None);
        }

        #endregion Constructors

        #region Properties

        public Func<TextLine, string[]> SplitLineFunc { get; set; }

        #endregion Properties

        #region Methods

        public override void ExtractToTarget()
        {
            var sw = Stopwatch.StartNew();
            var linesToSave = EnumerateSourceLines();

            var extractDurationMs = sw.ElapsedMilliseconds;
            var lineCount = Target.CommitData(linesToSave);

            sw.Stop();

            var commitDurationMs = sw.ElapsedMilliseconds - extractDurationMs;

            OnExtractComplete(_fileLinesRead, _linesExtracted, lineCount, extractDurationMs, commitDurationMs,
                sw.ElapsedMilliseconds);
        }

        private long _fileLinesRead;
        private long _linesExtracted;

        private IEnumerable<object[]> EnumerateSourceLines()
        {
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
        }

        #endregion Methods
    }
}