namespace Glean.Core.EventArgs
{
    using System;

    public class ParseErrorEventArgs : EventArgs
    {
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

        public string ValueBeingParsed { get; }

        public Exception Exception { get; }

        public string Message { get; }

        public Type TargetType { get; }
    }
}