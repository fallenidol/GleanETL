using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Gleanio.Core.Columns;

namespace Gleanio.Core.Target
{
    public class DatabaseTableTarget : BaseExtractTarget
    {
        public override string ToString()
        {
            var sb = new SqlConnectionStringBuilder(this._connectionString);
            return string.Format("{0}.{1}.{2}", sb.DataSource, sb.InitialCatalog, _table);
        }

        #region Constructors

        public DatabaseTableTarget(string connectionString, string targetTable, string targetSchema = "dbo",
            bool deleteTargetIfExists = false)
            : base(deleteTargetIfExists)
        {
            _table = targetTable.Trim();
            _schema = targetSchema.Trim();

            var csb = new SqlConnectionStringBuilder(connectionString) { PersistSecurityInfo = true };

            _connectionString = csb.ConnectionString;
        }

        #endregion Constructors

        #region Fields

        private readonly string _connectionString;
        private readonly string _schema;
        private readonly string _table;

        private bool _firstTime = true;

        #endregion Fields

        #region Methods

        public override long CommitData(IEnumerable<object[]> dataRows)
        {
            const int batchSize = 10000;
            long lineCount = 0;

            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();

                if (DeleteIfExists && _firstTime)
                {
                    using (
                        var cmd =
                            new SqlCommand(
                                string.Format(
                                    "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{1}' and TABLE_SCHEMA='{0}') TRUNCATE TABLE {0}.{1};",
                                    _schema, _table), c))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (var data = new DataTable(_table))
                {
                    var ordinal = 0;

                    var rowId = new DataColumn("BULK_IMPORT_ID", typeof(long))
                    {
                        Unique = true,
                        AutoIncrementSeed = 1,
                        AutoIncrementStep = 1,
                        AutoIncrement = true,
                        AllowDBNull = false
                    };

                    if (!DeleteIfExists)
                    {
                        using (var cmd = new SqlCommand(string.Format("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='{1}' AND TABLE_SCHEMA='{0}') SELECT Coalesce(MAX(BULK_IMPORT_ID),0)+1 FROM [{0}].[{1}]; ELSE SELECT 1;", _schema, _table), c))
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
                                AddRow(row, data, batchSize, c, false);
                                lineCount++;

                                row = iterator.Current;
                            }

                            AddRow(row, data, batchSize, c, true);
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
                        foreach (var column in (Columns.OfType<BaseColumn>().Where(bc => bc is StringColumn || bc is DerivedStringColumn)))
                        {
                            int length = 255;
                            if (column is StringColumn)
                            {
                                var sc = column as StringColumn;
                                length = sc.DetectedMaxLength <= 0 ? sc.MaxLength <= 0 ? 255 : sc.MaxLength : sc.DetectedMaxLength;
                            }
                            else if (column is DerivedStringColumn)
                            {
                                var sc = column as DerivedStringColumn;
                                length = sc.DetectedMaxLength <= 0 ? 255 : sc.DetectedMaxLength;
                            }

                            if (DeleteIfExists)
                            {
                                sqlBuilder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] nvarchar({3});", _schema, _table, column.ColumnName, length));
                            }
                            else
                            {
                                sqlBuilder.AppendLine(string.Format("IF ((SELECT MAX(LEN([{2}])) FROM [{0}].[{1}]) < {3}) ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] nvarchar({3});", _schema, _table, column.ColumnName, length));
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
            var valuesWithoutIgnoredColumns = ValuesWithoutIgnoredColumns(row);

            var values = (new object[] { null }).Concat(valuesWithoutIgnoredColumns);

            if (data.Rows.Count <= batchSize)
            {
                data.Rows.Add(values.ToArray());
            }

            if (data.Rows.Count == batchSize || isLastRow)
            {
                if (_firstTime)
                {
                    CreateSchema(data, c);

                    _firstTime = false;
                }

                using (var sbc = new SqlBulkCopy(c.ConnectionString, SqlBulkCopyOptions.TableLock))
                {
                    sbc.DestinationTableName = _schema + "." + _table;
                    sbc.WriteToServer(data);
                }

                data.EndLoadData();
                data.Clear();
                data.Columns[0].AutoIncrementSeed += batchSize;
            }
        }

        private void CreateSchema(DataTable data, SqlConnection c)
        {
            var schemaSql = "IF NOT EXISTS (SELECT 'x' FROM sys.schemas WHERE name = N'" + _schema +
                            "') EXEC sp_executesql N'CREATE SCHEMA [" + _schema + "] AUTHORIZATION [dbo]';";
            using (var cmd = new SqlCommand(schemaSql, c))
            {
                cmd.ExecuteNonQuery();
            }

            var createTableSql = SqlTableCreator.GetCreateFromDataTableSql(_table, data, _schema);
            var sql = createTableSql + "; ";
            if (DeleteIfExists)
            {
                sql = "IF (EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + _schema +
                      "' AND TABLE_NAME = '" + _table + "')) BEGIN DROP TABLE " + _schema + "." + _table + " END; " + sql;
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
            else if (colType == typeof(StringNoWhitespaceColumn))
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

        #endregion Methods
    }
}