using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReFind.Data
{
    public static class DbProxy
    {
        private static readonly string dataProvider = ConfigurationManager.AppSettings.Get("Database.DataProvider");
        private static readonly DbProviderFactory _dbFactory = DbProviderFactories.GetFactory(dataProvider);
        private static readonly string connectionStringName = ConfigurationManager.AppSettings.Get("Database.ConnectionStringName");
        private static readonly string _connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

        #region Public Synchronous Database Operations

        public static DataTable Read(string sql, object[] parms, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.SetParameters(parms);

                    using (var adapter = _dbFactory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        connection.Open();

                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        connection.Close();

                        return dataTable;
                    }
                }
            }
        }

        public static T Read<T>(string sql, Func<IDataReader, T> make, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = commandType;
                    command.SetParameters(parms);

                    connection.Open();

                    T t = default(T);
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                        t = make(reader);

                    connection.Close();

                    return t;
                }
            }
        }

        public static List<T> ReadList<T>(string sql, Func<IDataReader, T> make, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = commandType;
                    command.SetParameters(parms);

                    connection.Open();

                    List<T> t = new List<T>();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                        t.Add(make(reader));

                    connection.Close();

                    return t;
                }
            }
        }

        public static int Insert(string sql, object[] parms = null, bool append = false, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.SetParameters(parms);

                    try
                    {
                        command.Parameters["@ID"].Direction = ParameterDirection.InputOutput;
                    }
                    catch { }

                    command.Connection = connection;
                    //if (append)
                    //    command.CommandText = sql.AppendIdentitySelect();
                    //else
                    command.CommandText = sql;
                    connection.Open();

                    command.ExecuteNonQuery();
                    int val = int.Parse(command.Parameters["@ID"].Value.ToString());
                    connection.Close();

                    return val;

                }
            }
        }

        public static void Update(string sql, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.SetParameters(parms);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static void Delete(string sql, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            Update(sql, parms, commandType);
        }

        #endregion

        #region Public Asynchronous Database Operations

        public static Task<DataTable> ReadAsync(string sql, object[] parms, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            return ReadAsyncInternal(sql, parms, commandType);
        }

        public static Task<int> InsertAsync(string sql, object[] parms = null, bool append = false, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            return InsertAsyncInternal(sql, parms, append, commandType);
        }

        public static Task<List<T>> ReadListAsync<T>(string sql, Func<IDataReader, T> make, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            return ReadListAsyncInternal(sql, make, parms, commandType);
        }

        public static Task<T> ReadAsync<T>(string sql, Func<IDataReader, T> make, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            return ReadAsyncInternal(sql, make, parms, commandType);
        }

        public static Task UpdateAsync(string sql, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            return UpdateAsyncInternal(sql, parms, commandType);
        }

        public static Task DeleteAsync(string sql, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (String.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql parameter can not be null or empty.");

            return DeleteAsyncInternal(sql, parms, commandType);
        }

        #endregion

        #region Private Internal Asynchronous Database Operations

        private static async Task<T> ReadAsyncInternal<T>(string sql, Func<IDataReader, T> make, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = commandType;
                    command.SetParameters(parms);

                    await connection.OpenAsync();

                    T t = default(T);

                    var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                        t = make(reader);

                    connection.Close();

                    return t;
                }
            }
        }

        private static async Task<DataTable> ReadAsyncInternal(string sql, object[] parms, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.SetParameters(parms);

                    using (var adapter = _dbFactory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;

                        await connection.OpenAsync();

                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        connection.Close();

                        return dataTable;
                    }
                }
            }
        }

        private static async Task<List<T>> ReadListAsyncInternal<T>(string sql, Func<IDataReader, T> make, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    command.CommandType = commandType;
                    command.SetParameters(parms);

                    connection.Open();

                    List<T> t = new List<T>();
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        t.Add(make(reader));

                    connection.Close();

                    return t;
                }
            }
        }

        private static async Task<int> InsertAsyncInternal(string sql, object[] parms = null, bool append = false, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.CommandType = commandType;
                    command.SetParameters(parms);

                    command.Connection = connection;
                    //if (append)
                    //    command.CommandText = sql.AppendIdentitySelect();
                    //else
                    command.CommandText = sql;
                    await connection.OpenAsync();

                    await command.ExecuteNonQueryAsync();
                    int val = int.Parse(command.Parameters["@ID"].Value.ToString());
                    connection.Close();

                    return val;

                }
            }
        }

        private static async Task UpdateAsyncInternal(string sql, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = _dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                using (var command = _dbFactory.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = commandType;
                    command.Connection = connection;
                    command.SetParameters(parms);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                }
            }
        }

        private static async Task DeleteAsyncInternal(string sql, object[] parms = null, CommandType commandType = CommandType.StoredProcedure)
        {
            await UpdateAsync(sql, parms, commandType);
        }

        #endregion
    }
}   
