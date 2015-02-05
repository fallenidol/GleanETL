using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Gleanio.Core.Columns;
using Gleanio.Core.EventArgs;
using Gleanio.Core.Source;
using Gleanio.Core.Target;

namespace Gleanio.Core.Extraction
{
    public abstract class Extract<TExtractTarget> : IExtract
        where TExtractTarget : BaseExtractTarget
    {
        public override string ToString()
        {
            return string.Format("{0} -> {1}", Source.FileInfo.Directory.Name + "/" + Source.FileInfo.Name, Target);
        }

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

                string[] rawValuesPlusDerived = null;


                var ic = Columns.Where(x => x.GetType().BaseType.Name.StartsWith("DerivedColumn"));
                //if (ic.Any())
                //{
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
                //}



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

        #endregion Methods
    }
}