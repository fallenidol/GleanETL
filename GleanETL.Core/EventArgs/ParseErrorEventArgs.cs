namespace Glean.Core.EventArgs
{
    using System;

    public class ParseErrorEventArgs : EventArgs
    {
        public ParseErrorEventArgs(string valueBeingParsed, string message, Type targetType)
        {
            this.Message = message;
            this.ValueBeingParsed = valueBeingParsed;
            this.TargetType = targetType;
        }

        public ParseErrorEventArgs(string valueBeingParsed, Exception exception, Type targetType)
        {
            this.Exception = exception;
            this.ValueBeingParsed = valueBeingParsed;
            this.TargetType = targetType;
        }

        public string ValueBeingParsed { get; private set; }

        public Exception Exception { get; private set; }

        public string Message { get; private set; }

        public Type TargetType { get; private set; }
    }
}