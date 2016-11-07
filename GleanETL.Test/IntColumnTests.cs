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
    public class IntColumnTests
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
        public void TestIntColumn()
        {
            var columns = new BaseColumn[] { new IntColumn() };
            var data = new[] { "1", "\t1", "\t1\t", "1-", "-1", "1.0-", "-1.0", " 1- ", " -1 ", "001", "1.0", " 1 ", "1 ", " 1", " 1.000", "1.000 ", int.MaxValue.ToString() };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true) { SplitLineFunc = line => line.OriginalLine.Split(',') };

            extraction.DataParseError += this.DataParseError;
            extraction.ExtractComplete += this.ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestIntColumnDecimal()
        {
            var columns = new BaseColumn[] { new IntColumn() };
            var data = new[] { "1.1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true) { SplitLineFunc = line => line.OriginalLine.Split(',') };

            extraction.DataParseError += this.DataParseError;
            extraction.ExtractComplete += this.ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestIntColumnSpace()
        {
            var columns = new BaseColumn[] { new IntColumn() };
            var data = new[] { "1 1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true) { SplitLineFunc = line => line.OriginalLine.Split(',') };

            extraction.DataParseError += this.DataParseError;
            extraction.ExtractComplete += this.ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestIntColumnString()
        {
            var columns = new BaseColumn[] { new IntColumn() };
            var data = new[] { "One" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true) { SplitLineFunc = line => line.OriginalLine.Split(',') };

            extraction.DataParseError += this.DataParseError;
            extraction.ExtractComplete += this.ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestIntColumnSymbol()
        {
            var columns = new BaseColumn[] { new IntColumn() };
            var data = new[] { "-" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true) { SplitLineFunc = line => line.OriginalLine.Split(',') };

            extraction.DataParseError += this.DataParseError;
            extraction.ExtractComplete += this.ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestIntColumnTooBig()
        {
            var columns = new BaseColumn[] { new IntColumn() };
            var data = new[] { (int.MaxValue + 1L).ToString() };

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