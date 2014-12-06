using System;
using Gleanio.Core.EventArgs;

namespace Gleanio.Core.Extraction
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