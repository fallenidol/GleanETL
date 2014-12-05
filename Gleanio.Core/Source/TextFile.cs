using System;
using System.Collections.Generic;
using System.IO;

namespace Gleanio.Core.Source
{
    /// <summary>
    /// </summary>
    public class TextFile
    {
        #region Constructors

        public TextFile(string pathToFile)
        {
            TakeLineFunc = line => true;

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

        #endregion Constructors

        #region Methods

        public IEnumerator<TextFileLine> EnumerateFileLines()
        {
            lock (_enumeratorLock)
            {
                foreach (var line in File.ReadLines(_pathToFile))
                {
                    if (TakeLineFunc.Invoke(line))
                    {
                        yield return new TextFileLine(line);
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

        public Func<string, bool> TakeLineFunc { get; set; }

        #endregion Properties
    }
}