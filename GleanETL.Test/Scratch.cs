﻿namespace Glean.Test
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using Glean.Core;
    using Glean.Core.Columns;
    using Glean.Core.Enumerations;
    using Glean.Core.EventArgs;
    using Glean.Core.Extraction;
    using Glean.Core.Source;
    using Glean.Core.Target;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Scratch
    {
        private const string LocalDbConnectionString =
            @"Server=(localDB)\MSSQLLocalDB;Integrated Security=true;Initial Catalog=GleanETL;";

        private static readonly string DatabaseConnectionString = LocalDbConnectionString;

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
        public void ExtractHostsFileToDatabase()
        {
            const string WindowsUpdateLogFilePath = @"C:\Windows\System32\drivers\etc\HOSTS";

            if (!File.Exists(WindowsUpdateLogFilePath))
            {
                Assert.Fail("The Windows Update log file does not exist!");
            }
            else
            {
                var source = new TextFileSource(WindowsUpdateLogFilePath)
                {
                    TakeLineIf = line => line.Length > 0 && (!line.Contains("#") || line.IndexOf('#') > 0)
                };

                var target = new DatabaseTableTarget(DatabaseConnectionString, "Hosts", deleteTableIfExists: true);

                var columns = new[]
                {
                    new StringColumn("IpAddress"),
                    new StringColumn("Hostname", stringCapitalisation: StringCapitalisation.ToLowercase)
                };

                var extraction = new LineExtraction<DatabaseTableTarget>(columns, source, target, true)
                {
                    SplitLineFunc =
                        line =>
                            line.OriginalLine.TrimAndRemoveConsecutiveWhiteSpace()
                                .Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries)
                                .ForEachAssign((i, s) => i > 0 && s.Contains("#") ? s.Substring(0, s.IndexOf('#')) : s)
                };

                extraction.DataParseError += DataParseError;
                extraction.ExtractComplete += ExtractComplete;
                extraction.ExtractToTarget();
            }
        }

        [TestMethod]
        public void ExtractHostsFileToTraceOutput()
        {
            var columns = new[]
            {
                new StringColumn("IP", 16),
                new StringColumn("HOST", 250, stringCapitalisation: StringCapitalisation.ToLowercase)
            };

            var source = new TextFileSource(@"C:\Windows\System32\drivers\etc\HOSTS")
            {
                TakeLineIf = line => line.Length > 0 && (!line.Contains("#") || line.IndexOf('#') > 0)
            };

            var target = new TraceOutputTarget();

            var extraction = new LineExtraction<TraceOutputTarget>(columns, source, target, true)
            {
                SplitLineFunc =
                    line =>
                        line.OriginalLine.TrimAndRemoveConsecutiveWhiteSpace()
                            .Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries)
                            .ForEachAssign((i, s) => i > 0 && s.Contains("#") ? s.Substring(0, s.IndexOf('#')) : s)
            };

            extraction.DataParseError += DataParseError;
            extraction.ExtractComplete += ExtractComplete;
            extraction.ExtractToTarget();
        }

        [TestMethod]
        public void ExtractWindowsUpdateLogFileToDatabase()
        {
            const string WindowsUpdateLog = @"C:\Windows\WindowsUpdate.log";

            if (File.Exists(WindowsUpdateLog))
            {
                var sourceFile = Path.GetTempFileName();
                File.Copy(WindowsUpdateLog, sourceFile, true);

                var columns = new BaseColumn[]
                {
                    new DateColumn("Timestamp", new[] { "yyyy-MM-dd HH:mm:ss:fff" }, "yyyy-MM-dd HH:mm:ss.fff"),
                    new IgnoredColumn(), new IgnoredColumn(),
                    new StringColumn("Message")
                };

                var source = new TextFileSource(sourceFile) { TakeLineIf = line => line.Length > 0 };

                var target = new DatabaseTableTarget(DatabaseConnectionString, "WindowsUpdateLog",
                    deleteTableIfExists: true);

                var extraction =
                    new LineExtraction<DatabaseTableTarget>(columns, source, target, true)
                    {
                        SplitLineFunc = line => line.Split(0, 23, 28, 33)
                    };

                extraction.DataParseError += DataParseError;
                extraction.ExtractComplete += ExtractComplete;
                extraction.ExtractToTarget();

                if (File.Exists(sourceFile))
                {
                    File.Delete(sourceFile);
                }
            }
            else
            {
                Assert.Inconclusive("File could not be found: {0}.", WindowsUpdateLog);
            }
        }

        [TestInitialize]
        public void TestInit()
        {
            var csb = new SqlConnectionStringBuilder(DatabaseConnectionString);
            csb.InitialCatalog = "master";

            using (var c = new SqlConnection(csb.ConnectionString))
            {
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
            IF NOT EXISTS (
            SELECT 1
            FROM sys.databases
            WHERE [name] = 'GleanETL'
            )
            CREATE DATABASE [GleanETL];
            ";

                    c.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DataParseError(object sender, ParseErrorEventArgs e)
        {
            Trace.WriteLine(string.Format("PARSE ERROR: {0}, {1}", e.ValueBeingParsed ?? string.Empty, e.Message));
        }

        private void ExtractComplete(object sender, ExtractCompleteArgs e)
        {
            Trace.WriteLine(string.Format("EXTRACTION COMPLETE!!: {0}", e));
        }

        //[TestMethod]
        //public void GenerateBigFile()
        //{
        //    string up = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"GleanETL\");
        //    string tf = Path.Combine(up, @"rnd_big_file.txt");
        //    if (!File.Exists(tf))
        //    {
        //        Directory.CreateDirectory(up);
        //        using (File.Create(tf)) { }
        //        var sb = new StringBuilder();

        //        int bufferedLines = 0;
        //        for (int i = 0; i < 3000000; i++)
        //        {
        //            var rnd = new Random(i);
        //            int day = rnd.Next(1, 27);
        //            int month = rnd.Next(1, 12);
        //            int year = rnd.Next(1950, 2014);
        //            int hr = rnd.Next(0, 23);
        //            int mm = rnd.Next(0, 59);
        //            int ss = rnd.Next(0, 59);
        //            var dt = new DateTime(year, month, day, hr, mm, ss);
        //            sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}", dt.ToString("dd/MM/yyyy"), dt.ToString("HH:mm:ss"), new Random(i).Next(1000, 9999), Guid.NewGuid()));
        //            bufferedLines++;
        //            if (bufferedLines == 50000)
        //            {
        //                File.AppendAllText(tf, sb.ToString());
        //                sb.Clear();
        //                bufferedLines = 0;
        //            }
        //        }
        //    }
        //}
    }
}