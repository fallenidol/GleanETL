using System;
using System.Collections.Generic;
using System.IO;

namespace Gleanio.Core.Source
{
    public interface IExtractSource
    {
        string DisplayName { get; }
        IEnumerator<TextLine> EnumerateLines();
        Func<string, bool> TakeLineIf { get; set; }
    }




    /// <summary>
    ///     A source text file.
    /// </summary>
    public class TextFileSource : IExtractSource
    {
        public string DisplayName
        {
            get
            {
                return string.Format("{0}/{1}", FileInfo.Directory.Name, FileInfo.Name);
            }
        }

        #region Constructors

        public TextFileSource(string pathToFile)
        {
            if (pathToFile != null)
            {
                TakeLineIf = line => true;

                _pathToFile = pathToFile.Trim();

                var lastBackslashIndex = pathToFile.LastIndexOf('\\') + 1;
                var filenameWithExtenstion = pathToFile.Substring(lastBackslashIndex).Trim();

                var lastPeriodIndex = filenameWithExtenstion.LastIndexOf('.');
                var filenameWithoutExtenstion = lastPeriodIndex < 0
                    ? filenameWithExtenstion.Substring(0).Trim()
                    : filenameWithExtenstion.Substring(0, lastPeriodIndex).Trim();
                var extension = lastPeriodIndex < 0
                    ? string.Empty
                    : filenameWithExtenstion.Substring(lastPeriodIndex).Trim();

                _filenameWithoutExtension = filenameWithoutExtenstion;
                _filenameWithExtension = filenameWithExtenstion;
                _filenameExtension = extension;
            }
        }

        #endregion Constructors

        #region Methods

        public IEnumerator<TextLine> EnumerateLines()
        {
            lock (_enumeratorLock)
            {
                foreach (var line in File.ReadLines(_pathToFile))
                {
                    if (TakeLineIf.Invoke(line))
                    {
                        yield return new TextLine(line);
                    }
                }
            }
        }

        #endregion Methods

        #region Fields

        private readonly object _enumeratorLock = new object();
        private readonly string _filenameExtension;
        private readonly string _filenameWithExtension;
        private readonly string _filenameWithoutExtension;
        private readonly string _pathToFile;

        #endregion Fields

        #region Properties

        private FileInfo _fileInfo;

        public FileInfo FileInfo
        {
            get { return _pathToFile == null ? null : _fileInfo ?? (_fileInfo = new FileInfo(_pathToFile)); }
        }

        public string FilePath
        {
            get { return _pathToFile; }
        }

        public string FilenameExtension
        {
            get { return _filenameExtension; }
        }

        public string FilenameWithExtension
        {
            get { return _filenameWithExtension; }
        }

        public string FilenameWithoutExtension
        {
            get { return _filenameWithoutExtension; }
        }

        public Func<string, bool> TakeLineIf { get; set; }

        #endregion Properties
    }
}