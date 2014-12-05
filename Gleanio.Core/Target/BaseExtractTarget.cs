using System.Collections.Generic;
using Gleanio.Core.Columns;

namespace Gleanio.Core.Target
{
    public abstract class BaseExtractTarget : IExtractTarget
    {
        #region Constructors

        protected BaseExtractTarget(bool deleteIfExists = false)
        {
            DeleteIfExists = deleteIfExists;
        }

        #endregion Constructors

        #region Methods

        public abstract void CommitData(IEnumerable<object[]> dataRows);

        #endregion Methods

        #region Properties

        public bool DeleteIfExists { get; private set; }

        internal BaseColumn[] Columns { get; set; }

        #endregion Properties
    }
}