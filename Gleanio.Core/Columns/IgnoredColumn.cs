using System;

namespace Gleanio.Core.Columns
{
    public sealed class IgnoredColumn : BaseColumn
    {
        public IgnoredColumn()
            : base(null, null)
        {
        }
    }

    public class DerivedColumn<T> : BaseColumn<T>
    {
        protected DerivedColumn(string columnName = null)
            : base(columnName)
        {

        }

        public Func<object, T> DeriveValue { get; set; }

        public override T ParseValue(string value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }

    public class DerivedStringColumn : DerivedColumn<string>
    {
        public DerivedStringColumn(string columnName = null)
            : base(columnName)
        {
        }

        public int DetectedMaxLength { get; internal set; }
    }
}