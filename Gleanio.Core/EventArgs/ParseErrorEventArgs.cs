using System;

namespace Gleanio.Core.EventArgs
{
    public class ParseErrorEventArgs : System.EventArgs
    {
        public string ValueBeingParsed { get; private set; }
        public Exception Exception { get; private set; }
        public string Message { get; private set; }

        #region Constructors

        public ParseErrorEventArgs(string valueBeingParsed, string message)
        {
            Message = message;
            ValueBeingParsed = valueBeingParsed;
        }

        public ParseErrorEventArgs(string valueBeingParsed, Exception exception)
        {
            Exception = exception;
            ValueBeingParsed = valueBeingParsed;
        }

        #endregion Constructors
    }
}