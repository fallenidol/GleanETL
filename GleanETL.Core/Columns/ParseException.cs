namespace Glean.Core.Columns
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    [Serializable]
    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ParseException(string valueBeingParsed, Type targetType)
            : base(string.Format(CultureInfo.InvariantCulture, "'{0}' could not be parsed to [{1}].", valueBeingParsed,
                targetType.Name))
        {
            TargetType = targetType;
            ValueBeingParsed = valueBeingParsed;
        }

        public Type TargetType { get; }

        public string ValueBeingParsed { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}