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

        public override void AfterExtract()
        {
            throw new NotImplementedException();
        }

        public override void BeforeExtract()
        {
            throw new NotImplementedException();
        }

        public override void ExtractToTarget()
        {
            var linesToSave = EnumerateSourceLines();

            Target.CommitData(linesToSave);

            Trace.WriteLine(string.Format("{0} finished.", Source.FilenameWithExtension));
        }

        private IEnumerable<object[]> EnumerateSourceLines()
        {
            var enumerator = Source.EnumerateFileLines();
            while (enumerator.MoveNext())
            {
                var line = enumerator.Current;
                var rawLineValues = SplitLineFunc(line);

                var parsedLineValues = ParseStringValues(rawLineValues);
                if (!parsedLineValues.IsNullOrEmpty())
                {
                    yield return parsedLineValues;
                }
            }
        }

        #endregion Methods
    }
}