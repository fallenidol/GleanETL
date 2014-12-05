using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class ExtractRecordsToSeparatedValueFile : RecordExtraction<SeparatedValueFileTarget>
    {
        #region Constructors

        public ExtractRecordsToSeparatedValueFile(BaseColumn[] columns, TextFile source, SeparatedValueFileTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }
}