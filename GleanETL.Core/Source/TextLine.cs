namespace Glean.Core.Source
{
    using System;

    public class TextLine
    {
        public TextLine(string originalLineText)
        {
            OriginalLine = originalLineText;
        }

        public int LineLength => OriginalLine.Length;

        public string OriginalLine { get; }

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

                    if (length < LineLength - startPos)
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
            return OriginalLine;
        }
    }
}