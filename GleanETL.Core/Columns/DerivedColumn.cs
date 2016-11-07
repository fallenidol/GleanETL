namespace Glean.Core.Columns
{
    using System;

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
}