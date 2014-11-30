namespace Gleanio.Core.Extraction
{
    using System;

    using Gleanio.Core.Columns;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;

    public abstract class Extract<TExtractTarget> : IExtract
        where TExtractTarget : BaseExtractTarget
    {
        #region Constructors

        protected Extract(BaseColumn[] columns, TextFile source, TExtractTarget target)
        {
            Columns = columns;
            target.Columns = Columns;

            Source = source;
            Target = target;
        }

        #endregion Constructors

        #region Properties

        public TextFile Source
        {
            get;
            private set;
        }

        public IExtractTarget Target
        {
            get;
            private set;
        }

        internal BaseColumn[] Columns
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        public abstract void AfterExtract();

        public abstract void BeforeExtract();

        public abstract void ExtractToTarget();

        protected object[] ParseStringValues(string[] rawLineValues)
        {
            if (!rawLineValues.IsNullOrEmpty())
            {
                var parsedLineValues = new object[Columns.Length];

                Columns.ForEach((i, column) =>
                {
                    if (i < rawLineValues.Length)
                    {
                        Type colType = column.GetType();

                        if (colType == typeof (StringNoWhitespaceColumn))
                        {
                            parsedLineValues[i] = ((StringNoWhitespaceColumn) column).ParseValue(rawLineValues[i]);
                        }
                        else if (colType == typeof (StringColumn))
                        {
                            parsedLineValues[i] = ((StringColumn) column).ParseValue(rawLineValues[i]);
                        }
                        else if (colType == typeof (IntColumn))
                        {
                            parsedLineValues[i] = ((IntColumn) column).ParseValue(rawLineValues[i]);
                        }
                        else if (colType == typeof (DecimalColumn))
                        {
                            parsedLineValues[i] = ((DecimalColumn) column).ParseValue(rawLineValues[i]);
                        }
                        else if (colType == typeof (MoneyColumn))
                        {
                            parsedLineValues[i] = ((MoneyColumn) column).ParseValue(rawLineValues[i]);
                        }
                        else if (colType == typeof (DateColumn))
                        {
                            parsedLineValues[i] = ((DateColumn) column).ParseValueAndFormat(rawLineValues[i]);
                        }
                        else
                        {
                            throw new NotImplementedException("Column type not implemented.");
                        }
                    }
                    else
                    {
                        parsedLineValues[i] = null;
                    }
                });

                return parsedLineValues;
            }
            else
            {
                return null;
            }
        }

        #endregion Methods

        #region Other

        //public event ProgressChangedEventHandler ProgressChanged;
        //public event EventHandler<ExtractCompleteEventArgs> ExtractComplete;
        //protected void OnExtractComplete()
        //{
        //    if (ExtractComplete != null)
        //    {
        //        var args = new ExtractCompleteEventArgs();
        //        ExtractComplete.Invoke(this, args);
        //    }
        //}
        //protected void OnExtractComplete(ExtractCompleteEventArgs args)
        //{
        //    if (ExtractComplete != null)
        //    {
        //        ExtractComplete.Invoke(this, args);
        //    }
        //}
        //private int _progressPercent = -1;
        //private int _totalLinesToImport = -1;
        //protected void IncrementProgress(int lineNumber)
        //{
        //    if (_totalLinesToImport == -1)
        //    {
        //        _totalLinesToImport = Source.LinesToImport.Count();
        //    }
        //    int percent = (lineNumber * 100) / _totalLinesToImport;
        //    OnProgressChanged(percent);
        //}
        //protected void OnProgressChanged(int percent)
        //{
        //    if (ProgressChanged != null && _progressPercent != percent)
        //    {
        //        _progressPercent = percent;
        //        var args = new ProgressChangedEventArgs(percent, null);
        //        ProgressChanged.Invoke(this, args);
        //    }
        //}

        #endregion Other
    }
}