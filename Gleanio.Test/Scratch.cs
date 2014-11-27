using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gleanio.Core.Columns;
using Gleanio.Core.Extraction;
using Gleanio.Core.Source;
using Gleanio.Core.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gleanio.Test
{
    [TestClass]
    public class Scratch
    {
        [TestMethod]
        public void ExtractHostsFileToTraceOutput()
        {
            var columns = new BaseColumn[]
            {
                new StringColumn("IP", 12),
                new StringColumn("NAME", 250)
            };

            var source = new TextFile(@"C:\Windows\System32\drivers\etc\HOSTS")
            {
                TakeLineFunc = line => !line.OriginalLine.Contains("#")
            };

            var target = new TraceOutputTarget();


            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, target)
            {
                SplitLineFunc = line => line.OriginalLine.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries)
            };

            extraction.ExtractToTarget();
        }
    }
}
