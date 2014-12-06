using System.Collections.Generic;

namespace Gleanio.Core.Target
{
    public interface IExtractTarget
    {
        #region Properties

        bool DeleteIfExists { get; }

        #endregion Properties

        #region Methods

        long CommitData(IEnumerable<object[]> dataRows);

        #endregion Methods

        #region Other

        #endregion Other
    }
}