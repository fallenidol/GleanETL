namespace Gleanio.Core.Source
{
    using System;

    public class TextFileLine
    {
        #region Fields

        private readonly string _delimiter;
        private readonly long _lineNumber;
        private readonly string _originalLine;

        private string _lowerCaseLine;
        private string _trimmedLine;
        private string _trimmedLowerCaseLine;

        #endregion Fields

        #region Constructors

        public TextFileLine(long lineNumber, string originalLineText, string delimiter = null)
            : this(lineNumber, originalLineText)
        {
            _delimiter = delimiter;
        }

        public TextFileLine(long lineNumber, string originalLineText)
        {
            _lineNumber = lineNumber;
            _originalLine = originalLineText;
        }

        #endregion Constructors

        #region Properties

        public string Delimiter
        {
            get { return _delimiter; }
        }

        public int LineLength
        {
            get { return _originalLine.Length; }
        }

        public long LineNumber
        {
            get { return _lineNumber; }
        }

        public string LowerCaseLine
        {
            get { return _lowerCaseLine ?? (_lowerCaseLine = _originalLine.ToLowerInvariant()); }
        }

        public string OriginalLine
        {
            get { return _originalLine; }
        }

        public string TrimmedLine
        {
            get { return _trimmedLine ?? (_trimmedLine = _originalLine.Trim()); }
        }

        public int TrimmedLineLength
        {
            get { return TrimmedLine.Length; }
        }

        public string TrimmedLowerCaseLine
        {
            get { return _trimmedLowerCaseLine ?? (_trimmedLowerCaseLine = TrimmedLine.ToLowerInvariant()); }
        }

        #endregion Properties

        #region Methods

        public string[] Split(params int[] columnStartIndexes)
        {
            string[] values = new string[columnStartIndexes.Length];

            for (int i = 0; i < columnStartIndexes.Length; i++)
            {
                int startPos = columnStartIndexes[i];
                int nextIdx = i + 1;

                if (nextIdx < columnStartIndexes.Length)
                {
                    int nextPos = columnStartIndexes[nextIdx];
                    int length = nextPos - startPos;

                    if (length < (LineLength - startPos))
                    {
                        values[i] = OriginalLine.Substring(startPos, length);
                    }
                    else
                    {
                        values[i] = OriginalLine.Substring(startPos);
                        break;
                    }
                }
                else
                {
                    values[i] = OriginalLine.Substring(startPos);
                    break;
                }
            }

            return values;
        }

        public string SplitAndGetString(int index, char delimiter)
        {
            return OriginalLine.Split(new[] { delimiter }, StringSplitOptions.None)[index];
        }

        public override string ToString()
        {
            return _originalLine;
        }

        #endregion Methods
    }
}