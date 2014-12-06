using System.Collections.Generic;
using System.Diagnostics;

namespace Gleanio.Core.Target
{
    public class TraceOutputTarget : BaseExtractTarget
    {
        #region Methods

        public override long CommitData(IEnumerable<object[]> dataRows)
        {
            long lineCount = 0;

            dataRows.ForEach((i, o) =>
            {
                Trace.WriteLine(string.Format("Row {0}: {1}", i + 1, string.Join(", ", o)));
                lineCount++;
            });

            return lineCount;
        }

        #endregion Methods
    }
}