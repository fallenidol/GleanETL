namespace Gleanio.Framework.Target
{
    using System.Collections.Generic;

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