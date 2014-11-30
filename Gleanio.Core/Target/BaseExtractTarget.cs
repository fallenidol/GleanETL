namespace Gleanio.Core.Target
{
    using System.Collections.Generic;

    using Gleanio.Core.Columns;

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