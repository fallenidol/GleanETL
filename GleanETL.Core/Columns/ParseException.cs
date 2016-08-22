namespace GleanETL.Core.Columns
{
	using System;

	[Serializable]
	public class ParseException : Exception
	{
		#region Constructors

		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public ParseException(string valueBeingParsed, Type targetType)
			: base(string.Format("'{0}' could not be parsed to [{1}].", valueBeingParsed, targetType.Name))
		{
			this.TargetType = targetType;
			this.ValueBeingParsed = valueBeingParsed;
		}

		#endregion Constructors

		#region Properties

		public Type TargetType
		{
			get;
			private set;
		}

		public string ValueBeingParsed
		{
			get;
			private set;
		}

		#endregion Properties
	}
}