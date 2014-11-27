namespace Gleanio.Framework.Source
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Gleanio.Framework.Columns;

    public class TextFile
    {
        #region Fields

        private readonly System.IO.FileInfo _fileInfo;

        private IEnumerable<TextFileLine> _allLines;

        #endregion Fields

        #region Constructors

        public TextFile(string pathToFile)
        {
            TakeLineFunc = line => true;

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
                if (_allLines == null)
                {
                    _allLines = System.IO.File.ReadLines(_fileInfo.FullName).Select((l, idx) => new TextFileLine(idx + 1, l));

                    //Debug.WriteLine("{0}: Line Count: Total={1}, To Keep={2}", Name, LineCount, LinesToImport.Count());
                }

                return (from l in _allLines
                    where TakeLineFunc(l)
                    select l).AsParallel();
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

        public Func<TextFileLine, bool> TakeLineFunc
        {
            get; set;
        }

        #endregion Properties
    }
}