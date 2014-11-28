namespace Gleanio.Core.Target
{
    using Gleanio.Core.Columns;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public class DatabaseTableTarget : BaseExtractTarget
    {
        #region Fields

        private readonly BaseColumn[] _columns;
        private readonly string _connectionString;
        private readonly string _schema;
        private readonly string _table;

        private bool firstTime = true;

        #endregion Fields

        #region Constructors

        public DatabaseTableTarget(string connectionString, string targetTable, string targetSchema = "dbo")
        {
            _table = targetTable.Trim();
            _schema = targetSchema.Trim();

            var csb = new SqlConnectionStringBuilder(connectionString);
            csb.PersistSecurityInfo = true;

            _connectionString = csb.ConnectionString;
        }

        #endregion Constructors

        #region Methods

        public override void CommitData(IEnumerable<object[]> dataRows)
        {
            // TODO USE TAKE/ WHILE
            int totalRows = 0;//rowData.Count();
            const int batchSize = 10000;
            int processed = 0;

            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();

                while (processed < totalRows)
                {
                    using (var data = new DataTable(_table))
                    {
                        int ordinal = 0;

                        var rowId = new DataColumn("BULK_IMPORT_ID", typeof(long));
                        rowId.Unique = true;
                        rowId.AutoIncrementSeed = processed + 1;
                        rowId.AutoIncrementStep = 1;
                        rowId.AutoIncrement = true;
                        rowId.AllowDBNull = false;
                        data.Columns.Add(rowId);
                        rowId.SetOrdinal(ordinal);

                        data.PrimaryKey = new[] { rowId };

                        foreach (BaseColumn col in _columns)
                        {
                            ordinal++;

                            var dc = GetDataColumn(col);
                            data.Columns.Add(dc);

                            dc.SetOrdinal(ordinal);

                        }

                        var batchRows = dataRows.Skip(processed).Take(batchSize);

                        data.BeginLoadData();

                        foreach (IEnumerable<object> row in batchRows)
                        {
                            var values = new List<object>();
                            values.Add(null);
                            values.AddRange(row.ToArray());

                            data.Rows.Add(values.ToArray());
                        }

                        data.EndLoadData();

                        if (firstTime)
                        {
                            string schemaSQL = "IF NOT EXISTS (SELECT 'x' FROM sys.schemas WHERE name = N'" + _schema + "') EXEC sp_executesql N'CREATE SCHEMA [" + _schema + "] AUTHORIZATION [dbo]';";
                            using (var cmd = new SqlCommand(schemaSQL, c))
                            {
                                cmd.ExecuteNonQuery();
                            }

                            string createTableSQL = SqlTableCreator.GetCreateFromDataTableSQL(_table, data, _schema);
                            string dropCreateSql = "IF (EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + _schema + ".' AND TABLE_NAME = '" + _table + "_SUBITEM')) BEGIN DROP TABLE " + _schema + "." + _table + "_SUBITEM END; IF (EXISTS (SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" + _schema + "' AND TABLE_NAME = '" + _table + "')) BEGIN DROP TABLE " + _schema + "." + _table + " END; " + createTableSQL + "; ";
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

                            //Debug.WriteLine("SAVED " + data.Rows.Count + " ROWS TO " + _table);
                            processed += data.Rows.Count;
                        }
                    }
                }
            }
        }

        private DataColumn GetDataColumn(BaseColumn col)
        {
            var dc = new DataColumn(col.ColumnName);
            dc.Caption = col.ColumnDisplayName;
            dc.AllowDBNull = true;
            dc.ReadOnly = true;

            Type colType = col.GetType();

            if (colType == typeof(StringNoWhitespaceColumn))
            {
                dc.DataType = typeof(string);

                var c = col as StringColumn;
                dc.MaxLength = c.MaxLength;
            }
            else if (colType == typeof(StringColumn))
            {
                dc.DataType = typeof(string);

                var c = col as StringColumn;
                dc.MaxLength = c.MaxLength;
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