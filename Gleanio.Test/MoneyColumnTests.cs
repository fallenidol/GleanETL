﻿namespace Gleanio.Test
{
    using System.Diagnostics;

    using Gleanio.Core.Columns;
    using Gleanio.Core.Extraction;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MoneyColumnTests
    {
        #region Methods

        [TestMethod]
        public void TestMoneyColumn()
        {
            var columns = new BaseColumn[] { new MoneyColumn() };
            var data = new[] { "$10.00", "¥10.00", "£10.00", "€10.00", "10.00", " $10.00 ", " ¥10.00 ", " £10.00 ", " €10.00 ", "1.123", "\t1.123", "\t1.123\t", "1.123-", "-1.123", "1.123-", "-1.123", " 1.123- ", " -1.123 ", "001.123", " 1.123 ", "1.123 ", " 1.123", " 1.123", "1.123 ", "1", decimal.MaxValue.ToString(), decimal.MinValue.ToString() };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractToTarget();
        }

        // "¥10 .00", 
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnTooBig()
        {
            var columns = new BaseColumn[] { new MoneyColumn() };
            var data = new[] { "$" + decimal.MaxValue.ToString() + "1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnTooSmall()
        {
            var columns = new BaseColumn[] { new MoneyColumn() };
            var data = new[] { decimal.MinValue.ToString() + "1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnSpace()
        {
            var columns = new BaseColumn[] { new MoneyColumn() };
            var data = new[] { "$1. 1" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnString()
        {
            var columns = new BaseColumn[] { new MoneyColumn() };
            var data = new[] { "One" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void TestDecimalColumnSymbol()
        {
            var columns = new BaseColumn[] { new MoneyColumn() };
            var data = new[] { "$" };

            var source = new MemorySource("Data", data);
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, new TraceOutputTarget(), throwParseErrors: true)
            {
                SplitLineFunc = line => line.OriginalLine.Split(',')
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractToTarget();
        }

        private void DataParseError(object sender, Core.EventArgs.ParseErrorEventArgs e)
        {
            Trace.WriteLine(string.Format("PARSE ERROR: {0}, {1}", e.ValueBeingParsed ?? string.Empty, e.Message));
        }

        #endregion Methods
    }
}