using System.Collections.Generic;

namespace gleanio.framework.Target
{
    public interface IExtractTarget
    {
        #region Properties

        bool DeleteIfExists
        {
            get;
        }

        #endregion Properties

        #region Methods

        void SaveRows(ICollection<ICollection<object>> rowData);

        #endregion Methods
    }
}