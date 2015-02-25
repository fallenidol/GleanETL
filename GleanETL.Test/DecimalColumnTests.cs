namespace GleanETL.Test
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using GleanETL.Core.Columns;
    using GleanETL.Core.Extraction;
    using GleanETL.Core.Source;
    using GleanETL.Core.Target;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DecimalColumnTests
    {
        #region Fields

        private static string _testResultsDirectoryPath = null;

        #endregion Fields

        #region Methods

        [ClassCleanup]
        public static void ClassCleanup()
        {
            var di = Directory.GetParent(_testResultsDirectoryPath);
            if (di.Exists && di.Name.Equals("TestResults", StringComparison.InvariantCultureIgnoreCase))
            {
                Directory.Delete(di.FullName, true);
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            _testResultsDirectoryPath = ctx.TestDir;
        }

        [TestMethod]
        public void TestDecimalColumn()
        {
            var columns = new BaseColumn[] { new DecimalColumn() };
            var data = new[] { "1.123", "\t1.123", "\t1.123\t", "1.123-", "-1.123", "1.123-", "-1.123", " 1.123- ", " -1.123 ", "001.123", " 1.123 ", "1.123 ", " 1.123", " 1.123", "1.123 ", "1", decimal.MaxValue.ToString(), decimal.MinValue.ToString() };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnSpace()
        {
            var columns = new BaseColumn[] { new DecimalColumn() };
            var data = new[] { "1.1 1.1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnString()
        {
            var columns = new BaseColumn[] { new DecimalColumn() };
            var data = new[] { "One" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnSymbol()
        {
            var columns = new BaseColumn[] { new DecimalColumn() };
            var data = new[] { "-" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnTooBig()
        {
            var columns = new BaseColumn[] { new DecimalColumn() };
            var data = new[] { decimal.MaxValue.ToString() + "1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnTooSmall()
        {
            var columns = new BaseColumn[] { new DecimalColumn() };
            var data = new[] { decimal.MinValue.ToString() + "1" };

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