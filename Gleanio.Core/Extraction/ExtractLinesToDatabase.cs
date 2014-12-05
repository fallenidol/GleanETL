using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class ExtractLinesToDatabase : LineExtraction<DatabaseTableTarget>
    {
        #region Constructors

        public ExtractLinesToDatabase(BaseColumn[] columns, TextFile source, DatabaseTableTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }
}