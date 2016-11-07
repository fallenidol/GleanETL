namespace Glean.Test
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Glean.Core.Columns;
    using Glean.Core.EventArgs;
    using Glean.Core.Extraction;
    using Glean.Core.Source;
    using Glean.Core.Target;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IgnoredColumnTests
    {
        private static string testResultsDirectoryPath;

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var di = Directory.GetParent(testResultsDirectoryPath);
            if (di.Exists && di.Name.Equals("TestResults", StringComparison.InvariantCultureIgnoreCase))
            {
                Directory.Delete(di.FullName, true);
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            testResultsDirectoryPath = ctx.TestDir;
        }

        [TestMethod]
        public void TestIgnoredColumn()
        {
            var columns = new BaseColumn[] { new StringColumn(), new IgnoredColumn(), new StringColumn(), new IgnoredColumn(), new StringColumn() };
            var data = new[] { "r1c1,r1c2,r1c3,r1c4,r1c5", "r2c1,r2c2,r2c3,r2c4,r2c5" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true) { SplitLineFunc = line => line.OriginalLine.Split(',') };

            extraction.DataParseError += this.DataParseError;
            extraction.ExtractComplete += this.ExtractComplete;
            extraction.ExtractToTarget();
        }

        private void DataParseError(object sender, ParseErrorEventArgs e)
        {
            Trace.WriteLine(string.Format("PARSE ERROR: {0}, {1}", e.ValueBeingParsed ?? string.Empty, e.Message));
        }

        private void ExtractComplete(object sender, ExtractCompleteArgs e)
        {
            Trace.WriteLine(string.Format("EXTRACTION COMPLETE!!: {0}", e));
        }
    }
}