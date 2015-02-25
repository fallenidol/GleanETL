using System;

namespace GleanETL.Core.Source
{
    public class TextLine
    {
        #region Fields

        private readonly string _originalLine;

        #endregion Fields

        #region Constructors

        public TextLine(string originalLineText)
        {
            _originalLine = originalLineText;
        }

        #endregion Constructors

        #region Properties

        public int LineLength
        {
            get { return _originalLine.Length; }
        }

        public string OriginalLine
        {
            get { return _originalLine; }
        }

        #endregion Properties

        #region Methods

        public string[] Split(params int[] columnStartIndexes)
        {
            var values = new string[columnStartIndexes.Length];

            for (var i = 0; i < columnStartIndexes.Length; i++)
            {
                var startPos = columnStartIndexes[i];
                var nextIdx = i + 1;

                if (nextIdx < columnStartIndexes.Length)
                {
                    var nextPos = columnStartIndexes[nextIdx];
                    var length = nextPos - startPos;

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
            return OriginalLine.Split(new[] {delimiter}, StringSplitOptions.None)[index];
        }

        public override string ToString()
        {
            return _originalLine;
        }

        #endregion Methods
    }
}