namespace Gleanio.Test
{
    using System;
    using System.IO;
    using System.Text;

    using Gleanio.Core;
    using Gleanio.Core.Columns;
    using Gleanio.Core.Extraction;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Scratch
    {
        #region Methods

        [TestMethod]
        public void ExtractWindowsUpdateLogFileToDatabase()
        {
            var columns = new BaseColumn[]
            {
                new DateColumn("Timestamp", new []{"yyyy-MM-dd HH:mm:ss:fff"}, "yyyy-MM-dd HH:mm:ss.fff"), 
                new IntColumn("Unknown1"), 
                new StringColumn("Unknown2"), 
                new StringColumn("Message", 250)
            };

            var source = new TextFile(@"C:\Windows\WindowsUpdate.log")
            {
                TakeLineFunc = line => line.Length > 0
            };

            var target = new DatabaseTableTarget(@"Server=(localdb)\v11.0;Integrated Security=true;Initial Catalog=Gleanio;", "WindowsUpdateLog");

            var extraction = new ExtractLinesToDatabase(columns, source, target)
            {
                SplitLineFunc = line => line.Split(0, 23, 28, 33)
            };

            extraction.ExtractToTarget();
        }

        [TestMethod]
        public void ExtractHostsFileToDatabase()
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
                    (!line.Contains("#") || line.IndexOf('#') > 0)
            };

            var target = new DatabaseTableTarget(@"Server=(localdb)\v11.0;Integrated Security=true;Initial Catalog=Gleanio;", "Hosts");

            var extraction = new ExtractLinesToDatabase(columns, source, target)
            {
                SplitLineFunc = line => line.OriginalLine.TrimAndRemoveConsecutiveWhitespace().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).ForEachAssign((i, s) => (i > 0 && s.Contains("#")) ? s.Substring(0, s.IndexOf('#')) : s)
            };

            extraction.ExtractToTarget();
        }

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
                    (!line.Contains("#") || line.IndexOf('#') > 0)
            };

            var target = new TraceOutputTarget();

            var extraction = new ExtractLinesToTrace(columns, source, target)
            {
                SplitLineFunc = line => line.OriginalLine.TrimAndRemoveConsecutiveWhitespace().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).ForEachAssign((i, s) => (i > 0 && s.Contains("#")) ? s.Substring(0, s.IndexOf('#')) : s)
            };

            extraction.ExtractToTarget();
        }

        [TestMethod]
        public void GenerateBigFile()
        {
            string up = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Gleanio\");
            string tf = Path.Combine(up, @"rnd_big_file.txt");

            if (!File.Exists(tf))
            {
                Directory.CreateDirectory(up);
                using (File.Create(tf)) { }

                var sb = new StringBuilder();
                int bufferedLines = 0;

                for (int i = 0; i < 3000000; i++)
                {
                    var rnd = new Random(i);

                    int day = rnd.Next(1, 27);
                    int month = rnd.Next(1, 12);
                    int year = rnd.Next(1950, 2014);

                    int hr = rnd.Next(0, 23);
                    int mm = rnd.Next(0, 59);
                    int ss = rnd.Next(0, 59);

                    var dt = new DateTime(year, month, day, hr, mm, ss);

                    sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}", dt.ToString("dd/MM/yyyy"), dt.ToString("HH:mm:ss"), new Random(i).Next(1000, 9999), Guid.NewGuid()));
                    bufferedLines++;

                    if (bufferedLines == 50000)
                    {
                        File.AppendAllText(tf, sb.ToString());
                        sb.Clear();
                        bufferedLines = 0;
                    }
                }
            }
        }

        #endregion Methods
    }
}