namespace Gleanio.Core.Columns
{
    using Gleanio.Core.Enumerations;

    public class StringNoWhitespaceColumn : StringColumn
    {
        #region Constructors

        public StringNoWhitespaceColumn(string columnName = null, int maxLength = -1, bool encloseInDoubleQuotes = false)
            : base(columnName, maxLength, encloseInDoubleQuotes, WhitespaceHandling.RemoveAllWhitespace,
                StringCapitalisation.DefaultDoNothing)
        {
        }

        #endregion Constructors
    }
}