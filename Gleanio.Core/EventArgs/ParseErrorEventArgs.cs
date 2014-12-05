using System;

namespace Gleanio.Core.EventArgs
{
    public class ParseErrorEventArgs : System.EventArgs
    {
        #region Constructors

        public ParseErrorEventArgs(string valueBeingParsed, string message)
        {
        }

        public ParseErrorEventArgs(string valueBeingParsed, Exception exception)
        {
        }

        #endregion Constructors
    }
}