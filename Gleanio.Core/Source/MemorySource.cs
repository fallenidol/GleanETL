namespace Gleanio.Core.Source
{
    using System;
    using System.Collections.Generic;

    public class MemorySource : IExtractSource
    {
        #region Fields

        private readonly object _enumeratorLock1 = new object();

        #endregion Fields

        #region Constructors

        public MemorySource(string displayName, IEnumerable<string> data)
        {
            TakeLineIf = line => true;

            DisplayName = displayName;
            Data = data;
        }

        #endregion Constructors

        #region Properties

        public string DisplayName
        {
            get;
            private set;
        }

        public Func<string, bool> TakeLineIf
        {
            get; set;
        }

        private IEnumerable<string> Data
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public IEnumerator<TextLine> EnumerateLines()
        {
            lock (_enumeratorLock1)
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

        #endregion Methods
    }
}