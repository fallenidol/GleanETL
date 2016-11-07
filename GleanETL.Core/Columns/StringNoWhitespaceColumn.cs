namespace Glean.Core.Columns
{
    using Glean.Core.Enumerations;

    public class StringNoWhiteSpaceColumn : StringColumn
    {
        public StringNoWhiteSpaceColumn()
    : base(null, -1, false, WhiteSpaceHandling.RemoveAllWhiteSpace, StringCapitalisation.DefaultDoNothing)
        {
        }

        public StringNoWhiteSpaceColumn(string columnName = null, int maxLength = -1, bool encloseInDoubleQuotes = false)
            : base(columnName, maxLength, encloseInDoubleQuotes, WhiteSpaceHandling.RemoveAllWhiteSpace, StringCapitalisation.DefaultDoNothing)
        {
        }
    }
}