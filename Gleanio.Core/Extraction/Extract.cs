namespace Gleanio.Core.Extraction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Gleanio.Core.Columns;
    using Gleanio.Core.EventArgs;
    using Gleanio.Core.Source;
    using Gleanio.Core.Target;

    public abstract class Extract<TExtractTarget> : IExtract
        where TExtractTarget : BaseExtractTarget
    {
        #region Fields

        private bool _throwParseErrors = true;

        #endregion Fields

        #region Constructors

        protected Extract(BaseColumn[] columns, IExtractSource source, TExtractTarget target, bool throwParseErrors = true)
        {
            ThrowMultipleEnumerationError = true;
            _throwParseErrors = throwParseErrors;

            Columns = columns;
            target.Columns = Columns;

            Source = source;
            Target = target;

            Columns.ForEach(column =>
            {
                column.ParseError -= OnDataParseError;
                column.ParseError += OnDataParseError;
            });

            DataParseError -= DataParseErrorHandler;
            DataParseError += DataParseErrorHandler;
        }

        #endregion Constructors

        #region Events

        public event EventHandler<ParseErrorEventArgs> DataParseError;

        public event EventHandler<ExtractCompleteArgs> ExtractComplete;

        #endregion Events

        #region Properties

        public IExtractSource Source
        {
            get; private set;
        }

        public IExtractTarget Target
        {
            get; private set;
        }

        public bool ThrowMultipleEnumerationError
        {
            get; private set;
        }

        internal BaseColumn[] Columns
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        public abstract void ExtractToTarget();

        public override string ToString()
        {
            return string.Format("{0} -> {1}", Source.DisplayName, Target);
        }

        protected void OnExtractComplete(long linesExtractedFromSource, long linesPassedToTarget,
            long linesCommittedToTarget, long extractDurationMs, long commitDurationMs, long durationInMs)
        {
            var handler = ExtractComplete;
            if (handler != null)
            {
                handler(this,
                    new ExtractCompleteArgs(linesExtractedFromSource, linesPassedToTarget, linesCommittedToTarget,
                        extractDurationMs, commitDurationMs, durationInMs));
            }
        }

        protected object[] ParseStringValues(string[] rawLineValues)
        {
            if (!rawLineValues.IsNullOrEmpty())
            {
                var parsedLineValues = new object[Columns.Length];

                string[] rawValuesPlusDerived = rawLineValues;

                var ic = Columns.Where(x => x.GetType().BaseType.Name.StartsWith("DerivedColumn"));
                if (ic.Any())
                {
                    var l = new List<string>(rawLineValues);
                    var iic = ic.Select(column => Array.IndexOf(Columns, column)).ToArray();

                    for (int i = 0; i < iic.Length; i++)
                    {
                        if (iic[i] > l.Count)
                        {
                            l.Add(null);
                        }
                        else
                        {
                            l.Insert(Math.Max(0, iic[i]), null);
                        }
                    }

                    rawValuesPlusDerived = l.ToArray();
                }

                Columns.ForEach((i, column) =>
                {
                    if (rawValuesPlusDerived != null && i < rawValuesPlusDerived.Length)
                    {
                        var colType = column.GetType();

                        if (colType == typeof(IgnoredColumn))
                        {
                        }
                        else if (colType == typeof(DerivedStringColumn))
                        {
                            var col = ((DerivedStringColumn)column);
                            var value = col.ParseValue(col.DeriveValue(parsedLineValues));
                            parsedLineValues[i] = value;

                            var len = value == null ? 0 : value.Length;
                            if (col.DetectedMaxLength < len)
                            {
                                col.DetectedMaxLength = len;
                            }
                        }
                        else if (colType == typeof(StringNoWhitespaceColumn))
                        {
                            var scol = (StringNoWhitespaceColumn)column;
                            var value = scol.ParseValue(rawValuesPlusDerived[i]);

                            parsedLineValues[i] = value;

                            var len = value == null ? 0 : value.Length;
                            if (scol.DetectedMaxLength < len)
                            {
                                scol.DetectedMaxLength = len;
                            }
                        }
                        else if (colType == typeof(StringColumn))
                        {
                            var scol = (StringColumn)column;
                            var value = scol.ParseValue(rawValuesPlusDerived[i]);

                            parsedLineValues[i] = value;

                            var len = value == null ? 0 : value.Length;
                            if (scol.DetectedMaxLength < len)
                            {
                                scol.DetectedMaxLength = len;
                            }
                        }
                        else if (colType == typeof(IntColumn))
                        {
                            parsedLineValues[i] = ((IntColumn)column).ParseValue(rawValuesPlusDerived[i]);
                        }
                        else if (colType == typeof(DecimalColumn))
                        {
                            parsedLineValues[i] = ((DecimalColumn)column).ParseValue(rawValuesPlusDerived[i]);
                        }
                        else if (colType == typeof(MoneyColumn))
                        {
                            parsedLineValues[i] = ((MoneyColumn)column).ParseValue(rawValuesPlusDerived[i]);
                        }
                        else if (colType == typeof(DateColumn))
                        {
                            parsedLineValues[i] = ((DateColumn)column).ParseValueAndFormat(rawValuesPlusDerived[i]);
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
            return null;
        }

        private void DataParseErrorHandler(object sender, ParseErrorEventArgs e)
        {
            if (_throwParseErrors)
            {
                Trace.WriteLine(string.Format("PARSE ERROR: {0}, {1}", e.ValueBeingParsed ?? string.Empty, e.Message));
                throw new ParseException(e.ValueBeingParsed, e.TargetType);
            }
        }

        private void OnDataParseError(object sender, ParseErrorEventArgs parseErrorEventArgs)
        {
            var handler = DataParseError;
            if (handler != null)
            {
                handler(this, parseErrorEventArgs);
            }
        }

        #endregion Methods
    }
}