namespace Gleanio.Core.EventArgs
{
    using System;

    public class ParseErrorEventArgs : System.EventArgs
    {
        #region Constructors

        public ParseErrorEventArgs(string valueBeingParsed, string message)
            : base()
        {
        }

        public ParseErrorEventArgs(string valueBeingParsed, Exception exception)
            : base()
        {
        }

        #endregion Constructors
    }
}