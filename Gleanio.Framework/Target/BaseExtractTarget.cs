using System.Collections.Generic;
using System.Threading.Tasks;

namespace gleanio.framework.Target
{
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

        #endregion Properties

        #region Methods

        public abstract void SaveRows(ICollection<ICollection<object>> rowData);

        #endregion Methods
    }
}