using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class ExtractRecordsToTrace : RecordExtraction<TraceOutputTarget>
    {
        #region Constructors

        public ExtractRecordsToTrace(BaseColumn[] columns, TextFile source, TraceOutputTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }
}