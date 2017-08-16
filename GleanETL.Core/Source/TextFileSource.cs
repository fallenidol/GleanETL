namespace Glean.Core.Source
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    ///     A source text file.
    /// </summary>
    public class TextFileSource : IExtractSource
    {
        private readonly object enumeratorLock = new object();

        private FileInfo fileInfo;

        public TextFileSource(string pathToFile)
        {
            if (pathToFile != null)
            {
                TakeLineIf = line => true;

                FilePath = pathToFile.Trim();

                var lastBackslashIndex = pathToFile.LastIndexOf('\\') + 1;
                var filenameWithExtenstion = pathToFile.Substring(lastBackslashIndex).Trim();

                var lastPeriodIndex = filenameWithExtenstion.LastIndexOf('.');
                var filenameWithoutExtenstion = lastPeriodIndex < 0
                    ? filenameWithExtenstion.Substring(0).Trim()
                    : filenameWithExtenstion.Substring(0, lastPeriodIndex).Trim();
                var extension = lastPeriodIndex < 0
                    ? string.Empty
                    : filenameWithExtenstion.Substring(lastPeriodIndex).Trim();

                FileNameWithoutExtension = filenameWithoutExtenstion;
                FileNameWithExtension = filenameWithExtenstion;
                FileNameExtension = extension;
            }
        }

        public FileInfo FileInfo => FilePath == null ? null : fileInfo ?? (fileInfo = new FileInfo(FilePath));

        public string FileNameExtension { get; }

        public string FileNameWithExtension { get; }

        public string FileNameWithoutExtension { get; }

        public string FilePath { get; }

        public IEnumerator<TextLine> EnumerateLines()
        {
            lock (enumeratorLock)
            {
                foreach (var line in File.ReadLines(FilePath))
                {
                    if (TakeLineIf.Invoke(line))
                    {
                        yield return new TextLine(line);
                    }
                }
            }
        }

        public string DisplayName => string.Format("{0}/{1}", FileInfo.Directory.Name, FileInfo.Name);

        public Func<string, bool> TakeLineIf { get; set; }
    }
}