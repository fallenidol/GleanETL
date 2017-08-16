namespace Glean.Core.Source
{
    using System;
    using System.Collections.Generic;

    public class MemorySource : IExtractSource
    {
        private readonly object enumeratorLock1 = new object();

        public MemorySource(string displayName, IEnumerable<string> data)
        {
            TakeLineIf = line => true;

            DisplayName = displayName;
            Data = data;
        }

        private IEnumerable<string> Data { get; }

        public IEnumerator<TextLine> EnumerateLines()
        {
            lock (enumeratorLock1)
            {
                foreach (var line in Data)
                {
                    if (TakeLineIf.Invoke(line))
                    {
                        yield return new TextLine(line);
                    }
                }
            }
        }

        public string DisplayName { get; }

        public Func<string, bool> TakeLineIf { get; set; }
    }
}