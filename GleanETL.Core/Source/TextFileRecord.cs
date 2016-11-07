namespace Glean.Core.Source
{
    using System.Collections.Generic;

    public class TextFileRecord
    {
        private readonly List<TextLine> fileLines;

        public TextFileRecord()
        {
            this.fileLines = new List<TextLine>();
        }

        public IEnumerable<TextLine> FileLines
        {
            get
            {
                return this.fileLines;
            }
        }

        public void AddFileLine(TextLine textFileLine)
        {
            this.fileLines.Add(textFileLine);
        }
    }
}