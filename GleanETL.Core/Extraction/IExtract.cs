using System;
using GleanETL.Core.EventArgs;

namespace GleanETL.Core.Extraction
{
    public interface IExtract
    {
        #region Methods

        event EventHandler<ParseErrorEventArgs> DataParseError;
        event EventHandler<ExtractCompleteArgs> ExtractComplete;

        void ExtractToTarget();

        #endregion Methods
    }
}