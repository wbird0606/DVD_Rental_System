using System;
using System.Collections.Generic;
using Npgsql;
using System.Configuration;

namespace DVD_Rental.Helpers
{
    public class SqlHelper : IDisposable
    {
        private readonly NpgsqlConnection _conn;
        private readonly NpgsqlTransaction _tx;
        private bool _disposed = false;

        public SqlHelper(bool useTransaction = false)
        {
            //_conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString);
            _conn = PostgreSqlConnection.GetConnection();
            _conn.Open();

            if (useTransaction)
                _tx = _conn.BeginTransaction();
        }

        // 查詢多筆
        public List<T> ExecuteQuery<T>(string sql, Action<NpgsqlParameterCollection> paramBuilder, Func<NpgsqlDataReader, T> rowMapper)
        {
            using (var cmd = new NpgsqlCommand(sql, _conn, _tx))
            {
                paramBuilder?.Invoke(cmd.Parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    var list = new List<T>();
                    while (reader.Read())
                    {
                        list.Add(rowMapper(reader));
                    }
                    return list;
                }
            }
        }

        // 查詢單筆（或 null）
        public T ExecuteSingle<T>(string sql, Action<NpgsqlParameterCollection> paramBuilder, Func<NpgsqlDataReader, T> rowMapper)
        {
            using (var cmd = new NpgsqlCommand(sql, _conn, _tx))
            {
                paramBuilder?.Invoke(cmd.Parameters);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return rowMapper(reader);
                    return default;
                }
            }
        }

        // 執行新增/刪除/修改
        public int ExecuteNonQuery(string sql, Action<NpgsqlParameterCollection> paramBuilder)
        {
            using (var cmd = new NpgsqlCommand(sql, _conn, _tx))
            {
                paramBuilder?.Invoke(cmd.Parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        // 執行單值查詢
        public T ExecuteScalar<T>(string sql, Action<NpgsqlParameterCollection> paramBuilder)
        {
            using (var cmd = new NpgsqlCommand(sql, _conn, _tx))
            {
                paramBuilder?.Invoke(cmd.Parameters);
                object result = cmd.ExecuteScalar();

                if (result == null || result is DBNull)
                    return default;

                // 嘗試強制轉型
                try
                {
                    return (T)Convert.ChangeType(result, typeof(T));
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"無法將結果轉型為 {typeof(T).Name}，實際值為 {result}，錯誤: {ex.Message}", ex);
                }
            }
        }


        // 提交交易（若有）
        public void Commit()
        {
            _tx?.Commit();
        }

        // 回滾交易（若有）
        public void Rollback()
        {
            _tx?.Rollback();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _tx?.Dispose();
                _conn?.Close();
                _conn?.Dispose();
                _disposed = true;
            }
        }
    }
}
