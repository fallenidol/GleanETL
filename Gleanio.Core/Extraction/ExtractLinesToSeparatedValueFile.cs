using Gleanio.Core.Columns;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public class ExtractLinesToSeparatedValueFile : LineExtraction<SeparatedValueFileTarget>
    {
        #region Constructors

        public ExtractLinesToSeparatedValueFile(BaseColumn[] columns, TextFile source, SeparatedValueFileTarget target)
            : base(columns, source, target)
        {
        }

        #endregion Constructors
    }
}