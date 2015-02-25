namespace GleanETL.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using GleanETL.Core.Columns;
    using GleanETL.Core.Extraction;
    using GleanETL.Core.Source;
    using GleanETL.Core.Target;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IgnoredColumnTests
    {
        #region Methods

        [TestMethod]
        public void TestIgnoredColumn()
        {
            var columns = new BaseColumn[] { new StringColumn(), new IgnoredColumn(), new StringColumn(), new IgnoredColumn(), new StringColumn() };
            var data = new[] { "r1c1,r1c2,r1c3,r1c4,r1c5", "r2c1,r2c2,r2c3,r2c4,r2c5" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        private void DataParseError(object sender, Core.EventArgs.ParseErrorEventArgs e)
        {
            Trace.WriteLine(string.Format("PARSE ERROR: {0}, {1}", e.ValueBeingParsed ?? string.Empty, e.Message));
        }

        private void ExtractComplete(object sender, Core.EventArgs.ExtractCompleteArgs e)
        {
            Trace.WriteLine(string.Format("EXTRACTION COMPLETE!!: {0}", e.ToString()));
        }

        #endregion Methods
    }
}