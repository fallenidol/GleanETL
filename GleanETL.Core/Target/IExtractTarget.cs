namespace Glean.Core.Target
{
    using System.Collections.Generic;

    public interface IExtractTarget
    {
        bool DeleteIfExists { get; }

        long CommitData(IEnumerable<object[]> dataRows);
    }
}