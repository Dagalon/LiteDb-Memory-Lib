using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SqliteDB_Memory_Lib
{
    public static class QueryExecutor
    {

        public static void Insert(SqliteConnection db, string idDataBase, string idTable, List<string> fields,
            object[,] values, string extraEnd)
        {
         
            List<string> filteredFields =
                fields.Select((field, i) => Regex.Replace(field, @"[^0-9-a-zA-Z]" , "").Replace("\"", "").Replace(" ", "")).ToList();
            string insertParameters = string.Join(",", filteredFields.Select((field, i) => "@" + field).ToList());

            string qry = !string.IsNullOrEmpty(extraEnd)
                ? $"INSERT INTO {idDataBase}.{idTable} ({string.Join(",", fields)}) VALUES ({insertParameters}) {extraEnd}"
                : $"INSERT INTO {idDataBase}.{idTable} ({string.Join(",", fields)}) VALUES ({insertParameters})";

            lock (db)
            {
                var transaction = db.BeginTransaction();
                int noRows = values.GetLength(0);
                int noColumns = values.GetLength(1);

                SqliteCommand cmd = db.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = qry;

                try
                {
                    for (int i = 0; i < noRows; i++)
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


        public static void CreateTable(SqliteConnection db, string idDataBase, string idTable,
            List<string> headers, List<Type>? types = null)
        {

            var fieldsDefinition = new List<string>();

           
            int noFields = headers.Count;

            if (types is null)
            {
                for (int i = 0; i < noFields; i++)
                {
                    fieldsDefinition.Add(headers[i]);
                }
               
            }
            else
            {
                for (int i = 0; i < noFields; i++)
                {
                    var t = NetTypeToSqLiteType.GetDBType(types[i]);
                    fieldsDefinition.Add(headers[i] + " " + t);
                }
               
            }

            var qry =
                $"CREATE TABLE IF NOT EXISTS {idDataBase}.{idTable}({string.Join(",", fieldsDefinition)})";
            var cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();

        }

        public static List<Dictionary<string, object>>? Select(SqliteConnection db, string idDataBase, string idTable,
                                                              string select, string where, string groupBy, string orderBy)
        {
            string qry = $"SELECT {select} FROM {idDataBase}.{idTable} WHERE {where} GROUP BY {groupBy} ORDER BY {orderBy}";

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

            SqliteCommand cmd = new SqliteCommand(qry, db);
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

        public static void ExecuteQryNotReader(SqliteConnection db, string qry)
        {
            SqliteCommand cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();
        }

        public static void ExecuteQryNotReader(SqliteConnection db, string qry, Dictionary<string, string> parameters)
        {
            foreach (var param in parameters.Keys)
            {
                qry = qry.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase);
            }
           
            SqliteCommand cmd = new SqliteCommand(qry, db);
            cmd.ExecuteNonQuery();
        }

        public static List<Dictionary<string, object>> ExecuteQryReader(SqliteConnection db, string qry)
        {
            SqliteCommand cmd = new SqliteCommand(qry, db);
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
            return resultList;
        }

        public static List<Dictionary<string, object>> ExecuteQryReader(SqliteConnection db, string qry, Dictionary<string, string> parameters)
        {
            foreach (var param in parameters.Keys)
            {
                qry = qry.Replace(param, parameters[param], StringComparison.OrdinalIgnoreCase);
                // qry = Regex.Replace(qry, param, parameters[param], RegexOptions.IgnoreCase);
            }

            return ExecuteQryReader(db, qry);
        }
    }
}

