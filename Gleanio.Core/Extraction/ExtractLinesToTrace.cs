using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class ExtractLinesToTrace : LineExtraction<TraceOutputTarget>
    {
        #region Constructors

        public ExtractLinesToTrace(BaseColumn[] columns, TextFile source, TraceOutputTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }
}