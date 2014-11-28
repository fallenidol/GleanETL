namespace Gleanio.Core.Target
{
    using Gleanio.Core.Columns;
    using System.Collections.Generic;

    public abstract class BaseExtractTarget : IExtractTarget
    {
        #region Constructors

        protected BaseExtractTarget(bool deleteIfExists = false)
        {
            DeleteIfExists = deleteIfExists;
        }

        #endregion Constructors

        #region Properties

        public bool DeleteIfExists
        {
            get; private set;
        }

        internal BaseColumn[] Columns
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public abstract void CommitData(IEnumerable<object[]> dataRows);

        #endregion Methods
    }
}