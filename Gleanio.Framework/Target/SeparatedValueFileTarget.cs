using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace gleanio.framework.Target
{
    public class SeparatedValueFileTarget : BaseExtractTarget
    {
        #region Fields

        private readonly string _columnDelimiter;
        private readonly string _targetFilePath;

        private bool _firstSave = true;

        #endregion Fields

        #region Constructors

        public SeparatedValueFileTarget(string targetFilePath, string columnDelimiter = ",", bool deleteTargetIfExists = true)
            : base(deleteTargetIfExists)
        {
            _targetFilePath = targetFilePath;
            _columnDelimiter = columnDelimiter;
        }

        #endregion Constructors

        #region Properties

        public string ColumnDelimiter
        {
            get { return _columnDelimiter; }
        }

        public string TargetFilePath
        {
            get { return _targetFilePath; }
        }

        #endregion Properties

        #region Methods

        public override void SaveRows(ICollection<ICollection<object>> rowData)
        {
            if (_firstSave && DeleteIfExists && System.IO.File.Exists(TargetFilePath))
            {
                System.IO.File.Delete(TargetFilePath);
            }
            _firstSave = false;

            long rowCount = 0;
            var rows = new StringBuilder();

            foreach (var row in rowData)
            {
                string line = string.Join(ColumnDelimiter, row.ToArray());

                rows.AppendLine(line);
                rowCount++;

                if (rowCount % 10000 == 0)
                {
                    System.IO.File.AppendAllText(TargetFilePath, rows.ToString());
                    rows.Clear();

                    //Debug.WriteLine("Committed {0} rows.", rowCount);
                }
            }

            if (rows.Length > 0)
            {
                System.IO.File.AppendAllText(TargetFilePath, rows.ToString());
                rows.Clear();
            }

            Debug.WriteLine("DONE! Committed {0} rows.", rowCount);
        }

        #endregion Methods
    }
}