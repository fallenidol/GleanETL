namespace Gleanio.Core.Source
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class TextFile
    {
        #region Fields

        private readonly object _enumeratorLock = new object();
        private readonly string _filenameExtension;
        private readonly string _filenameWithExtension;
        private readonly string _filenameWithoutExtension;
        private readonly string _pathToFile;

        private bool _alreadyEnumerated;

        #endregion Fields

        #region Constructors

        public TextFile(string pathToFile)
        {
            TakeLineFunc = line => true;

            _pathToFile = pathToFile.Trim();

            int lastBackslashIndex = pathToFile.LastIndexOf('\\') + 1;
            string filenameWithExtenstion = pathToFile.Substring(lastBackslashIndex).Trim();

            int lastPeriodIndex = filenameWithExtenstion.LastIndexOf('.');
            string filenameWithoutExtenstion = lastPeriodIndex < 0 ? filenameWithExtenstion.Substring(0).Trim() : filenameWithExtenstion.Substring(0, lastPeriodIndex).Trim();
            string extension = lastPeriodIndex < 0 ? string.Empty : filenameWithExtenstion.Substring(lastPeriodIndex).Trim();

            _filenameWithoutExtension = filenameWithoutExtenstion;
            _filenameWithExtension = filenameWithExtenstion;
            _filenameExtension = extension;
        }

        #endregion Constructors

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

        public Func<string, bool> TakeLineFunc
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public IEnumerator<TextFileLine> EnumerateFileLines()
        //public IEnumerable<TextFileLine> EnumerateFileLines()
        {
            if (_alreadyEnumerated) throw new InvalidProgramException("Already enumerated!!!");

            lock (_enumeratorLock)
            {
                _alreadyEnumerated = true;

                foreach (var line in System.IO.File.ReadLines(_pathToFile))
                {
                    if (TakeLineFunc.Invoke(line))
                    {
                        yield return new TextFileLine(line);
                    }
                }
            }
        }

        #endregion Methods
    }
}