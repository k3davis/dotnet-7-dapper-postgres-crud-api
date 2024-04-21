namespace WebApi.Helpers;

using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class DataContext(IOptions<DbSettings> dbSettings)
{
    private readonly DbSettings _dbSettings = dbSettings.Value;

    public IDbConnection CreateConnection()
    {
        var connectionString = $"Host={_dbSettings.Server}; Database={_dbSettings.Database}; Username={_dbSettings.UserId}; Password={_dbSettings.Password};";
        return new NpgsqlConnection(connectionString);
    }

    public async Task Init()
    {
        await InitDatabase();
        await InitTables();
    }

    private async Task InitDatabase()
    {
        // create database if it doesn't exist
        var connectionString = $"Host={_dbSettings.Server}; Database=postgres; Username={_dbSettings.UserId}; Password={_dbSettings.Password};";
        using var connection = new NpgsqlConnection(connectionString);
        var sqlDbCount = $"SELECT COUNT(*) FROM pg_database WHERE datname = '{_dbSettings.Database}';";
        var dbCount = await connection.ExecuteScalarAsync<int>(sqlDbCount);
        if (dbCount == 0)
        {
            var sql = $"CREATE DATABASE \"{_dbSettings.Database}\"";
            await connection.ExecuteAsync(sql);
        }
    }

    private async Task InitTables()
    {
        // create tables if they don't exist
        using var connection = CreateConnection();
        await _initUsers();

        async Task _initUsers()
        {
            var sql = """
                CREATE TABLE IF NOT EXISTS Users (
                    Id SERIAL PRIMARY KEY,
                    Title VARCHAR,
                    FirstName VARCHAR,
                    LastName VARCHAR,
                    Email VARCHAR,
                    Role INTEGER,
                    PasswordHash VARCHAR
                );
            """;
            await connection.ExecuteAsync(sql);
        }
    }
}