using System;
using System.Collections.Generic;
using System.Linq;
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

        protected object[] ValuesWithoutIgnoredColumns(object[] row)
        {
            var ic = Columns.OfType<IgnoredColumn>();
            if (ic.Any())
            {
                var iic = ic.Select(column => Array.IndexOf(Columns, column));
                var io = row.Where((o, i) => iic.Contains(i));
                var valuesWithoutIgnoredColumns = row.Except(io).ToArray();
                return valuesWithoutIgnoredColumns;
            }
            else
            {
                return row;
            }
        }

        public abstract long CommitData(IEnumerable<object[]> dataRows);

        #endregion Methods

        #region Properties

        public bool DeleteIfExists { get; private set; }

        internal BaseColumn[] Columns { get; set; }

        #endregion Properties
    }
}