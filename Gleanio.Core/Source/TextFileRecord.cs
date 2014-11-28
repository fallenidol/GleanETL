namespace Gleanio.Core.Source
{
    using System.Collections.Generic;

    public class TextFileRecord
    {
        #region Fields

        private readonly List<TextFileLine> _fileLines;

        #endregion Fields

        #region Constructors

        public TextFileRecord()
        {
            _fileLines = new List<TextFileLine>();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<TextFileLine> FileLines
        {
            get { return _fileLines; }
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