using System.Collections.Generic;

namespace GleanETL.Core.Source
{
    public class TextFileRecord
    {
        #region Fields

        private readonly List<TextLine> _fileLines;

        #endregion Fields

        #region Constructors

        public TextFileRecord()
        {
            _fileLines = new List<TextLine>();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TextLine> FileLines
        {
            get { return _fileLines; }
        }

        #endregion Properties

        #region Methods

        public void AddFileLine(TextLine textFileLine)
        {
            _fileLines.Add(textFileLine);
        }

        #endregion Methods
    }
}