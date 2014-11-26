using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using gleanio.framework.Columns;

namespace gleanio.framework.Target
{
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

        public DatabaseTableTarget(BaseColumn[] columns, string server, string database, string table, string schema = "dbo", string userId = null, string password = null)
        {
            _table = table.Trim();
            _columns = columns;
            _schema = schema;

            var csb = new SqlConnectionStringBuilder();
            csb.PersistSecurityInfo = true;
            csb.ApplicationName = "FileImportFramework";
            csb.DataSource = server;
            csb.InitialCatalog = database;

            csb.IntegratedSecurity = userId == null && password == null;
            if (!csb.IntegratedSecurity)
            {
                csb.UserID = userId;
                csb.Password = password;
            }
            csb.WorkstationID = Environment.MachineName;

            _connectionString = csb.ConnectionString;
        }

        #endregion Constructors

        #region Methods

        public override void SaveRows(ICollection<ICollection<object>> rowData)
        {
            int totalRows = rowData.Count;
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

                        data.PrimaryKey = new DataColumn[] { rowId };

                        foreach (BaseColumn col in _columns)
                        {
                            ordinal++;

                            var dc = Helper.GetDataColumn(col);
                            data.Columns.Add(dc);

                            dc.SetOrdinal(ordinal);

                        }

                        var batchRows = rowData.Skip(processed).Take(batchSize);

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

        #endregion Methods
    }
}