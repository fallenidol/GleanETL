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
    public class DateColumnTests
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
        public void TestDateColumn()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[]
            {
                "2015-01-01", "2015-12-31", "3015-01-01", "1015-01-01", "0001-01-01", "2015-01-01 ", " 2015-01-01 ",
                "2015-01-01", "2015-1-1", "01/01/2015", "1/1/2015",
                "31122015", "31/1/15", "1/1/15", "2015-1-1", "2015-12-1", "2015-1-31"
            };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDateColumnInvalid1()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[] { "2015-01" };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDateColumnInvalid2()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[] { "12th Jan" };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDateColumnInvalid3()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[] { "12/31/2015" };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDateColumnInvalid4()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[] { "0000-01-01" };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDateColumnInvalid5()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[] { "2015-01-01a" };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDateColumnNonExistant()
        {
            var columns = new BaseColumn[] { new DateColumn() };
            var data = new[] { "2015-02-31" };

            var source = new MemorySource("Data", data);
            var extraction =
                new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), true)
                {
                    SplitLineFunc = line => line.OriginalLine.Split(',')
                };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
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