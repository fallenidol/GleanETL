using Gleanio.Core.Columns;

namespace Gleanio.Core.Target
{
    using System.Collections.Generic;

    public interface IExtractTarget
    {
        #region Properties

        bool DeleteIfExists
        {
            get;
        }

        //BaseColumn[] Columns { get; set; }

        #endregion Properties

        #region Methods

        void SaveRows(ICollection<ICollection<object>> rowData);

        #endregion Methods
    }
}