using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class ExtractRecordsToDatabase : RecordExtraction<DatabaseTableTarget>
    {
        #region Constructors

        public ExtractRecordsToDatabase(BaseColumn[] columns, TextFile source, DatabaseTableTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }
}