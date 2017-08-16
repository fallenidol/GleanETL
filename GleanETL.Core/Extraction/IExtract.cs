namespace Glean.Core.Extraction
{
    using System;
    using Glean.Core.EventArgs;

    public interface IExtract
    {
        event EventHandler<ParseErrorEventArgs> DataParseError;

        event EventHandler<ExtractCompleteArgs> ExtractComplete;

        void ExtractToTarget();
    }
}