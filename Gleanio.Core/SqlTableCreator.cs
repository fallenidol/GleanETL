﻿namespace Gleanio.Core
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    internal class SqlTableCreator
    {
        #region Fields

        private SqlConnection _connection;
        private string _tableName;
        private SqlTransaction _transaction;

        #endregion Fields

        #region Constructors

        public SqlTableCreator(SqlConnection connection)
            : this(connection, null)
        {
        }

        public SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        #endregion Constructors

        #region Properties

        public SqlConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        public string DestinationTableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public SqlTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        #endregion Properties

        #region Methods

        public static string GetCreateFromDataTableSql(string tableName, DataTable table, string schema = "dbo")
        {
            var sql = new StringBuilder();

            sql.AppendFormattedLine("CREATE TABLE [{0}].[{1}] (", schema, tableName);

            foreach (DataColumn column in table.Columns)
            {
                sql.AppendFormattedLine("\t[{0}] {1}, ", column.ColumnName, SQLGetType(column));
            }
            sql = sql.Remove(sql.Length - 6, 6);

            if (table.PrimaryKey.Length > 0)
            {
                sql.AppendFormattedLine("\tCONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED (", tableName);

                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql.AppendFormattedLine("\t[{0}],", column.ColumnName);
                }

                sql = sql.Remove(sql.Length -5, 5);
                sql.AppendLine(")");
            }
            sql.AppendLine(")");

            return sql.ToString();
        }

        public static string GetCreateSql(string tableName, DataTable schema, int[] primaryKeys)
        {
            string sql = "CREATE TABLE [" + tableName + "] (\n";

            // columns
            foreach (DataRow column in schema.Rows)
            {
                if (!(schema.Columns.Contains("IsHidden") && (bool)column["IsHidden"]))
                {
                    sql += "\t[" + column["ColumnName"] + "] " + SQLGetType(column);

                    if (schema.Columns.Contains("AllowDBNull") && (bool)column["AllowDBNull"] == false)
                        sql += " NOT NULL";

                    sql += ",\n";
                }
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";

            // primary keys
            string pk = ", CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED (";
            bool hasKeys = (primaryKeys != null && primaryKeys.Length > 0);
            if (hasKeys)
            {
                // user defined keys
                pk = primaryKeys.Aggregate(pk, (current, key) => current + (schema.Rows[key]["ColumnName"].ToString() + ", "));
            }
            else
            {
                // check schema for keys
                string keys = string.Join(", ", GetPrimaryKeys(schema));
                pk += keys;
                hasKeys = keys.Length > 0;
            }
            pk = pk.TrimEnd(new char[] { ',', ' ', '\n' }) + ")\n";
            if (hasKeys) sql += pk;

            sql += ")";

            return sql;
        }

        public static string[] GetPrimaryKeys(DataTable schema)
        {
            return (from DataRow column in schema.Rows where schema.Columns.Contains("IsKey") && (bool)column["IsKey"] select column["ColumnName"].ToString()).ToArray();
        }

        // Return T-SQL data type definition, based on schema definition for a column
        public static string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    int colSize = columnSize == 0 ? 255 : columnSize;
                    return "VARCHAR(" + ((colSize == -1) ? "255" : (colSize > 8000) ? "MAX" : colSize.ToString()) + ")";

                case "System.Decimal":
                    if (numericScale > 0)
                        return "Decimal(17," + numericScale + ")";
                    else
                        return "Decimal";

                case "System.Double":
                case "System.Single":
                    return "Decimal";

                case "System.Int64":
                    return "BIGINT";

                case "System.Int16":
                case "System.Int32":
                    return "INT";

                case "System.DateTime":
                    return "DATETIME";

                case "System.Boolean":
                    return "BIT";

                case "System.Byte":
                    return "TINYINT";

                case "System.Guid":
                    return "UNIQUEIDENTIFIER";

                default:
                    throw new Exception(type + " not implemented.");
            }
        }

        // Overload based on row from schema table
        public static string SQLGetType(DataRow schemaRow)
        {
            return SQLGetType(schemaRow["DataType"],
                                int.Parse(schemaRow["ColumnSize"].ToString()),
                                int.Parse(schemaRow["NumericPrecision"].ToString()),
                                int.Parse(schemaRow["NumericScale"].ToString()));
        }

        // Overload based on DataColumn from DataTable type
        public static string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2);
        }

        public object Create(DataTable schema)
        {
            return Create(schema, null);
        }

        public object Create(DataTable schema, int numKeys)
        {
            var primaryKeys = new int[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                primaryKeys[i] = i;
            }
            return Create(schema, primaryKeys);
        }

        public object Create(DataTable schema, int[] primaryKeys)
        {
            string sql = GetCreateSql(_tableName, schema, primaryKeys);

            SqlCommand cmd;
            if (_transaction != null && _transaction.Connection != null)
                cmd = new SqlCommand(sql, _connection, _transaction);
            else
                cmd = new SqlCommand(sql, _connection);

            return cmd.ExecuteNonQuery();
        }

        public object CreateFromDataTable(DataTable table)
        {
            string sql = GetCreateFromDataTableSql(_tableName, table);

            SqlCommand cmd;
            if (_transaction != null && _transaction.Connection != null)
                cmd = new SqlCommand(sql, _connection, _transaction);
            else
                cmd = new SqlCommand(sql, _connection);

            return cmd.ExecuteNonQuery();
        }

        #endregion Methods
    }
}