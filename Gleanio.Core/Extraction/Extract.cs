using System;
using Gleanio.Core.Columns;
using Gleanio.Core.EventArgs;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
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

            Columns.ForEach(column =>
            {
                column.ParseError -= OnDataParseError;
                column.ParseError += OnDataParseError;
            });
        }

        #endregion Constructors

        public event EventHandler<ParseErrorEventArgs> DataParseError;
        public event EventHandler<ExtractCompleteArgs> ExtractComplete;

        private void OnDataParseError(object sender, ParseErrorEventArgs parseErrorEventArgs)
        {
            var handler = DataParseError;
            if (handler != null)
            {
                handler(this, parseErrorEventArgs);
            }
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

        #region Properties

        public TextFile Source { get; private set; }

        public IExtractTarget Target { get; private set; }

        internal BaseColumn[] Columns { get; private set; }

        #endregion Properties

        #region Methods

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
                        var colType = column.GetType();

                        if (colType == typeof (IgnoredColumn))
                        {
                        }
                        else if (colType == typeof (StringNoWhitespaceColumn))
                        {
                            var scol = (StringNoWhitespaceColumn) column;
                            var value = scol.ParseValue(rawLineValues[i]);

                            parsedLineValues[i] = value;

                            var len = value.Length;
                            if (scol.DetectedMaxLength < len)
                            {
                                scol.DetectedMaxLength = len;
                            }
                        }
                        else if (colType == typeof (StringColumn))
                        {
                            var scol = (StringColumn) column;
                            var value = scol.ParseValue(rawLineValues[i]);

                            parsedLineValues[i] = value;

                            var len = value.Length;
                            if (scol.DetectedMaxLength < len)
                            {
                                scol.DetectedMaxLength = len;
                            }
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
            return null;
        }

        #endregion Methods
    }
}