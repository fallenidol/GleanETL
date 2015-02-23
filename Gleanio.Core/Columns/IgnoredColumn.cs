﻿namespace Gleanio.Core.Columns
{
    using System;

    public class DerivedColumn<T> : BaseColumn<T>
    {
        #region Constructors

        protected DerivedColumn(string columnName = null)
            : base(columnName)
        {
        }

        #endregion Constructors

        #region Properties

        public Func<object, T> DeriveValue
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public override T ParseValue(string value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        #endregion Methods
    }

    public class DerivedStringColumn : DerivedColumn<string>
    {
        #region Constructors

        public DerivedStringColumn(string columnName = null)
            : base(columnName)
        {
        }

        #endregion Constructors

        #region Properties

        public int DetectedMaxLength
        {
            get; internal set;
        }

        #endregion Properties
    }

    public sealed class IgnoredColumn : BaseColumn
    {
        #region Constructors

        public IgnoredColumn()
            : base(null, null)
        {
        }

        #endregion Constructors
    }
}