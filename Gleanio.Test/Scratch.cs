namespace Gleanio.Test
{
    using Gleanio.Core;
    using Gleanio.Core.Columns;
    using Gleanio.Core.Extraction;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;

    [TestClass]
    public class Scratch
    {
        #region Methods

        [TestMethod]
        public void ExtractHostsFileToTraceOutput()
        {
            var columns = new BaseColumn[]
            {
                new StringColumn("IP", 16),
                new StringColumn("HOST", 250)
            };

            var source = new TextFile(@"C:\Windows\System32\drivers\etc\HOSTS")
            {
                TakeLineFunc = line =>
                    line.Length > 0 &&
                    !line.Contains("0.0.0.0") &&
                    !line.ContainsCaseInsensitive("localhost") &&
                    (!line.Contains("#") || line.IndexOf('#') > 0)
            };

            var target = new TraceOutputTarget();

            var extraction = new ExtractLinesToTrace(columns, source, target)
            {
                SplitLineFunc = line => line.OriginalLine.TrimAndRemoveConsecutiveWhitespace().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).ForEachAssign((i, s) => (i > 0 && s.Contains("#")) ? s.Substring(0, s.IndexOf('#')) : s)
            };

            //extraction.ProgressChanged += (sender, args) => Trace.WriteLine("Percent Complete " + args.ProgressPercentage + "%");

            extraction.ExtractToTarget();
        }

        [TestMethod]
        public void GLTrans()
        {
            var columns = new BaseColumn[]
            {
                new StringColumn("ACCOUNT", 4)
            };

            var source = new TextFile(@"C:\Users\paul.mcilreavy\Dropbox\Development\Code\Gleanio\Gleanio.Test\Files\GLTRANS.txt")
            {
                TakeLineFunc = line => line.IsNumber(0, 4)
            };

            var target = new TraceOutputTarget();

            var extraction = new ExtractLinesToTrace(columns, source, target)
            {
                SplitLineFunc = line => new[] { line.OriginalLine.Substring(0, 4) }
            };

            var sw = Stopwatch.StartNew();

            extraction.ExtractToTarget();

            sw.Stop();
            Trace.WriteLine(sw.ElapsedMilliseconds + "ms");
        }

        #endregion Methods
    }
}