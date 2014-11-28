namespace Gleanio.Core.Extraction
{
    using Gleanio.Core.Columns;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;
    using System;
    using System.Collections.Generic;

    public class ExtractLinesToDatabase : LineExtraction<DatabaseTableTarget>
    {
        #region Constructors

        public ExtractLinesToDatabase(BaseColumn[] columns, TextFile source, DatabaseTableTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }

    public class ExtractLinesToSeparatedValueFile : LineExtraction<SeparatedValueFileTarget>
    {
        #region Constructors

        public ExtractLinesToSeparatedValueFile(BaseColumn[] columns, TextFile source, SeparatedValueFileTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }

    public class ExtractLinesToTrace : LineExtraction<TraceOutputTarget>
    {
        #region Constructors

        public ExtractLinesToTrace(BaseColumn[] columns, TextFile source, TraceOutputTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }

    public class LineExtraction<TExtractTarget> : Extract<TExtractTarget>
        where TExtractTarget : BaseExtractTarget
    {
        #region Constructors

        protected LineExtraction(BaseColumn[] columns, TextFile source, TExtractTarget target)
            : base(columns, source, target)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] { ',' }, StringSplitOptions.None);
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, string[]> SplitLineFunc
        {
            get; set;
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

        public override void ExtractToTarget()
        {
            //var targetFileLines = new List<object[]>();

            //int lineCount = Source.LinesToImport.Count();

            //Source.LinesToImport.ForEach((i, line) =>
            //{
            //    var rawLineValues = SplitLineFunc(line);
            //    var parsedLineValues = ParseStringValues(rawLineValues);

            //    if (!parsedLineValues.IsNullOrEmpty())
            //    {
            //        //targetFileLines.Add(parsedLineValues);
            //    }

            //    //int percent = (int)(((i+1) / (double)lineCount) * 100d);
            //    //OnProgressChanged(percent);
            //});

            //Target.SaveRows(targetFileLines.ToArray());

            var linesToSave = EnumerateSourceLines();

            Target.CommitData(linesToSave);

            //Debug.WriteLine("*** " + Source.Name.ToUpperInvariant() + " FINISHED. " + targetFileLines.Count + " LINES SAVED!!");
        }

        private IEnumerable<object[]> EnumerateSourceLines()
        {
            foreach (var line in Source.EnumerateFileLines())
            {
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