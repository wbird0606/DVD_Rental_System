using Npgsql;

public static class PostgreSqlConnection
{
    private static readonly string _connectionString = "Host=192.168.62.137;Port=5432;Username=postgres;Password=1qaz@WSX3edc;Database=dvdrental";

    public static  NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
