namespace Glean.Core.Source
{
    using System;
    using System.Collections.Generic;

    public class MemorySource : IExtractSource
    {
        private readonly object enumeratorLock1 = new object();

        public MemorySource(string displayName, IEnumerable<string> data)
        {
            this.TakeLineIf = line => true;

            this.DisplayName = displayName;
            this.Data = data;
        }

        private IEnumerable<string> Data { get; set; }

        public IEnumerator<TextLine> EnumerateLines()
        {
            lock (this.enumeratorLock1)
            {
                foreach (var line in this.Data)
                {
                    if (this.TakeLineIf.Invoke(line))
                    {
                        yield return new TextLine(line);
                    }
                }
            }
        }

        public string DisplayName { get; private set; }

        public Func<string, bool> TakeLineIf { get; set; }
    }
}