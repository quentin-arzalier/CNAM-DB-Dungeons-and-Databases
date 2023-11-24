using Dapper;
using Dungeons_Databases.Models;
using Npgsql;
using System;

namespace Dungeons_Databases.Data
{
    public class DatabaseService
    {
        private readonly IConfiguration _config;

        public DatabaseService(IConfiguration config)
        {
            _config = config;
        }

        private NpgsqlConnection GetConnection() => new(_config.GetConnectionString("postgres"));

        public T? Get<T>(string query, object? param = null)
        {
            try
			{
				using var conn = GetConnection();
				var item = conn.QuerySingle<T>(query, param);
				return item;
			}
            catch (Exception)
            {
				return default;
            }
        }
        public List<T> GetList<T>(string query, object? param = null)
        {
            using var conn = GetConnection();
            var items = conn.Query<T>(query, param);
            return items.ToList();
        }

        public int ExecuteSql(string query, object? param = null)
        {
            using var conn = GetConnection();
            return conn.Execute(query, param);
		}

        /// <param name="query">Must end in RETURNING generated_id</param>
		public int ExecuteInsert(string query, object? param = null)
		{
			using var conn = GetConnection();
			return conn.ExecuteScalar<int>(query, param);
		}

        public T ExecuteScalar<T>(string query, object? param = null)
        {
            using var conn = GetConnection();
            return conn.ExecuteScalar<T>(query, param);
        }
	}
}
