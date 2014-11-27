using Gleanio.Core.Columns;

namespace Gleanio.Core.Target
{
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

        public abstract void SaveRows(ICollection<ICollection<object>> rowData);

        #endregion Methods
    }
}