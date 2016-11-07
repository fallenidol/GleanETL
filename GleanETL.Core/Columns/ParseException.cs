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
            : base(string.Format(CultureInfo.InvariantCulture, "'{0}' could not be parsed to [{1}].", valueBeingParsed, targetType.Name))
        {
            this.TargetType = targetType;
            this.ValueBeingParsed = valueBeingParsed;
        }

        public Type TargetType { get; private set; }

        public string ValueBeingParsed { get; private set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}