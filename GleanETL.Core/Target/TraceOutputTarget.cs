namespace Glean.Core.Target
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public class TraceOutputTarget : BaseExtractTarget
    {
        public override long CommitData(IEnumerable<object[]> dataRows)
        {
            long lineCount = 0;

            dataRows.ForEach(
                (i, o) =>
                {
                    //var valuesWithoutIgnoredColumns = ValuesWithoutIgnoredColumns(o);

                    Trace.WriteLine(string.Format("Row {0}: {1}", i + 1, string.Join(",", o)));

                    lineCount++;
                });

            return lineCount;
        }
    }
}