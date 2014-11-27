using System.Collections.Generic;
using System.Diagnostics;

namespace Gleanio.Core.Target
{
    public class TraceOutputTarget: BaseExtractTarget
    {

        public override void SaveRows(ICollection<ICollection<object>> rowData)
        {
            rowData.ForEach(objects => Trace.WriteLine(string.Join("=>", objects)));
        }

    }
}
