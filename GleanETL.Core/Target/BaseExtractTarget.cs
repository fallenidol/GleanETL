namespace Glean.Core.Target
{
    using System.Collections.Generic;

    using Glean.Core.Columns;

    public abstract class BaseExtractTarget : IExtractTarget
    {
        protected BaseExtractTarget(bool deleteIfExists = false)
        {
            this.ThrowMultipleEnumerationError = true;
            this.DeleteIfExists = deleteIfExists;
        }

        public bool ThrowMultipleEnumerationError { get; private set; }

        internal BaseColumn[] Columns { get; set; }

        //protected object[] ValuesWithoutIgnoredColumns(object[] row)
        //{
        //    var ic = Columns.OfType<IgnoredColumn>();
        //    if (ic.Any())
        //    {
        //        var iic = ic.Select(column => Array.IndexOf(Columns, column));
        //        var io = row.Where((o, i) => iic.Contains(i));
        //        var valuesWithoutIgnoredColumns = row.Except(io).ToArray();
        //        return valuesWithoutIgnoredColumns;
        //    }
        //    else
        //    {
        //        return row;
        //    }
        //}

        public abstract long CommitData(IEnumerable<object[]> dataRows);

        public bool DeleteIfExists { get; private set; }
    }
}