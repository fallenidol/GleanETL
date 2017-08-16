namespace Glean.Core.Target
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Glean.Core.Columns;

    public class DatabaseTableTarget : BaseExtractTarget
    {
        public static string UniqueRowIdColumnName = "RowIndex";

        private readonly string connectionString;

        private readonly string schema;

        private readonly string table;

        private bool firstTime = true;

        public DatabaseTableTarget(string connectionString, string targetTable, string targetSchema = "dbo",
            bool deleteTableIfExists = false)
            : base(deleteTableIfExists)
        {
            table = targetTable.Trim();
            schema = targetSchema.Trim();

            var csb = new SqlConnectionStringBuilder(connectionString) { PersistSecurityInfo = true };

            this.connectionString = csb.ConnectionString;
        }

        public override string ToString()
        {
            var sb = new SqlConnectionStringBuilder(connectionString);
            return string.Format("{0}.{1}.{2}", sb.DataSource, sb.InitialCatalog, table);
        }

        public override long CommitData(IEnumerable<object[]> dataRows)
        {
            const int BatchSize = 10000;
            long lineCount = 0;

            using (var c = new SqlConnection(connectionString))
            {
                c.Open();

                if (DeleteIfExists && firstTime)
                {
                    using (
                        var cmd =
                            new SqlCommand(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{1}' and TABLE_SCHEMA='{0}') TRUNCATE TABLE {0}.{1};",
                                    schema,
                                    table),
                                c))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (var data = new DataTable(table))
                {
                    var ordinal = 0;

                    var rowId = new DataColumn(UniqueRowIdColumnName, typeof(long))
                    {
                        Unique = true,
                        AutoIncrementSeed = 1,
                        AutoIncrementStep = 1,
                        AutoIncrement = true,
                        AllowDBNull = false
                    };

                    if (!DeleteIfExists)
                    {
                        using (
                            var cmd =
                                new SqlCommand(
                                    string.Format(
                                        "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{1}' AND TABLE_SCHEMA='{0}') SELECT Coalesce(MAX({2}),0)+1 FROM [{0}].[{1}]; ELSE SELECT 1;",
                                        schema,
                                        table,
                                        UniqueRowIdColumnName),
                                    c))
                        {
                            rowId.AutoIncrementSeed = Convert.ToInt64(cmd.ExecuteScalar());
                        }
                    }

                    data.Columns.Add(rowId);
                    rowId.SetOrdinal(ordinal);

                    data.PrimaryKey = new[] { rowId };

                    foreach (var col in Columns)
                    {
                        if (!(col is IgnoredColumn))
                        {
                            ordinal++;

                            var dc = GetDataColumn(col);
                            data.Columns.Add(dc);

                            dc.SetOrdinal(ordinal);
                        }
                    }

                    data.BeginLoadData();

                    using (var iterator = dataRows.GetEnumerator())
                    {
                        if (iterator.MoveNext())
                        {
                            var row = iterator.Current;

                            while (iterator.MoveNext())
                            {
                                AddRow(row, data, BatchSize, c, false);
                                lineCount++;

                                row = iterator.Current;
                            }

                            AddRow(row, data, BatchSize, c, true);
                            lineCount++;
                        }
                        else
                        {
                            CreateSchema(data, c);
                        }
                    }

                    if (lineCount > 0)
                    {
                        var sqlBuilder = new StringBuilder();
                        foreach (var column in Columns.OfType<BaseColumn>()
                            .Where(bc => bc is StringColumn || bc is DerivedStringColumn))
                        {
                            var length = 255;
                            var maxlength = -1;
                            if (column is StringColumn)
                            {
                                var sc = column as StringColumn;
                                maxlength = sc.MaxLength;
                                length = sc.DetectedMaxLength <= 0
                                    ? sc.MaxLength <= 0
                                        ? 255
                                        : sc.MaxLength
                                    : sc.DetectedMaxLength;
                            }
                            else if (column is DerivedStringColumn)
                            {
                                var sc = column as DerivedStringColumn;
                                length = sc.DetectedMaxLength <= 0 ? 255 : sc.DetectedMaxLength;
                                maxlength = length > 4000 ? -1 : length;
                            }

                            maxlength = length > 4000 ? -1 : length;

                            if (DeleteIfExists)
                            {
                                sqlBuilder.AppendLine(
                                    string.Format(
                                        "ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] nvarchar({3});",
                                        schema,
                                        table,
                                        column.ColumnName,
                                        maxlength != -1 ? length.ToString() : "MAX"));
                            }
                            else
                            {
                                sqlBuilder.AppendLine(
                                    string.Format(
                                        "IF ((SELECT MAX(LEN([{2}])) FROM [{0}].[{1}]) < {3}) ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] nvarchar({4});",
                                        schema,
                                        table,
                                        column.ColumnName,
                                        length,
                                        maxlength != -1 ? length.ToString() : "MAX"));
                            }
                        }
                        using (var cmd = new SqlCommand(sqlBuilder.ToString(), c))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return lineCount;
        }

        private void AddRow(object[] row, DataTable data, int batchSize, SqlConnection c, bool isLastRow)
        {
            //var valuesWithoutIgnoredColumns = ValuesWithoutIgnoredColumns(row);

            var values = new object[] { null }.Concat(row);

            if (data.Rows.Count <= batchSize)
            {
                data.Rows.Add(values.ToArray());
            }

            if (data.Rows.Count == batchSize || isLastRow)
            {
                if (firstTime)
                {
                    CreateSchema(data, c);

                    firstTime = false;
                }

                using (var sbc = new SqlBulkCopy(c.ConnectionString, SqlBulkCopyOptions.TableLock))
                {
                    sbc.DestinationTableName = schema + "." + table;
                    sbc.WriteToServer(data);
                }

                data.EndLoadData();
                data.Clear();
                data.Columns[0].AutoIncrementSeed += batchSize;
            }
        }

        private void CreateSchema(DataTable data, SqlConnection c)
        {
            var schemaSql = @"
sp_executesql @statement=N'
IF (NOT EXISTS (SELECT ''x'' FROM sys.schemas WHERE name = ''" + schema + @"''))
    BEGIN
        EXEC sp_executesql N''CREATE SCHEMA [" + schema + @"] AUTHORIZATION [dbo]'';
    END
'
";

            using (var cmd = new SqlCommand(schemaSql, c))
            {
                cmd.ExecuteNonQuery();
            }

            var createTableSql = SqlTableCreator.GetCreateFromDataTableSql(table, data, schema);
            var sql = createTableSql + "; ";
            if (DeleteIfExists)
            {
                sql = @"
sp_executesql @statement=N'
IF (EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = ''" + schema +
                      @"'' AND TABLE_NAME = ''" + table + @"'')) 
    BEGIN 
        DROP TABLE [" + schema + @"].[" + table + @"];
    END;';

" + sql;
            }
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private DataColumn GetDataColumn(BaseColumn col)
        {
            var dc = new DataColumn(col.ColumnName)
            {
                Caption = col.ColumnDisplayName,
                AllowDBNull = true,
                ReadOnly = true
            };

            var colType = col.GetType();

            if (colType == typeof(DerivedStringColumn))
            {
                dc.DataType = typeof(string);
            }
            else if (colType == typeof(StringNoWhiteSpaceColumn))
            {
                dc.DataType = typeof(string);
            }
            else if (colType == typeof(StringColumn))
            {
                dc.DataType = typeof(string);
            }
            else if (colType == typeof(IntColumn))
            {
                dc.DataType = typeof(int);
            }
            else if (colType == typeof(DecimalColumn))
            {
                dc.DataType = typeof(decimal);
            }
            else if (colType == typeof(MoneyColumn))
            {
                dc.DataType = typeof(decimal);
            }
            else if (colType == typeof(DateColumn))
            {
                dc.DataType = typeof(DateTime);
                dc.DateTimeMode = DataSetDateTime.Local;
            }

            return dc;
        }
    }
}