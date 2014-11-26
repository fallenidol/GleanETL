using System.Collections.Generic;

namespace gleanio.framework.Source
{
    public class TextFileRecord
    {
        #region Fields

        private readonly List<TextFileLine> _fileLines;

        #endregion Fields

        #region Constructors

        public TextFileRecord(int recordNumber)
        {
            RecordNumber = recordNumber;
            _fileLines = new List<TextFileLine>();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TextFileLine> FileLines
        {
            get { return _fileLines; }
        }

        public long RecordNumber
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public void AddFileLine(TextFileLine textFileLine)
        {
            _fileLines.Add(textFileLine);
        }

        #endregion Methods
    }
}