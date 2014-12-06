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

        protected LineExtraction(BaseColumn[] columns, TextFile source, TExtractTarget target)
            : base(columns, source, target)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] {','}, StringSplitOptions.None);
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, string[]> SplitLineFunc { get; set; }

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
            var enumerator = Source.EnumerateFileLines();
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