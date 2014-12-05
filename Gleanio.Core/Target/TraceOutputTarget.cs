using System.Collections.Generic;
using System.Diagnostics;

namespace Gleanio.Core.Target
{
    public class TraceOutputTarget : BaseExtractTarget
    {
        #region Methods

        public override void CommitData(IEnumerable<object[]> dataRows)
        {
            dataRows.ForEach((i, o) => Trace.WriteLine(string.Format("Row {0}: {1}", i + 1, string.Join(", ", o))));
        }

        #endregion Methods
    }
}