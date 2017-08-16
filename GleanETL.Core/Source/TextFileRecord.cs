namespace Glean.Core.Source
{
    using System.Collections.Generic;

    public class TextFileRecord
    {
        private readonly List<TextLine> fileLines;

        public TextFileRecord()
        {
            fileLines = new List<TextLine>();
        }

        public IEnumerable<TextLine> FileLines => fileLines;

        public void AddFileLine(TextLine textFileLine)
        {
            fileLines.Add(textFileLine);
        }
    }
}