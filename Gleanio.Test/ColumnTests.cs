using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gleanio.Core.Columns;
using Gleanio.Core.Extraction;
using Gleanio.Core.Source;
using Gleanio.Core.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gleanio.Test
{
    public class MemorySource : IExtractSource
    {
        public MemorySource(string displayName, string[] data)
        {
            TakeLineIf = line => true;

            DisplayName = displayName;
            Data = data;
        }

        public string DisplayName
        {
            get;
            private set;
        }
        public Func<string, bool> TakeLineIf { get; set; }

        private string[] Data { get; set; }

        public IEnumerator<TextLine> EnumerateLines()
        {
            foreach (var line in Data)
            {
                if (TakeLineIf.Invoke(line))
                {
                    yield return new TextLine(line);
                }
            }
        }
    }

    [TestClass]
    public class ColumnTests
    {

        [TestMethod]
        public void TestIntColumn()
        {
            var columns = new BaseColumn[]
                {
                    new StringColumn(),
                    new IntColumn()
                };

            var data = new[] {
                //"[19283746918273461982374],19283746918273461982374",
                "[1-],1-",
                "[-1],-1",
                "[1],1",
                "[1.0],1.0",
                //"[1.1],1.1",
                //"[1.000000001],1.000000001",
                "[ 1 ], 1 ",
                "[1 ],1 ",
                "[ 1], 1",
                "[ 1.000], 1.000"
            };

            var source = new MemorySource("ints", data);

            var target = new TraceOutputTarget();
            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, target, throwParseErrors: true)
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

    }
}
