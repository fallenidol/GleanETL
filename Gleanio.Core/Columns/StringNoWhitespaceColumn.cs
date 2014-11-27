namespace Gleanio.Core.Columns
{
    using Gleanio.Core.Enumerations;

    public class StringNoWhitespaceColumn : StringColumn
    {
        #region Constructors

        public StringNoWhitespaceColumn(string columnName, int maxLength, bool encloseInDoubleQuotes = false)
            : base(columnName, maxLength, encloseInDoubleQuotes, WhitespaceHandling.RemoveAllWhitespace, StringCapitalisation.DefaultDoNothing)
        {
        }

        #endregion Constructors
    }
}