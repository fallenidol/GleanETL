using System;

namespace Gleanio.Core.EventArgs
{
    public class ParseErrorEventArgs : System.EventArgs
    {
        public string ValueBeingParsed { get; private set; }
        public Exception Exception { get; private set; }
        public string Message { get; private set; }
        public Type TargetType { get; private set; }

        #region Constructors

        public ParseErrorEventArgs(string valueBeingParsed, string message, Type targetType)
        {
            Message = message;
            ValueBeingParsed = valueBeingParsed;
            TargetType = targetType;
        }

        public ParseErrorEventArgs(string valueBeingParsed, Exception exception, Type targetType)
        {
            Exception = exception;
            ValueBeingParsed = valueBeingParsed;
            TargetType = targetType;
        }

        #endregion Constructors
    }
}