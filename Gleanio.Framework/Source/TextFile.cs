using System;
using System.Collections.Generic;
using System.Linq;
using gleanio.framework.Columns;

namespace gleanio.framework.Source
{
    public class TextFile
    {
        #region Fields

        private readonly System.IO.FileInfo _fileInfo;

        private IEnumerable<TextFileLine> _allLines;
        private Func<TextFileLine, bool> _keepLineFunc = line => true;

        #endregion Fields

        #region Constructors

        public TextFile(string pathToFile)
        {
            if (System.IO.File.Exists(pathToFile))
            {
                _fileInfo = new System.IO.FileInfo(pathToFile);
            }
        }

        #endregion Constructors

        #region Properties

        public long ByteCount
        {
            get { return _fileInfo.Length; }
        }

        public BaseColumn[] Columns
        {
            get; set;
        }

        public string Extension
        {
            get { return _fileInfo.Extension; }
        }

        public string FullName
        {
            get { return _fileInfo.FullName; }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return _fileInfo.LastWriteTimeUtc; }
        }

        public long LineCount
        {
            get
            {
                return _allLines.Count();
            }
        }

        public IEnumerable<TextFileLine> LinesToImport
        {
            get
            {
                return from l in _allLines
                       where _keepLineFunc(l)
                       select l;
            }
        }

        public string Name
        {
            get { return _fileInfo.Name; }
        }

        public string NameWithoutExtension
        {
            get { return _fileInfo.Name.Replace(_fileInfo.Extension, string.Empty); }
        }

        #endregion Properties

        #region Methods

        public void Parse(Func<TextFileLine, bool> keepLineFunc)
        {
            _keepLineFunc = keepLineFunc;

            _allLines = System.IO.File.ReadLines(_fileInfo.FullName).Select((l, idx) => new TextFileLine(idx + 1, l));

            //Debug.WriteLine("{0}: Line Count: Total={1}, To Keep={2}", Name, LineCount, LinesToImport.Count());
        }

        #endregion Methods
    }
}