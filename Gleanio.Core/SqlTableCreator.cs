﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Gleanio.Core
{
    internal class SqlTableCreator
    {
        #region Fields

        #endregion Fields

        #region Constructors

        public SqlTableCreator(SqlConnection connection)
            : this(connection, null)
        {
        }

        public SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        #endregion Constructors

        #region Properties

        public SqlConnection Connection { get; set; }

        public string DestinationTableName { get; set; }

        public SqlTransaction Transaction { get; set; }

        #endregion Properties

        #region Methods

        public static string GetCreateFromDataTableSql(string tableName, DataTable table, string schema = "dbo")
        {
            var sql = new StringBuilder();

            sql.AppendFormattedLine("CREATE TABLE [{0}].[{1}] (", schema, tableName);

            foreach (DataColumn column in table.Columns)
            {
                sql.AppendFormattedLine("\t[{0}] {1}, ", column.ColumnName, SqlGetType(column));
            }
            sql = sql.Remove(sql.Length - 6, 6);

            if (table.PrimaryKey.Length > 0)
            {
                sql.AppendFormattedLine("\tCONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED (", tableName);

                foreach (var column in table.PrimaryKey)
                {
                    sql.AppendFormattedLine("\t[{0}],", column.ColumnName);
                }

                sql = sql.Remove(sql.Length - 5, 5);
                sql.AppendLine(")");
            }
            sql.AppendLine(")");

            return sql.ToString();
        }

        public static string GetCreateSql(string tableName, DataTable schema, int[] primaryKeys)
        {
            var sql = "CREATE TABLE [" + tableName + "] (\n";

            // columns
            foreach (DataRow column in schema.Rows)
            {
                if (!(schema.Columns.Contains("IsHidden") && (bool) column["IsHidden"]))
                {
                    sql += "\t[" + column["ColumnName"] + "] " + SqlGetType(column);

                    if (schema.Columns.Contains("AllowDBNull") && (bool) column["AllowDBNull"] == false)
                        sql += " NOT NULL";

                    sql += ",\n";
                }
            }
            sql = sql.TrimEnd(',', '\n') + "\n";

            // primary keys
            var pk = ", CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED (";
            var hasKeys = (primaryKeys != null && primaryKeys.Length > 0);
            if (hasKeys)
            {
                // user defined keys
                pk = primaryKeys.Aggregate(pk,
                    (current, key) => current + (schema.Rows[key]["ColumnName"].ToString() + ", "));
            }
            else
            {
                // check schema for keys
                var keys = string.Join(", ", GetPrimaryKeys(schema));
                pk += keys;
                hasKeys = keys.Length > 0;
            }
            pk = pk.TrimEnd(',', ' ', '\n') + ")\n";
            if (hasKeys) sql += pk;

            sql += ")";

            return sql;
        }

        public static string[] GetPrimaryKeys(DataTable schema)
        {
            return (from DataRow column in schema.Rows
                where schema.Columns.Contains("IsKey") && (bool) column["IsKey"]
                select column["ColumnName"].ToString()).ToArray();
        }

        // Return T-SQL data type definition, based on schema definition for a column
        public static string SqlGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    var colSize = columnSize == 0 ? 255 : columnSize;
                    //return "NVARCHAR(" + ((colSize == -1) ? "255" : (colSize > 8000) ? "MAX" : colSize.ToString()) + ")";
                    return "NVARCHAR(MAX)";
                case "System.Decimal":
                    if (numericScale > 0)
                        return "Decimal(17," + numericScale + ")";
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
        public static string SqlGetType(DataRow schemaRow)
        {
            return SqlGetType(schemaRow["DataType"],
                int.Parse(schemaRow["ColumnSize"].ToString()),
                int.Parse(schemaRow["NumericPrecision"].ToString()),
                int.Parse(schemaRow["NumericScale"].ToString()));
        }

        // Overload based on DataColumn from DataTable type
        public static string SqlGetType(DataColumn column)
        {
            return SqlGetType(column.DataType, column.MaxLength, 10, 2);
        }

        public object Create(DataTable schema)
        {
            return Create(schema, null);
        }

        public object Create(DataTable schema, int numKeys)
        {
            var primaryKeys = new int[numKeys];
            for (var i = 0; i < numKeys; i++)
            {
                primaryKeys[i] = i;
            }
            return Create(schema, primaryKeys);
        }

        public object Create(DataTable schema, int[] primaryKeys)
        {
            var sql = GetCreateSql(DestinationTableName, schema, primaryKeys);

            SqlCommand cmd;
            if (Transaction != null && Transaction.Connection != null)
                cmd = new SqlCommand(sql, Connection, Transaction);
            else
                cmd = new SqlCommand(sql, Connection);

            return cmd.ExecuteNonQuery();
        }

        public object CreateFromDataTable(DataTable table)
        {
            var sql = GetCreateFromDataTableSql(DestinationTableName, table);

            SqlCommand cmd;
            if (Transaction != null && Transaction.Connection != null)
                cmd = new SqlCommand(sql, Connection, Transaction);
            else
                cmd = new SqlCommand(sql, Connection);

            return cmd.ExecuteNonQuery();
        }

        #endregion Methods
    }
}