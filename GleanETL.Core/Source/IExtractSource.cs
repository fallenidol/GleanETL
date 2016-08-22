namespace GleanETL.Core.Source
{
	using System;
	using System.Collections.Generic;

	public interface IExtractSource
	{
		#region Properties

		string DisplayName
		{
			get;
		}

		Func<string, bool> TakeLineIf
		{
			get; set;
		}

		#endregion Properties

		#region Methods

		IEnumerator<TextLine> EnumerateLines();

		#endregion Methods
	}
}