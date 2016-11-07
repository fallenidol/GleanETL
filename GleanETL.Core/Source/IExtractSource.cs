namespace Glean.Core.Source
{
    using System;
    using System.Collections.Generic;

    public interface IExtractSource
    {
        string DisplayName { get; }

        Func<string, bool> TakeLineIf { get; set; }

        IEnumerator<TextLine> EnumerateLines();
    }
}