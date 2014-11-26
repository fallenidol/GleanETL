using gleanio.framework.Enumerations;

namespace gleanio.framework.Columns
{
    public class StringNoWhitespaceColumn : StringColumn
    {
        #region Constructors

        public StringNoWhitespaceColumn(string columnName, int maxLength, bool encloseInDoubleQuotes = false)
            : base(columnName, maxLength, encloseInDoubleQuotes, WhitespaceHandling.RemoveAllWhitespace, Capitalisation.DefaultDoNothing)
        {
        }

        #endregion Constructors
    }
}