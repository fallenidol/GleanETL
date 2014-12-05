using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Gleanio.Core.Columns;

namespace Gleanio.Core.Target
{
    public class DatabaseTableTarget : BaseExtractTarget
    {
        #region Constructors

        public DatabaseTableTarget(string connectionString, string targetTable, string targetSchema = "dbo",
            bool deleteTargetIfExists = true)
            : base(deleteTargetIfExists)
        {
            _table = targetTable.Trim();
            _schema = targetSchema.Trim();

            var csb = new SqlConnectionStringBuilder(connectionString);
            csb.PersistSecurityInfo = true;

            _connectionString = csb.ConnectionString;
        }

        #endregion Constructors

        #region Fields

        private readonly string _connectionString;
        private readonly string _schema;
        private readonly string _table;

        private bool firstTime = true;

        #endregion Fields

        #region Methods

        public override void CommitData(IEnumerable<object[]> dataRows)
        {
            const int batchSize = 10000;

            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();

                if (DeleteIfExists && firstTime)
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

                    var rowId = new DataColumn("BULK_IMPORT_ID", typeof (long));
                    rowId.Unique = true;
                    rowId.AutoIncrementSeed = 1;
                    rowId.AutoIncrementStep = 1;
                    rowId.AutoIncrement = true;
                    rowId.AllowDBNull = false;
                    data.Columns.Add(rowId);
                    rowId.SetOrdinal(ordinal);

                    data.PrimaryKey = new[] {rowId};

                    foreach (var col in Columns)
                    {
                        ordinal++;

                        var dc = GetDataColumn(col);
                        data.Columns.Add(dc);

                        dc.SetOrdinal(ordinal);
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

                                row = iterator.Current;
                            }

                            AddRow(row, data, batchSize, c, true);
                        }
                    }
                }
            }
        }

        private void AddRow(object[] row, DataTable data, int batchSize, SqlConnection c, bool isLastRow)
        {
            var values = new List<object>(row.ToArray());
            values.Insert(0, null);
            if (data.Rows.Count <= batchSize)
            {
                data.Rows.Add(values.ToArray());
            }

            if (data.Rows.Count == batchSize || isLastRow)
            {
                if (firstTime)
                {
                    var schemaSQL = "IF NOT EXISTS (SELECT 'x' FROM sys.schemas WHERE name = N'" + _schema +
                                    "') EXEC sp_executesql N'CREATE SCHEMA [" + _schema + "] AUTHORIZATION [dbo]';";
                    using (var cmd = new SqlCommand(schemaSQL, c))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    var createTableSQL = SqlTableCreator.GetCreateFromDataTableSql(_table, data, _schema);
                    var dropCreateSql =
                        "IF (EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + _schema +
                        "' AND TABLE_NAME = '" + _table + "')) BEGIN DROP TABLE " + _schema + "." + _table + " END; " +
                        createTableSQL + "; ";
                    using (var cmd = new SqlCommand(dropCreateSql, c))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    firstTime = false;
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

        private DataColumn GetDataColumn(BaseColumn col)
        {
            var dc = new DataColumn(col.ColumnName);
            dc.Caption = col.ColumnDisplayName;
            dc.AllowDBNull = true;
            dc.ReadOnly = true;

            var colType = col.GetType();

            if (colType == typeof (StringNoWhitespaceColumn))
            {
                dc.DataType = typeof (string);

                var c = col as StringColumn;
                dc.MaxLength = c.MaxLength;
            }
            else if (colType == typeof (StringColumn))
            {
                dc.DataType = typeof (string);

                var c = col as StringColumn;
                dc.MaxLength = c.MaxLength;
            }
            else if (colType == typeof (IntColumn))
            {
                dc.DataType = typeof (int);
            }
            else if (colType == typeof (DecimalColumn))
            {
                dc.DataType = typeof (decimal);
            }
            else if (colType == typeof (MoneyColumn))
            {
                dc.DataType = typeof (decimal);
            }
            else if (colType == typeof (DateColumn))
            {
                dc.DataType = typeof (DateTime);
                dc.DateTimeMode = DataSetDateTime.Local;
            }

            return dc;
        }

        #endregion Methods
    }
}