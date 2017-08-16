namespace Glean.Core.Target
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class SeparatedValueFileTarget : BaseExtractTarget
    {
        private bool firstSave = true;

        public SeparatedValueFileTarget(string targetFilePath, string columnDelimiter = ",",
            bool deleteTargetIfExists = true)
            : base(deleteTargetIfExists)
        {
            TargetFilePath = targetFilePath;
            ColumnDelimiter = columnDelimiter;
        }

        public string ColumnDelimiter { get; }

        public string TargetFilePath { get; }

        public override long CommitData(IEnumerable<object[]> dataRows)
        {
            if (firstSave && DeleteIfExists && File.Exists(TargetFilePath))
            {
                File.Delete(TargetFilePath);
            }
            firstSave = false;

            long rowCount = 0;
            var rows = new StringBuilder();

            foreach (var row in dataRows)
            {
                //var valuesWithoutIgnoredColumns = ValuesWithoutIgnoredColumns(row);
                var line = string.Join(ColumnDelimiter, row);

                rows.AppendLine(line);
                rowCount++;

                if (rowCount % 10000 == 0)
                {
                    File.AppendAllText(TargetFilePath, rows.ToString());
                    rows.Clear();
                }
            }

            if (rows.Length > 0)
            {
                File.AppendAllText(TargetFilePath, rows.ToString());
                rows.Clear();
            }

            return rowCount;
        }
    }
}