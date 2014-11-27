using System.Linq;

namespace Gleanio.Core.Extraction
{
    using Gleanio.Core.Columns;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    public class LineExtraction<TExtractTarget> : Extract<TExtractTarget> where TExtractTarget : BaseExtractTarget
    {
        #region Constructors

        public LineExtraction(BaseColumn[] columns, TextFile source, TExtractTarget target)
            : base(columns, source, target)
        {
            SplitLineFunc = line => line.OriginalLine.Split(new[] { ',' }, StringSplitOptions.None);

            
        }

        #endregion Constructors

        #region Properties

        public Func<TextFileLine, string[]> SplitLineFunc { get; set; }

        #endregion Properties

        #region Methods

       

        public override void ExtractToTarget()
        {
            var targetFileLines = new List<object[]>();

            int lineCount = Source.LinesToImport.Count();

            Source.LinesToImport.ForEach((i, line) =>
            {
                var rawLineValues = SplitLineFunc(line);
                var parsedLineValues = ParseStringValues(rawLineValues);

                if (parsedLineValues != null && parsedLineValues.Length > 0)
                {
                    targetFileLines.Add(parsedLineValues);
                }

                int percent = (i * 200 + lineCount) / (lineCount * 2);
                OnProgressChanged(percent);
            });

            Target.SaveRows(targetFileLines.ToArray());

            Debug.WriteLine("*** " + Source.Name.ToUpperInvariant() + " FINISHED. " + targetFileLines.Count + " LINES SAVED!!");
        }

        #endregion Methods
    }
}