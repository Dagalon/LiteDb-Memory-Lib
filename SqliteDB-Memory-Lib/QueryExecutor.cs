using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SqliteDB_Memory_Lib
{
    /// <summary>
    /// Executes SQLite commands and queries while providing helper utilities for parameter handling.
    /// </summary>
    public static class QueryExecutor
    {

        /// <summary>
        /// Executes an INSERT operation using the provided fields and values within a transaction.
        /// </summary>
        /// <param name="db">Connection used to execute the command.</param>
        /// <param name="idDataBase">Identifier of the database that contains the target table.</param>
        /// <param name="idTable">Name of the target table.</param>
        /// <param name="fields">List of column names participating in the insert.</param>
        /// <param name="values">Matrix containing the rows to insert.</param>
        /// <param name="extraEnd">Optional SQL clause appended to the command.</param>
        public static void Insert(SqliteConnection db, string idDataBase, string idTable, List<string> fields,
            object[,] values, string extraEnd)
        {
         
            var filteredFields =
                fields.Select((field, i) => Regex.Replace(field, @"[^0-9-a-zA-Z]" , "").Replace("\"", "").Replace(" ", "")).ToList();
            var insertParameters = string.Join(",", filteredFields.Select((field, i) => "@" + field).ToList());

            var qry = !string.IsNullOrEmpty(extraEnd)
                ? $"INSERT INTO {idDataBase}.{idTable} ({string.Join(",", fields)}) VALUES ({insertParameters}) {extraEnd}"
                : $"INSERT INTO {idDataBase}.{idTable} ({string.Join(",", fields)}) VALUES ({insertParameters})";

            lock (db)
            {
                var transaction = db.BeginTransaction();
                var noRows = values.GetLength(0);
                var noColumns = values.GetLength(1);

                var cmd = db.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = qry;

                try
                {
                    for (var i = 0; i < noRows; i++)
                    {
                        cmd.CommandText = qry;
                        var row = values.SubArrayToList(i, i + 1, 0, noColumns)[0];
                        var jumpRow = row.Count(x => string.IsNullOrEmpty(x.ToString())) ==
                                           noColumns;

                        if (!jumpRow)
                        {
                            for (var j = 0; j < noColumns; j++)
                            {
                                cmd.Parameters.AddWithValue(filteredFields[j], values[i, j]);
                            }

                            cmd.ExecuteNonQuery();
                        }
                        
                        cmd.Parameters.Clear();
                    }

                    transaction.Commit();
                    cmd.Dispose();
                    transaction.Dispose();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    throw new Exception(ex.Message);
                }
            }
        }


        /// <summary>
        /// Creates a new table using the provided headers and optional column types.
        /// </summary>
        /// <param name="db">Connection used to execute the command.</param>
        /// <param name="idDataBase">Identifier of the database that will host the table.</param>
        /// <param name="idTable">Name of the table to create.</param>
        /// <param name="headers">List of column definitions.</param>
        /// <param name="types">Optional list of .NET types used to infer SQLite types.</param>
        public static void CreateTable(SqliteConnection db, string idDataBase, string idTable,
            List<string> headers, List<Type>? types = null)
        {

            var fieldsDefinition = new List<string>();
            var noFields = headers.Count;

            if (types is null)
            {
                for (var i = 0; i < noFields; i++)
                {
                    fieldsDefinition.Add(headers[i]);
                }
               
            }
            else
            {
                for (var i = 0; i < noFields; i++)
                {
                    var t = NetTypeToSqLiteType.GetDbType(types[i]);
                    fieldsDefinition.Add(headers[i] + " " + t);
                }
               
            }

            var qry =
                $"CREATE TABLE IF NOT EXISTS {idDataBase}.{idTable}({string.Join(",", fieldsDefinition)})";
            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();

        }

        /// <summary>
        /// Executes a SELECT statement using the provided clauses and returns the result set.
        /// </summary>
        /// <param name="db">Connection used to execute the query.</param>
        /// <param name="idDataBase">Identifier of the database that contains the table.</param>
        /// <param name="idTable">Name of the table to query.</param>
        /// <param name="select">Columns to retrieve.</param>
        /// <param name="where">WHERE clause applied to the query.</param>
        /// <param name="groupBy">GROUP BY clause applied to the query.</param>
        /// <param name="orderBy">ORDER BY clause applied to the query.</param>
        /// <returns>Result rows represented as dictionaries or <c>null</c> when none exist.</returns>
        public static List<Dictionary<string, object>>? Select(SqliteConnection db, string idDataBase, string idTable,
                                                              string select, string where, string groupBy, string orderBy)
        {
            var qry = $"SELECT {select} FROM {idDataBase}.{idTable} WHERE {where} GROUP BY {groupBy} ORDER BY {orderBy}";

            if (string.IsNullOrEmpty(where))
            {
                qry = qry.Replace("WHERE", "");
            }

            if (string.IsNullOrEmpty(groupBy))
            {
                qry = qry.Replace("GROUP BY", "");
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                qry = qry.Replace("ORDER BY", "");
            }

            var cmd = new SqliteCommand(qry, db);
            var qryResult = cmd.ExecuteReader();
            var resultList = new List<Dictionary<string, object>>();

            if (qryResult.HasRows)
            {
                while (qryResult.Read())
                {
                    resultList.Add(Enumerable.Range(0, qryResult.FieldCount)
                        .ToDictionary(qryResult.GetName, qryResult.GetValue));
                }

                qryResult.Close();
                return resultList;
            }

            qryResult.Close();
            return null;

        }

        /// <summary>
        /// Executes a SQL command that does not return a result set.
        /// </summary>
        /// <param name="db">Connection used to execute the command.</param>
        /// <param name="qry">SQL command text.</param>
        public static void ExecuteQryNotReader(SqliteConnection db, string qry)
        {
            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQL command after replacing placeholders with the provided parameters.
        /// </summary>
        /// <param name="db">Connection used to execute the command.</param>
        /// <param name="qry">SQL command text containing placeholders.</param>
        /// <param name="parameters">Dictionary with parameter replacements.</param>
        public static void ExecuteQryNotReader(SqliteConnection db, string qry, Dictionary<string, string> parameters)
        {
            qry = parameters.Keys.Aggregate(qry, (current, param) => current.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase));

            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQL query and returns the results as dictionaries.
        /// </summary>
        /// <param name="db">Connection used to execute the query.</param>
        /// <param name="qry">SQL query string.</param>
        /// <returns>List of result rows represented as dictionaries.</returns>
        public static List<Dictionary<string, object>> ExecuteQryReader(SqliteConnection db, string qry)
        {
            var cmd = new SqliteCommand(qry, db);
            var qryResult = cmd.ExecuteReader();

            var resultList = new List<Dictionary<string, object>>();

            if (qryResult.HasRows)
            {
                while (qryResult.Read())
                {
                    resultList.Add(Enumerable.Range(0, qryResult.FieldCount)
                        .ToDictionary(qryResult.GetName, qryResult.GetValue));
                }

            }

            qryResult.Close();
            return resultList;
        }

        /// <summary>
        /// Executes a SQL query with parameter substitutions and returns the results as dictionaries.
        /// </summary>
        /// <param name="db">Connection used to execute the query.</param>
        /// <param name="qry">SQL query string containing placeholders.</param>
        /// <param name="parameters">Dictionary with parameter replacements.</param>
        /// <returns>List of result rows represented as dictionaries.</returns>
        public static List<Dictionary<string, object>> ExecuteQryReader(SqliteConnection db, string qry, Dictionary<string, string> parameters)
        {
            qry = parameters.Keys.Aggregate(qry, (current, param) => current.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase));

            return ExecuteQryReader(db, qry);
        }
    }
}

