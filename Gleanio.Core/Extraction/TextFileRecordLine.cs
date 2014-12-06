namespace Gleanio.Core.Extraction
{
    public class TextFileRecordLine
    {
        public string Text { get; private set; }
        public string Delimiter { get; private set; }

        public static TextFileRecordLine New(string lineText, string columnDelimiter)
        {
            return new TextFileRecordLine {Text = lineText, Delimiter = columnDelimiter};
        }
    }
}