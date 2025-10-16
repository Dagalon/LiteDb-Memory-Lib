using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SqliteDB_Memory_Lib
{
    /// <summary>
    /// Low-level helpers that execute SQL statements against SQLite connections.
    /// </summary>
    public static class QueryExecutor
    {

        /// <summary>
        /// Inserts multiple rows into the specified table by using parameterized statements.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="idDataBase">Alias of the database that contains the table.</param>
        /// <param name="idTable">Name of the table to insert into.</param>
        /// <param name="fields">Ordered list of column names.</param>
        /// <param name="values">Matrix that contains the values to insert.</param>
        /// <param name="extraEnd">Additional SQL appended to the INSERT statement.</param>
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
        /// Creates a table with the provided column definitions.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="idDataBase">Alias of the database that will contain the table.</param>
        /// <param name="idTable">Name of the table to create.</param>
        /// <param name="headers">Column definitions or names.</param>
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
        /// Executes a SELECT statement built from the supplied clauses.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="idDataBase">Alias of the database that contains the table.</param>
        /// <param name="idTable">Name of the table to query.</param>
        /// <param name="select">Columns to select.</param>
        /// <param name="where">WHERE clause without the keyword.</param>
        /// <param name="groupBy">GROUP BY clause without the keyword.</param>
        /// <param name="orderBy">ORDER BY clause without the keyword.</param>
        /// <returns>A list with the resulting rows or <c>null</c> when nothing is returned.</returns>
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
        /// Executes a SQL statement that does not return rows.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="qry">SQL statement to execute.</param>
        public static void ExecuteQryNotReader(SqliteConnection db, string qry)
        {
            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a parameterized SQL statement that does not return rows.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="qry">SQL statement that contains placeholders.</param>
        /// <param name="parameters">Parameter values used to replace placeholders.</param>
        public static void ExecuteQryNotReader(SqliteConnection db, string qry, Dictionary<string, string> parameters)
        {
            qry = parameters.Keys.Aggregate(qry, (current, param) => current.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase));

            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a SQL statement and returns the resulting rows.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="qry">SQL statement to execute.</param>
        /// <returns>List of rows represented as dictionaries.</returns>
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
        /// Executes a parameterized SQL statement and returns the resulting rows.
        /// </summary>
        /// <param name="db">Target SQLite connection.</param>
        /// <param name="qry">SQL statement that contains placeholders.</param>
        /// <param name="parameters">Parameter values used to replace placeholders.</param>
        /// <returns>List of rows represented as dictionaries.</returns>
        public static List<Dictionary<string, object>> ExecuteQryReader(SqliteConnection db, string qry, Dictionary<string, string> parameters)
        {
            qry = parameters.Keys.Aggregate(qry, (current, param) => current.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase));

            return ExecuteQryReader(db, qry);
        }
    }
}

